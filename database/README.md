# Banco de dados

Este projeto usa PostgreSQL e pode ser inicializado de duas formas:

- com Docker, usando `docker compose`
- sem Docker, usando um PostgreSQL local e os scripts do repositorio

## Configuracao esperada pela aplicacao

- Host: `localhost`
- Porta: `5432`
- Banco: `project_scene_db`
- Usuario: `postgres`
- Senha: `root`

## Fluxo com Docker

Para desenvolvimento local com Docker, o PostgreSQL e inicializado automaticamente usando `database/init.sql`.

Esse script roda apenas quando o container cria um volume novo do banco.

Subir o banco:

```powershell
docker compose up -d
```

Recriar o banco do zero:

```powershell
docker compose down -v
docker compose up -d
```

## Fluxo sem Docker

Se voce ja tiver PostgreSQL instalado localmente, crie o banco com:

```bash
export PGPASSWORD=root
createdb -h localhost -p 5432 -U postgres project_scene_db
```

Se a senha do usuario `postgres` ainda nao for `root`, ajuste antes:

```bash
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'root';"
```

Voce tambem pode preparar o banco local pelo script do repositorio:

```powershell
.\scripts\setup-db.ps1
```

Para apagar e recriar tudo:

```powershell
.\scripts\setup-db.ps1 -Reset
```

## Restaurando um dump

Se voce recebeu um dump do banco atual, restaure no `project_scene_db`.

Se o dump for SQL puro:

```bash
psql -h localhost -p 5432 -U postgres -d project_scene_db -f /caminho/para/seu-dump.sql
```

Se o dump for um backup custom do PostgreSQL:

```bash
pg_restore -h localhost -p 5432 -U postgres -d project_scene_db /caminho/para/seu-dump.backup
```

Dica: se o arquivo comecar com `PGDMP`, ele nao e SQL puro e deve ser restaurado com `pg_restore`, mesmo que a extensao seja `.sql`.

## Atualizando um banco local existente

Se o banco local ja existia antes da configuracao atual, aplique tambem:

```bash
psql -h localhost -p 5432 -U postgres -d project_scene_db -f database/scripts/001_users_unique_indexes.sql
psql -h localhost -p 5432 -U postgres -d project_scene_db -f database/scripts/002_users_refresh_token_columns.sql
```

No fluxo sem Docker, `.\scripts\setup-db.ps1` tambem tenta criar esses indices e colunas automaticamente.

## Scripts deste diretorio

- `init.sql`: inicializacao do banco no fluxo com Docker
- `scripts/001_users_unique_indexes.sql`: cria indices unicos para `users.email` e `users.username`
- `scripts/002_users_refresh_token_columns.sql`: adiciona colunas de refresh token em bancos antigos
