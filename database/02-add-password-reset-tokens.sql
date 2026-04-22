CREATE SEQUENCE IF NOT EXISTS sq_password_reset_tokens START WITH 1 INCREMENT BY 1;

CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_password_reset_tokens'),
    usuario_id BIGINT NOT NULL,
    token_hash_sha256 CHAR(64) NOT NULL,
    expira_em_utc TIMESTAMP NOT NULL,
    utilizado_em_utc TIMESTAMP NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_password_reset_tokens_hash UNIQUE (token_hash_sha256),
    CONSTRAINT fk_password_reset_tokens_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

CREATE INDEX IF NOT EXISTS ix_password_reset_tokens_usuario
    ON password_reset_tokens (usuario_id, utilizado_em_utc, expira_em_utc);
