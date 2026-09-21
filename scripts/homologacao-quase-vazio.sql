BEGIN;

TRUNCATE TABLE
    password_reset_tokens,
    financeiro_liberacoes,
    documentos_registrados,
    documentos_enviados,
    notificacoes,
    logs_auditoria,
    contratos,
    documentos_exigidos,
    usuarios,
    fornecedores
RESTART IDENTITY CASCADE;

INSERT INTO fornecedores (
    codigo_fornecedor,
    tipo_pessoa,
    porte_empresa,
    categoria_id,
    nome_ou_razao_social,
    nome_fantasia,
    cpf_ou_cnpj,
    ddi_telefone,
    ddd_telefone,
    numero_telefone,
    email,
    cep,
    logradouro,
    numero,
    complemento,
    bairro,
    cidade,
    estado,
    pais,
    ativo
)
SELECT
    'FORN-SISTEMA',
    2,
    2,
    (SELECT id FROM categorias ORDER BY id LIMIT 1),
    'Fornecedor de Sistema',
    'Fornecedor de Sistema',
    '00000000000191',
    '+55',
    '11',
    '000000000',
    'fornecedor@dalba.local',
    '',
    'Nao informado',
    'S/N',
    NULL,
    'Nao informado',
    'Nao informado',
    'SP',
    'Brasil',
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM fornecedores WHERE codigo_fornecedor = 'FORN-SISTEMA');

INSERT INTO usuarios (nome, email, login, senha_hash_sha256, perfil, fornecedor_id, ativo) VALUES
('Administrador Dalba', 'admin@dalba.local', 'admin', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 1, NULL, TRUE),
('Custos Dalba', 'financeiro@dalba.local', 'financeiro', '3FD19780BDA9898E8CFFA4429FC0EAC3CBE142295E57E4F9E5AC1DD8EC5C6DC1', 2, NULL, TRUE),
('Fornecedor Sistema', 'fornecedor@dalba.local', 'fornecedor', 'B803CDF310DF06A5C8A359A79F04A2A167A31477601778C67127FD1D2BE71A90', 3, (SELECT id FROM fornecedores WHERE codigo_fornecedor = 'FORN-SISTEMA'), TRUE);

INSERT INTO parametros_sistema (chave, valor, descricao, ativo) VALUES
('UPLOAD_MAX_MB', '10', 'Tamanho maximo do upload em MB', TRUE),
('UPLOAD_ALLOWED_EXTENSIONS', '.pdf,.jpg,.jpeg,.png', 'Extensoes permitidas para upload', TRUE),
('CFG_SMTP_HOST', '', 'Servidor SMTP', TRUE),
('CFG_SMTP_PORTA', '587', 'Porta SMTP', TRUE),
('CFG_SMTP_USUARIO', '', 'Usuario SMTP', TRUE),
('CFG_SMTP_SENHA', '', 'Senha SMTP', TRUE),
('CFG_SMS_PROVIDER', 'COMTELE', 'Provedor SMS', TRUE),
('CFG_SMS_CONTA', '', 'Conta SMS', TRUE),
('CFG_SMS_TOKEN', '', 'Token SMS', TRUE),
('CFG_SMS_SENHA', '', 'Senha SMS', TRUE),
('CFG_SMS_REMETENTE', '', 'Remetente SMS', TRUE),
('CFG_SMS_ENDPOINT', 'https://sms.comtele.com.br/api/v2/send', 'Endpoint SMS', TRUE),
('CFG_IA_API_KEY', '', 'API Key IA', TRUE),
('CFG_WHATSAPP_API_KEY', '', 'API Key WhatsApp', TRUE)
ON CONFLICT (chave) DO NOTHING;

SELECT setval('sq_fornecedores', COALESCE((SELECT MAX(id) FROM fornecedores), 1), TRUE);
SELECT setval('sq_usuarios', COALESCE((SELECT MAX(id) FROM usuarios), 1), TRUE);
SELECT setval('sq_contratos', 1, FALSE);
SELECT setval('sq_documentos_exigidos', 1, FALSE);
SELECT setval('sq_documentos_enviados', 1, FALSE);
SELECT setval('sq_documentos_registrados', 1, FALSE);
SELECT setval('sq_notificacoes', 1, FALSE);
SELECT setval('sq_financeiro_liberacoes', 1, FALSE);
SELECT setval('sq_logs_auditoria', 1, FALSE);
SELECT setval('sq_password_reset_tokens', 1, FALSE);

COMMIT;
