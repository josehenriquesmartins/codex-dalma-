INSERT INTO parametros_sistema (chave, valor, descricao, ativo)
VALUES
('CFG_SMTP_HOST', '', 'Servidor SMTP', TRUE),
('CFG_SMTP_PORTA', '587', 'Porta SMTP', TRUE),
('CFG_SMTP_USUARIO', '', 'Usuário SMTP', TRUE),
('CFG_SMTP_SENHA', '', 'Senha SMTP', TRUE),
('CFG_SMS_PROVIDER', 'COMTELE', 'Provedor SMS', TRUE),
('CFG_SMS_CONTA', '', 'Conta SMS', TRUE),
('CFG_SMS_TOKEN', '', 'Token SMS', TRUE),
('CFG_SMS_SENHA', '', 'Senha SMS', TRUE),
('CFG_SMS_REMETENTE', '', 'Remetente SMS', TRUE),
('CFG_SMS_ENDPOINT', 'https://sms.comtele.com.br/api/v2/send', 'Endpoint SMS', TRUE),
('CFG_IA_API_KEY', '', 'API Key IA', TRUE),
('CFG_WHATSAPP_API_KEY', '', 'API Key WhatsApp', TRUE)
ON CONFLICT (chave) DO UPDATE
SET descricao = EXCLUDED.descricao,
    ativo = TRUE,
    data_hora_atualizacao = now();
