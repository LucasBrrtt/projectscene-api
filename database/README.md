# Database scripts

Schema changes in this project are applied with SQL scripts, not EF Core migrations.

- `scripts/001_users_unique_indexes.sql`: creates unique indexes for `users.email` and `users.username`.
