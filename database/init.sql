CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    access_level VARCHAR(20) NOT NULL DEFAULT 'user',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_login TIMESTAMP NULL,
    refresh_token VARCHAR(255) NULL,
    refresh_token_expiry TIMESTAMP NULL,
    refresh_token_persistent BOOLEAN NULL,
    username VARCHAR(100) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_email
    ON users (email);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_username
    ON users (username);
