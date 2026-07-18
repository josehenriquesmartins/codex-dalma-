# DALBA Custos

Solução full-stack para gestão de documentos, conformidade e custos de pagamentos contratuais com backend `.NET 9`, frontend `Angular`, banco `PostgreSQL` e execução via `Docker Compose`.

## Estrutura

- `Dalba.Financeiro.slnx`: solução pronta para Visual Studio 2022.
- `database/01-create-dalba.sql`: DDL completo do banco `DALBA` com sequences explícitas.
- `src/backend`: API em Clean Architecture.
- `src/frontend`: aplicação Angular responsiva.
- `docker-compose.yml`: orquestra PostgreSQL, API e frontend.
- `.env.example`: modelo das variáveis de ambiente. O `.env` real não deve ser versionado.

## Documentação

- `docs/deploy.md`: execução local, Docker, variáveis e publicação.
- `docs/arquitetura.md`: camadas, projetos e integrações.
- `docs/sistema.md`: perfis, fluxo e regras principais.
- `docs/banco-de-dados.md`: tabelas, sequences, scripts e backup.
- `docs/telas-funcionalidades.md`: telas e funcionalidades por perfil.

## Credenciais seed

- `admin / Admin@123`
- `financeiro / Financeiro@123`
- `fornecedor / Fornecedor@123`

## Execução local com Docker

```powershell
docker compose up --build
```

## Execução no Visual Studio 2022

1. Abra `Dalba.Financeiro.slnx`.
2. Defina `Dalba.Financeiro.Api` como projeto de inicialização.
3. Garanta um PostgreSQL local com banco `DALBA` ou suba pelo `docker compose`.
4. Ajuste `appsettings.json` se necessário.
5. No frontend:

```powershell
cd src\frontend
npm install
npm start
```
