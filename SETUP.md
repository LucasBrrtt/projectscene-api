# ProjectScene Setup

## Requisitos

- .NET 8 SDK
- Docker Desktop ou outro runtime compativel com `docker compose`

## Fluxo rapido

Na raiz do projeto:

```powershell
docker compose up -d
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

Se quiser contexto rapido do projeto e dos fluxos de banco, consulte tambem `AGENTS.md`.

## PC novo sem dump

### Opcao 1: com Docker

```powershell
git clone <repo>
cd ProjectScene
docker compose up -d
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

### Opcao 2: sem Docker

Com PostgreSQL ja instalado localmente:

```powershell
git clone <repo>
cd ProjectScene
.\scripts\setup-db.ps1
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

Se quiser apagar e recriar o banco local sem Docker:

```powershell
.\scripts\setup-db.ps1 -Reset
```

## PC novo com dump do banco

Se voce tiver um dump do banco atual, o caminho muda: em vez de gerar a estrutura pelo projeto, voce restaura o dump.

### Com Docker

```powershell
git clone <repo>
cd ProjectScene
docker compose down -v
docker compose up -d
```

Se o dump for `.sql`:

```powershell
psql -h localhost -p 5432 -U postgres -d project_scene_db -f .\backup_project_scene.sql
```

Se o dump for `.backup`:

```powershell
pg_restore -h localhost -p 5432 -U postgres -d project_scene_db .\backup_project_scene.backup
```

Depois:

```powershell
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

### Sem Docker

Com PostgreSQL ja instalado localmente e um banco vazio criado:

Se o dump for `.sql`:

```powershell
psql -h localhost -p 5432 -U postgres -d project_scene_db -f .\backup_project_scene.sql
```

Se o dump for `.backup`:

```powershell
pg_restore -h localhost -p 5432 -U postgres -d project_scene_db .\backup_project_scene.backup
```

Depois:

```powershell
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

### Regra pratica

- sem dump: use Docker ou `.\scripts\setup-db.ps1`
- com dump: restaure o dump no banco e depois rode a API
- se voce restaurar um dump completo, normalmente nao precisa rodar `.\scripts\setup-db.ps1`

## Sem Docker

Se voce ja tiver PostgreSQL instalado localmente, pode criar a estrutura do banco direto pelo modelo atual do projeto:

```powershell
.\scripts\setup-db.ps1
```

Se quiser apagar e recriar tudo:

```powershell
.\scripts\setup-db.ps1 -Reset
```

Esse comando usa o `AppDbContext` e os mapeamentos da infraestrutura para criar o banco e as tabelas.
Ele tambem aplica os indices unicos de `email` e `username` e adiciona as colunas de refresh token quando elas ainda nao existirem.

Resumo do script:

- arquivo: `scripts/setup-db.ps1`
- projeto usado: `ProjectScene.DbSetup`
- estrategia: `EnsureCreated()`
- opcao de reset: `-Reset`

## O que o Docker sobe

O `docker compose` sobe um PostgreSQL local com esta configuracao:

- Host: `localhost`
- Port: `5432`
- Database: `project_scene_db`
- Username: `postgres`
- Password: `root`

O banco e inicializado automaticamente com `database/init.sql`.

## Importante

O script de inicializacao do Postgres roda apenas quando o volume do banco e criado do zero.

Se voce quiser recriar tudo do zero:

```powershell
docker compose down -v
docker compose up -d
```

## Atualizando um banco local que ja existia

Se o banco local foi criado antes da configuracao atual, aplique os scripts abaixo para garantir os indices unicos e as colunas de sessao:

```powershell
psql -h localhost -p 5432 -U postgres -d project_scene_db -f .\database\scripts\001_users_unique_indexes.sql
psql -h localhost -p 5432 -U postgres -d project_scene_db -f .\database\scripts\002_users_refresh_token_columns.sql
```

No fluxo sem Docker, `.\scripts\setup-db.ps1` tambem tenta criar esses indices e colunas automaticamente.

## Rodando a API

```powershell
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

URLs padrao:

- `http://localhost:5046`
- `https://localhost:7275`

Swagger:

- `http://localhost:5046/swagger`
- `https://localhost:7275/swagger`

## Configuracao local

O projeto ja vem com uma chave JWT de desenvolvimento compartilhada pela equipe para facilitar o uso do mesmo banco local e dos mesmos tokens entre maquinas.

- connection string padrao em `ProjectScene.API/appsettings.json`
- configuracao de exemplo em `ProjectScene.API/appsettings.Secrets.example.json`
- `UserSecretsId` configurado em `ProjectScene.API/ProjectScene.API.csproj`
- chave JWT compartilhada de desenvolvimento em `ProjectScene.API/appsettings.Development.json`

Para o modo de desenvolvimento compartilhado, todos os devs devem usar:

```powershell
Jwt:Key = e586514e6f835b8ee6fec2bb8f1df78ee96c112612298657356777b69c60909d
```

Com essa chave, um token gerado na maquina A tambem sera aceito na maquina B, desde que ambas usem o mesmo `Issuer` e `Audience`.

Se algum dev quiser sobrescrever isso com `user-secrets`, precisa manter exatamente os mesmos valores de:

- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`

Se quiser sobrescrever a connection string por segredo local:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=project_scene_db;Username=postgres;Password=root" --project .\ProjectScene.API\ProjectScene.API.csproj
```

Essa chave deve ser tratada como chave de desenvolvimento compartilhada. Nao reutilizar em producao.
