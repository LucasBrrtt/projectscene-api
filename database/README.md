# Banco de dados

Para desenvolvimento local com Docker, o PostgreSQL e inicializado automaticamente usando `init.sql`.

Esse script roda apenas quando o container cria um volume novo do banco.

Para bancos locais ja existentes, use tambem:

```powershell
psql -h localhost -p 5432 -U postgres -d project_scene_db -f .\database\scripts\001_users_unique_indexes.sql
```

Se precisar recriar o banco do zero:

```powershell
docker compose down -v
docker compose up -d
```
