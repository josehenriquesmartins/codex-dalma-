ALTER TABLE notificacoes
    ADD COLUMN IF NOT EXISTS remetente_usuario_id BIGINT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_notificacoes_remetente_usuario'
    ) THEN
        ALTER TABLE notificacoes
            ADD CONSTRAINT fk_notificacoes_remetente_usuario
            FOREIGN KEY (remetente_usuario_id) REFERENCES usuarios(id);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_notificacoes_usuario_remetente
    ON notificacoes (usuario_id, remetente_usuario_id);
