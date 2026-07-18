# Banco de Dados

## Banco

Nome: `DALBA`

Tecnologia: PostgreSQL.

Scripts:

- `database/00-bootstrap-database.sql`: recria o banco.
- `database/01-create-dalba.sql`: DDL principal com sequences, tabelas, constraints, indices e seeds.
- `database/02-add-password-reset-tokens.sql`: tokens de redefinicao de senha.
- `database/03-update-user-email-for-password-reset.sql`: ajustes de e-mail.
- `database/04-add-notification-sender.sql`: remetente de notificacao.
- `database/05-require-contract-per-monthly-submission.sql`: vinculo de contrato no envio mensal.
- `database/06-add-boleto-financeiro-liberacoes.sql`: campos de boleto.
- `database/07-add-af-financeiro-liberacoes.sql`: campo AF na liberacao.
- `database/08-add-communication-configuration-parameters.sql`: parametros SMTP, SMS, IA e WhatsApp para ambientes existentes.

## Tabelas principais

- `usuarios`: autenticacao e perfil.
- `fornecedores`: cadastro do fornecedor.
- `categorias`: classificacao de fornecedores.
- `contratos`: contratos por fornecedor.
- `documentos_tipos`: catalogo de documentos.
- `documentos_exigidos`: regra documental por tipo/porte/categoria.
- `documentos_enviados`: competencia mensal do fornecedor.
- `documentos_registrados`: arquivos enviados.
- `financeiro_liberacoes`: liberacoes para NF, boleto e pagamento.
- `notificacoes`: log e tentativa de envio.
- `logs_auditoria`: auditoria basica.
- `parametros_sistema`: configuracoes SMTP/SMS/API Keys.
- `password_reset_tokens`: redefinicao de senha.

## Sequences

Todas as tabelas com `id` possuem sequence explicita, por exemplo:

- `sq_usuarios`
- `sq_fornecedores`
- `sq_documentos_enviados`
- `sq_financeiro_liberacoes`
- `sq_parametros_sistema`

## Usuario inicial

Credenciais seed:

- `admin / Admin@123`
- `financeiro / Financeiro@123`
- `fornecedor / Fornecedor@123`

## Backup local

```powershell
docker exec dalba-postgres pg_dump -U postgres -d DALBA > dalba-backup.sql
```

## Restore local

```powershell
Get-Content .\dalba-backup.sql | docker exec -i dalba-postgres psql -U postgres -d DALBA
```
