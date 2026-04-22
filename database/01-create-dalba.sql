CREATE SEQUENCE sq_usuarios START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_fornecedores START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_categorias START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_contratos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_documentos_tipos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_documentos_exigidos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_documentos_enviados START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_documentos_registrados START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_notificacoes START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_financeiro_liberacoes START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_logs_auditoria START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_parametros_sistema START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE sq_password_reset_tokens START WITH 1 INCREMENT BY 1;

CREATE TABLE categorias (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_categorias'),
    codigo VARCHAR(30) NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_categorias_codigo UNIQUE (codigo)
);

CREATE TABLE fornecedores (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_fornecedores'),
    codigo_fornecedor VARCHAR(20) NOT NULL,
    tipo_pessoa SMALLINT NOT NULL,
    porte_empresa SMALLINT NULL,
    categoria_id BIGINT NOT NULL,
    nome_ou_razao_social VARCHAR(200) NOT NULL,
    nome_fantasia VARCHAR(200) NULL,
    cpf_ou_cnpj VARCHAR(18) NOT NULL,
    ddi_telefone VARCHAR(5) NOT NULL,
    ddd_telefone VARCHAR(4) NOT NULL,
    numero_telefone VARCHAR(20) NOT NULL,
    email VARCHAR(160) NOT NULL,
    cep VARCHAR(12) NOT NULL,
    logradouro VARCHAR(160) NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(120) NULL,
    bairro VARCHAR(120) NOT NULL,
    cidade VARCHAR(120) NOT NULL,
    estado VARCHAR(2) NOT NULL,
    pais VARCHAR(100) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_fornecedores_codigo UNIQUE (codigo_fornecedor),
    CONSTRAINT uq_fornecedores_documento UNIQUE (cpf_ou_cnpj),
    CONSTRAINT fk_fornecedores_categoria FOREIGN KEY (categoria_id) REFERENCES categorias(id),
    CONSTRAINT ck_fornecedores_tipo_pessoa CHECK (tipo_pessoa IN (1, 2)),
    CONSTRAINT ck_fornecedores_porte_pf CHECK ((tipo_pessoa = 1 AND porte_empresa IS NULL) OR (tipo_pessoa = 2 AND porte_empresa IS NOT NULL)),
    CONSTRAINT ck_fornecedores_porte_pj CHECK (porte_empresa IS NULL OR porte_empresa IN (1,2,3,4,5))
);

CREATE TABLE usuarios (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_usuarios'),
    nome VARCHAR(160) NOT NULL,
    email VARCHAR(160) NOT NULL,
    login VARCHAR(60) NOT NULL,
    senha_hash_sha256 CHAR(64) NOT NULL,
    perfil SMALLINT NOT NULL,
    fornecedor_id BIGINT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_usuarios_email UNIQUE (email),
    CONSTRAINT uq_usuarios_login UNIQUE (login),
    CONSTRAINT fk_usuarios_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id),
    CONSTRAINT ck_usuarios_perfil CHECK (perfil IN (1,2,3)),
    CONSTRAINT ck_usuarios_fornecedor CHECK ((perfil = 3 AND fornecedor_id IS NOT NULL) OR (perfil IN (1,2)))
);

CREATE TABLE password_reset_tokens (
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

CREATE TABLE contratos (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_contratos'),
    fornecedor_id BIGINT NOT NULL,
    numero_contrato VARCHAR(60) NOT NULL,
    descricao VARCHAR(300) NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_contratos_numero UNIQUE (fornecedor_id, numero_contrato),
    CONSTRAINT fk_contratos_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id),
    CONSTRAINT ck_contratos_datas CHECK (data_fim IS NULL OR data_fim >= data_inicio)
);

CREATE TABLE documentos_tipos (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_documentos_tipos'),
    codigo VARCHAR(40) NOT NULL,
    nome_documento VARCHAR(160) NOT NULL,
    descricao VARCHAR(300) NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_documentos_tipos_codigo UNIQUE (codigo)
);

