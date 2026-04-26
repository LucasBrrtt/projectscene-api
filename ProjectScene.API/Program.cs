using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectScene.Application;
using ProjectScene.Application.Options;
using ProjectScene.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Expoe a documentacao da API e o suporte ao token Bearer no Swagger.
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectScene API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT assim: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    // Libera chamadas de qualquer origem para facilitar integracao durante o desenvolvimento.
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Registra os servicos da aplicacao e da infraestrutura.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Falha na inicializacao se a configuracao essencial do JWT estiver ausente.
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions is null ||
    string.IsNullOrWhiteSpace(jwtOptions.Key) ||
    string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException("A secao Jwt da configuracao esta ausente ou incompleta.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);

// Configura a validacao do token usado nas rotas protegidas.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Esses parametros definem quando um token sera aceito pela API.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Centraliza o tratamento de excecoes antes de chegar no controller.
app.UseMiddleware<ProjectScene.API.Middleware.ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");

// Authentication identifica o usuario; Authorization aplica as regras de acesso.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
