using ProjectScene.Application;
using ProjectScene.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Controllers
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (Permitir frontend acessar sua API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Injeta dependências das camadas
builder.Services.AddApplication();      // Vai criar isso na camada Application
builder.Services.AddInfrastructure(builder.Configuration);  // Vai criar na Infrastructure

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

app.MapControllers();

app.Run();