CREATE TABLE documentos_exigidos (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_documentos_exigidos'),
    documento_tipo_id BIGINT NOT NULL,
    tipo_pessoa SMALLINT NOT NULL,
    porte_empresa SMALLINT NULL,
    categoria_id BIGINT NOT NULL,
    obrigatorio BOOLEAN NOT NULL DEFAULT TRUE,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_documentos_exigidos_regra UNIQUE (documento_tipo_id, tipo_pessoa, porte_empresa, categoria_id),
    CONSTRAINT fk_documentos_exigidos_tipo FOREIGN KEY (documento_tipo_id) REFERENCES documentos_tipos(id),
    CONSTRAINT fk_documentos_exigidos_categoria FOREIGN KEY (categoria_id) REFERENCES categorias(id),
    CONSTRAINT ck_documentos_exigidos_tipo CHECK (tipo_pessoa IN (1,2)),
    CONSTRAINT ck_documentos_exigidos_porte CHECK ((tipo_pessoa = 1 AND porte_empresa IS NULL) OR (tipo_pessoa = 2 AND porte_empresa IN (1,2,3,4,5)))
);

CREATE TABLE documentos_enviados (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_documentos_enviados'),
    fornecedor_id BIGINT NOT NULL,
    usuario_id BIGINT NOT NULL,
    contrato_id BIGINT NOT NULL,
    mes_referencia SMALLINT NOT NULL,
    ano_referencia SMALLINT NOT NULL,
    data_hora_registro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status SMALLINT NOT NULL,
    usuario_registro VARCHAR(60) NOT NULL,
    observacao VARCHAR(500) NULL,
    avaliado_por_usuario_id BIGINT NULL,
    data_hora_validacao_final TIMESTAMP NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_documentos_enviados_ref UNIQUE (fornecedor_id, contrato_id, mes_referencia, ano_referencia),
    CONSTRAINT fk_documentos_enviados_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id),
    CONSTRAINT fk_documentos_enviados_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    CONSTRAINT fk_documentos_enviados_contrato FOREIGN KEY (contrato_id) REFERENCES contratos(id),
    CONSTRAINT fk_documentos_enviados_avaliador FOREIGN KEY (avaliado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT ck_documentos_enviados_mes CHECK (mes_referencia BETWEEN 1 AND 12),
    CONSTRAINT ck_documentos_enviados_status CHECK (status IN (1,2,3))
);

