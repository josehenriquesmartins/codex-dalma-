# Arquitetura

## Visao geral

O sistema DALBA segue uma arquitetura em camadas:

- `Domain`: entidades e enums de negocio.
- `Application`: DTOs, contratos, regras de negocio e servicos de aplicacao.
- `Infrastructure`: EF Core, PostgreSQL, seguranca, armazenamento local, notificacoes e integracoes externas.
- `Api`: controllers REST, autenticacao JWT, Swagger e middleware de erros.
- `Frontend`: Angular com rotas, guards, telas e services HTTP.

## Backend

Projetos:

- `src/backend/Dalba.Financeiro.Domain`
- `src/backend/Dalba.Financeiro.Application`
- `src/backend/Dalba.Financeiro.Infrastructure`
- `src/backend/Dalba.Financeiro.Api`

Responsabilidades:

- `Domain`: `Fornecedor`, `Contrato`, `DocumentoEnviado`, `DocumentoRegistrado`, `FinanceiroLiberacao`, `ParametroSistema`.
- `Application`: fluxos de fornecedor, validacao documental, envio de NF/boleto, custos, dashboards e configuracoes.
- `Infrastructure`: `AppDbContext`, mapeamentos EF Core, JWT, armazenamento de arquivos, SMTP/SMS e Protheus mock.
- `Api`: endpoints REST e politicas de autorizacao.

## Frontend

Local: `src/frontend`

Principais areas:

- Login e redefinicao de senha.
- Layout com menu por perfil.
- Dashboard por perfil.
- Cadastros administrativos.
- Portal do fornecedor.
- Validacao documental.
- Custos.
- Configuracao Admin.

## Autenticacao e autorizacao

- JWT Bearer.
- Perfis: `Admin`, `Financeiro`, `Fornecedor`.
- Guards no Angular: `AuthGuard` e `RoleGuard`.
- Policies no backend: `AdminOnly`, `FinanceiroOnly`, `FornecedorOnly`, `AdminOrFinanceiro`.

## Armazenamento de arquivos

Uploads sao gravados no volume Docker `dalba-uploads`.

Padrao de caminho:

```text
codigo-fornecedor/ano/mes/
```

Exemplo:

```text
000123/2026/05/
```

## Integracoes

- SMTP/SMS: estrutura real com dispatcher e registro em `notificacoes`.
- IA/WhatsApp: configuracoes persistidas para integracao futura.
- Protheus: contrato de servico e mock em `Infrastructure`, documentado em `docs/integracao-protheus.md`.
