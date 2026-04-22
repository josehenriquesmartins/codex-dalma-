-- Troque pelo e-mail real que deve receber o token de recuperação.
UPDATE usuarios
SET email = 'seu-email@dominio.com',
    data_hora_atualizacao = CURRENT_TIMESTAMP
WHERE login = 'admin';