CREATE TABLE documentos_registrados (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_documentos_registrados'),
    documento_enviado_id BIGINT NOT NULL,
    documento_tipo_id BIGINT NOT NULL,
    nome_original_arquivo VARCHAR(255) NOT NULL,
    nome_arquivo_fisico VARCHAR(255) NOT NULL,
    caminho_arquivo VARCHAR(255) NOT NULL,
    extensao VARCHAR(10) NOT NULL,
    tamanho_bytes BIGINT NOT NULL,
    data_hora_upload TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    usuario_upload VARCHAR(60) NOT NULL,
    status_validacao_documento SMALLINT NOT NULL DEFAULT 1,
    avaliado_por_usuario_id BIGINT NULL,
    data_hora_avaliacao TIMESTAMP NULL,
    observacao_avaliacao VARCHAR(500) NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_documentos_registrados_item UNIQUE (documento_enviado_id, documento_tipo_id),
    CONSTRAINT fk_documentos_registrados_envio FOREIGN KEY (documento_enviado_id) REFERENCES documentos_enviados(id),
    CONSTRAINT fk_documentos_registrados_tipo FOREIGN KEY (documento_tipo_id) REFERENCES documentos_tipos(id),
    CONSTRAINT fk_documentos_registrados_avaliador FOREIGN KEY (avaliado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT ck_documentos_registrados_status CHECK (status_validacao_documento IN (1,2,3)),
    CONSTRAINT ck_documentos_registrados_extensao CHECK (LOWER(extensao) IN ('.pdf','.jpg','.jpeg','.png'))
);

CREATE TABLE notificacoes (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_notificacoes'),
    usuario_id BIGINT NULL,
    remetente_usuario_id BIGINT NULL,
    fornecedor_id BIGINT NULL,
    tipo_notificacao SMALLINT NOT NULL,
    titulo VARCHAR(180) NOT NULL,
    mensagem VARCHAR(1000) NOT NULL,
    status_envio SMALLINT NOT NULL DEFAULT 1,
    data_hora_envio TIMESTAMP NULL,
    tentativas INTEGER NOT NULL DEFAULT 0,
    referencia_entidade VARCHAR(60) NULL,
    referencia_id BIGINT NULL,
    destinatario VARCHAR(180) NULL,
    erro VARCHAR(500) NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT fk_notificacoes_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    CONSTRAINT fk_notificacoes_remetente_usuario FOREIGN KEY (remetente_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT fk_notificacoes_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id),
    CONSTRAINT ck_notificacoes_tipo CHECK (tipo_notificacao IN (1,2,3)),
    CONSTRAINT ck_notificacoes_status CHECK (status_envio IN (1,2,3))
);

CREATE TABLE financeiro_liberacoes (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_financeiro_liberacoes'),
    documento_enviado_id BIGINT NOT NULL,
    fornecedor_id BIGINT NOT NULL,
    contrato_id BIGINT NULL,
    status_financeiro SMALLINT NOT NULL,
    data_hora_geracao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    gerado_por_usuario_id BIGINT NOT NULL,
    observacao VARCHAR(500) NULL,
    numero_nota_fiscal VARCHAR(60) NULL,
    nome_original_nota_fiscal VARCHAR(255) NULL,
    nome_arquivo_fisico_nota_fiscal VARCHAR(255) NULL,
    caminho_arquivo_nota_fiscal VARCHAR(255) NULL,
    extensao_nota_fiscal VARCHAR(10) NULL,
    tamanho_bytes_nota_fiscal BIGINT NULL,
    data_recebimento_nota_fiscal TIMESTAMP NULL,
    data_hora_upload_nota_fiscal TIMESTAMP NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_financeiro_liberacoes_envio UNIQUE (documento_enviado_id),
    CONSTRAINT fk_financeiro_liberacoes_envio FOREIGN KEY (documento_enviado_id) REFERENCES documentos_enviados(id),
    CONSTRAINT fk_financeiro_liberacoes_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id),
    CONSTRAINT fk_financeiro_liberacoes_contrato FOREIGN KEY (contrato_id) REFERENCES contratos(id),
    CONSTRAINT fk_financeiro_liberacoes_usuario FOREIGN KEY (gerado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT ck_financeiro_liberacoes_status CHECK (status_financeiro IN (1,2,3,4,5))
);

CREATE TABLE logs_auditoria (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_logs_auditoria'),
    usuario_id BIGINT NULL,
    entidade VARCHAR(60) NOT NULL,
    entidade_id BIGINT NULL,
    acao SMALLINT NOT NULL,
    dados_resumidos VARCHAR(1000) NOT NULL,
    ip_origem VARCHAR(50) NULL,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT fk_logs_auditoria_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    CONSTRAINT ck_logs_auditoria_acao CHECK (acao IN (1,2,3,4,5,6,7,8,9))
);

CREATE TABLE parametros_sistema (
    id BIGINT PRIMARY KEY DEFAULT nextval('sq_parametros_sistema'),
    chave VARCHAR(80) NOT NULL,
    valor VARCHAR(500) NOT NULL,
    descricao VARCHAR(250) NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_hora_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_hora_atualizacao TIMESTAMP NULL,
    CONSTRAINT uq_parametros_sistema_chave UNIQUE (chave)
);

CREATE INDEX ix_fornecedores_categoria_id ON fornecedores (categoria_id);
CREATE INDEX ix_usuarios_fornecedor_id ON usuarios (fornecedor_id);
CREATE INDEX ix_password_reset_tokens_usuario ON password_reset_tokens (usuario_id, utilizado_em_utc, expira_em_utc);
CREATE INDEX ix_contratos_fornecedor_id ON contratos (fornecedor_id);
CREATE INDEX ix_documentos_exigidos_categoria ON documentos_exigidos (categoria_id, tipo_pessoa, porte_empresa);
CREATE INDEX ix_documentos_enviados_status ON documentos_enviados (status, ano_referencia, mes_referencia);
CREATE INDEX ix_documentos_registrados_status ON documentos_registrados (status_validacao_documento);
CREATE INDEX ix_notificacoes_status ON notificacoes (status_envio, tipo_notificacao);
CREATE INDEX ix_notificacoes_usuario_remetente ON notificacoes (usuario_id, remetente_usuario_id);
CREATE INDEX ix_financeiro_liberacoes_status ON financeiro_liberacoes (status_financeiro);
CREATE INDEX ix_logs_auditoria_entidade ON logs_auditoria (entidade, entidade_id);

INSERT INTO categorias (codigo, descricao, ativo) VALUES
('SERV', 'Serviços contínuos', TRUE),
('OBRA', 'Obras e manutenção', TRUE),
('CONS', 'Consultoria', TRUE);

INSERT INTO documentos_tipos (codigo, nome_documento, descricao, ativo) VALUES
('DOC_FISCAL', 'Certidão Fiscal', 'Comprovação de regularidade fiscal', TRUE),
('DOC_TRAB', 'Comprovante Trabalhista', 'Comprovação trabalhista mensal', TRUE),
('DOC_CONTRATO', 'Contrato Assinado', 'Contrato vigente assinado', TRUE),
('DOC_BANCARIO', 'Comprovante Bancário', 'Comprovante bancário atualizado', TRUE);

INSERT INTO fornecedores (
    codigo_fornecedor, tipo_pessoa, porte_empresa, categoria_id, nome_ou_razao_social, nome_fantasia,
    cpf_ou_cnpj, ddi_telefone, ddd_telefone, numero_telefone, email, cep, logradouro, numero,
    complemento, bairro, cidade, estado, pais, ativo
) VALUES (
    '000123', 2, 2, 1, 'Fornecedor Exemplo LTDA', 'Fornecedor Exemplo',
    '12345678000195', '+55', '11', '999999999', 'fornecedor@dalba.local', '01001000', 'Rua Exemplo', '100',
    NULL, 'Centro', 'Sao Paulo', 'SP', 'Brasil', TRUE
);

INSERT INTO usuarios (nome, email, login, senha_hash_sha256, perfil, fornecedor_id, ativo) VALUES
('Administrador Dalba', 'admin@dalba.local', 'admin', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 1, NULL, TRUE),
('Financeiro Dalba', 'financeiro@dalba.local', 'financeiro', '3FD19780BDA9898E8CFFA4429FC0EAC3CBE142295E57E4F9E5AC1DD8EC5C6DC1', 2, NULL, TRUE),
('Fornecedor Exemplo', 'fornecedor@dalba.local', 'fornecedor', 'B803CDF310DF06A5C8A359A79F04A2A167A31477601778C67127FD1D2BE71A90', 3, 1, TRUE);

INSERT INTO contratos (fornecedor_id, numero_contrato, descricao, data_inicio, data_fim, ativo) VALUES
(1, 'CTR-2026-0001', 'Contrato de prestação de serviços contínuos', '2026-01-01', '2026-12-31', TRUE);

INSERT INTO documentos_exigidos (documento_tipo_id, tipo_pessoa, porte_empresa, categoria_id, obrigatorio, ativo) VALUES
(1, 2, 2, 1, TRUE, TRUE),
(2, 2, 2, 1, TRUE, TRUE),
(3, 2, 2, 1, TRUE, TRUE),
(4, 2, 2, 1, TRUE, TRUE),
(1, 1, NULL, 1, TRUE, TRUE);

INSERT INTO parametros_sistema (chave, valor, descricao, ativo) VALUES
('UPLOAD_MAX_MB', '10', 'Tamanho máximo do upload em MB', TRUE),
('UPLOAD_ALLOWED_EXTENSIONS', '.pdf,.jpg,.jpeg,.png', 'Extensões permitidas para upload', TRUE);
