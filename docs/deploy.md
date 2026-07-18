# Deploy

## Objetivo

Executar o portal DALBA localmente ou em servidor Windows/Linux usando Docker Compose, com PostgreSQL, API .NET e frontend Angular.

## Pre-requisitos

- Docker Desktop ou Docker Engine com Docker Compose.
- Git.
- Portas livres: `5432`, `8080`, `4200`, ou ajuste no `.env`.

## Variaveis de ambiente

O projeto usa `.env` na raiz:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD="troque_esta_senha"
POSTGRES_DB=DALBA
POSTGRES_PORT=5432
API_PORT=8080
WEB_PORT=4200
JWT_ISSUER=Dalba.Financeiro.Api
JWT_AUDIENCE=Dalba.Financeiro.Frontend
JWT_KEY=troque_esta_chave_jwt
```

O arquivo `.env` real fica fora do Git. Use `.env.example` como modelo.

## Subir ambiente

```powershell
cd C:\Projetos\Dalba
docker compose up -d --build
```

URLs:

- Frontend: `http://localhost:4200/login`
- API/Swagger: `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/health`
- PostgreSQL: `localhost:5432`

## Senha PostgreSQL em ambiente existente

Se o volume `dalba-postgres-data` ja existe, alterar `POSTGRES_PASSWORD` no `.env` nao muda a senha do usuario ja criado no banco.

Opção sem perder dados:

```powershell
docker exec -it dalba-postgres psql -U postgres -d DALBA -c "ALTER USER postgres WITH PASSWORD 'nova_senha';"
docker compose up -d api
```

Opção recriando banco, com perda dos dados locais:

```powershell
docker compose down -v
docker compose up -d --build
```

## Atualizar versao

```powershell
git pull origin main
docker compose up -d --build
```

## Publicacao externa

Para publicar fora da rede local, recomenda-se:

- Usar HTTPS em proxy reverso como Nginx, Caddy, IIS ou Traefik.
- Nao expor PostgreSQL publicamente.
- Trocar `JWT_KEY` por chave forte.
- Configurar backup do volume PostgreSQL.
- Configurar SMTP/SMS/API Keys pela tela Admin `Configuracao`.
