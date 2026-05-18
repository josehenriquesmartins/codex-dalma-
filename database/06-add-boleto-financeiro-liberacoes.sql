ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS nome_original_boleto VARCHAR(255) NULL;
ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS nome_arquivo_fisico_boleto VARCHAR(255) NULL;
ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS caminho_arquivo_boleto VARCHAR(255) NULL;
ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS extensao_boleto VARCHAR(10) NULL;
ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS tamanho_bytes_boleto BIGINT NULL;
ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS data_hora_upload_boleto TIMESTAMP NULL;
