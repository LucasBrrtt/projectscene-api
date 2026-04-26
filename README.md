# ProjectScene API

Backend em ASP.NET Core com PostgreSQL e autenticacao JWT.

## Inicio rapido

Use o guia principal em `SETUP.md`.

Fluxo mais comum:

```powershell
docker compose up -d
dotnet build .\ProjectScene.sln
dotnet run --project .\ProjectScene.API\ProjectScene.API.csproj
```

O `SETUP.md` tambem cobre:

- onboarding local com e sem Docker
- restauracao de dump com `psql` ou `pg_restore`
- configuracao do banco local e JWT de desenvolvimento
- URLs da API e do Swagger

## Documentacao do repositorio

- `SETUP.md`: onboarding local e formas de subir banco/API
- `AGENTS.md`: notas de manutencao e contexto para suporte no projeto
- `database/README.md`: comportamento da inicializacao do banco local e scripts SQL auxiliares

## Banco de dados

Voce pode trabalhar de dois jeitos:

- com Docker: `docker compose up -d`
- sem Docker: `.\scripts\setup-db.ps1`

## JWT de desenvolvimento compartilhado

Para o ambiente local compartilhado da equipe, a configuracao de desenvolvimento usa a mesma chave JWT para que os tokens funcionem entre maquinas.

Detalhes e regras de uso estao em `SETUP.md`.
