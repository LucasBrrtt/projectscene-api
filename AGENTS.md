# Notas do Projeto

## Objetivo

Este backend usa:

- .NET 8
- ASP.NET Core Web API
- PostgreSQL
- EF Core apenas como mapeamento e criacao local do schema

## Regra importante sobre banco

- Em desenvolvimento local, o banco pode ser criado de duas formas:
  - com Docker, usando `docker compose up -d`
  - sem Docker, usando `.\scripts\setup-db.ps1`
- O projeto nao usa EF Core migrations como fluxo principal.
- Para ambiente local sem Docker, o schema pode ser criado diretamente a partir do `AppDbContext` pelo projeto `ProjectScene.DbSetup`.

## Como criar o banco sem Docker

Com PostgreSQL instalado localmente e acessivel pela connection string atual:

```powershell
.\scripts\setup-db.ps1
```

Para apagar e recriar tudo:

```powershell
.\scripts\setup-db.ps1 -Reset
```

## Como criar o banco com Docker

```powershell
docker compose up -d
```

O container do PostgreSQL executa automaticamente `database/init.sql` quando o volume e criado do zero.

Para recriar tudo do zero:

```powershell
docker compose down -v
docker compose up -d
```

## Como rodar a API

```powershell
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

## Configuracao local

- connection string: `ProjectScene.API/appsettings.json`
- exemplo de segredos: `ProjectScene.API/appsettings.Secrets.example.json`
- chave JWT de desenvolvimento compartilhada: `ProjectScene.API/appsettings.Development.json`

## Observacoes para manutencao

- Se alguem pedir para "gerar o banco pelo mapeamento", o caminho correto e `.\scripts\setup-db.ps1`.
- Se alguem pedir para "subir tudo rapido", o caminho mais simples e Docker + `dotnet run`.
- Se alguem pedir o fluxo de "PC novo", consultar `SETUP.md`.
- Se alguem tiver um dump do banco, orientar a restaurar o dump em vez de recriar a estrutura pelo `DbContext`.
- Se o dump for completo, normalmente nao usar `.\scripts\setup-db.ps1` antes.
- Para bancos locais antigos, existe um script de upgrade em `database/scripts/001_users_unique_indexes.sql`.
- O `.\scripts\setup-db.ps1` tambem tenta aplicar esses indices automaticamente apos `EnsureCreated()`.
- O arquivo `SETUP.md` contem o passo a passo completo para onboarding local.
- Para ambiente local compartilhado, todos os devs devem usar a mesma `Jwt:Key`, `Jwt:Issuer` e `Jwt:Audience`.
- A chave atual de desenvolvimento compartilhada e `e586514e6f835b8ee6fec2bb8f1df78ee96c112612298657356777b69c60909d`.
- Essa chave e somente para desenvolvimento. Nao reutilizar em producao.
