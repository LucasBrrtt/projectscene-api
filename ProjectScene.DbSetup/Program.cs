using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectScene.Infrastructure.Data;

var resetDatabase = args.Any(arg => arg.Equals("--reset", StringComparison.OrdinalIgnoreCase));
const string apiUserSecretsId = "53bcdf24-91ed-4710-8c2c-cf993414b6f9";

// Resolve a raiz do repositório para reutilizar arquivos de configuração da API.
var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var apiProjectPath = Path.Combine(basePath, "ProjectScene.API");

// Lê o mesmo User Secrets da API para manter uma única origem da connection string local.
var userSecretsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Microsoft",
    "UserSecrets",
    apiUserSecretsId,
    "secrets.json");

var configuration = new ConfigurationBuilder()
    .SetBasePath(apiProjectPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    // Mantem a mesma ideia de configuracao local usada pela API.
    .AddJsonFile(userSecretsPath, optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A connection string 'DefaultConnection' nao foi encontrada.");
}

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString)
    .Options;

// Reaproveita exatamente o mesmo modelo EF da aplicação para criar o schema local.
await using var dbContext = new AppDbContext(options);

if (resetDatabase)
{
    // Apaga o banco atual antes de recriar a estrutura do zero.
    await dbContext.Database.EnsureDeletedAsync();
}

// Cria banco e tabelas com base no modelo atual do DbContext.
// O schema gerado depende das restricoes declaradas no modelo EF.
await dbContext.Database.EnsureCreatedAsync();

// Atualiza bancos locais antigos com as colunas de refresh token adicionadas depois do schema inicial.
await dbContext.Database.ExecuteSqlRawAsync("""
    ALTER TABLE users
        ADD COLUMN IF NOT EXISTS refresh_token VARCHAR(255) NULL;
    """);

await dbContext.Database.ExecuteSqlRawAsync("""
    ALTER TABLE users
        ADD COLUMN IF NOT EXISTS refresh_token_expiry TIMESTAMP NULL;
    """);

await dbContext.Database.ExecuteSqlRawAsync("""
    ALTER TABLE users
        ADD COLUMN IF NOT EXISTS refresh_token_persistent BOOLEAN NULL;
    """);

// Garante indices unicos tambem em bancos locais que ja existiam antes dessa configuracao.
// Isso evita depender apenas do EnsureCreated em ambientes que nasceram com um modelo mais antigo.
await dbContext.Database.ExecuteSqlRawAsync("""
    CREATE UNIQUE INDEX IF NOT EXISTS ux_users_email
        ON users (email);
    """);

await dbContext.Database.ExecuteSqlRawAsync("""
    CREATE UNIQUE INDEX IF NOT EXISTS ux_users_username
        ON users (username);
    """);

Console.WriteLine(resetDatabase
    ? "Banco recriado com sucesso a partir do DbContext."
    : "Banco criado/verificado com sucesso a partir do DbContext.");
