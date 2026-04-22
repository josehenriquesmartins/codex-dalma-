DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM documentos_enviados
        WHERE contrato_id IS NULL
    ) THEN
        RAISE EXCEPTION 'Existem envios mensais sem contrato_id. Informe o contrato desses envios antes de aplicar esta atualização.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM documentos_enviados
        GROUP BY fornecedor_id, contrato_id, mes_referencia, ano_referencia
        HAVING COUNT(*) > 1
    ) THEN
        RAISE EXCEPTION 'Existem envios duplicados para o mesmo fornecedor, contrato e competência. Corrija os registros duplicados antes de aplicar esta atualização.';
    END IF;

    ALTER TABLE documentos_enviados
        DROP CONSTRAINT IF EXISTS uq_documentos_enviados_ref;

    ALTER TABLE documentos_enviados
        ALTER COLUMN contrato_id SET NOT NULL;

    ALTER TABLE documentos_enviados
        ADD CONSTRAINT uq_documentos_enviados_ref
        UNIQUE (fornecedor_id, contrato_id, mes_referencia, ano_referencia);
END $$;
