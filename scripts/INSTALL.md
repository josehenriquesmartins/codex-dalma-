# Instalacao DALBA - Servidor de Homologacao

Este pacote contem tudo que e necessario para subir o sistema DALBA (API .NET 9 +
frontend Angular + PostgreSQL) em um novo servidor via Docker Compose, incluindo os
dados atuais do banco de desenvolvimento.

## 1. Pre-requisitos no servidor

- Docker Engine + Docker Compose plugin instalados (`docker compose version`).
- Portas livres: `5432` (Postgres), `8080` (API), `4200` (frontend) - ou outras, definidas no `.env`.
- Acesso de rede liberado para as portas escolhidas, se o acesso for externo.

## 2. Configurar variaveis de ambiente

```powershell
Copy-Item .env.homologacao.example .env
```

Edite o `.env` e troque **obrigatoriamente**:

- `POSTGRES_PASSWORD`: defina uma senha forte, exclusiva deste ambiente (nao reutilize
  a senha de desenvolvimento).
- `JWT_KEY`: ja vem preenchida com uma chave unica gerada no empacotamento; pode manter
  ou trocar por outra.

## 3. Subir os containers (schema vazio)

```powershell
docker compose up -d --build
```

Isso cria os containers `dalba-postgres`, `dalba-api` e `dalba-web`, e o Postgres
executa automaticamente `database/01-create-dalba.sql` na primeira inicializacao
(schema + seed padrao). **Aguarde** o container do Postgres ficar saudavel antes do
proximo passo (`docker compose logs -f postgres`).

## 4. Restaurar os dados atuais (dump incluso no pacote)

O pacote inclui `database-dump/dalba-dump-<data>.sql`, um export completo do banco de
desenvolvimento no momento da geracao deste instalador (inclui fornecedores, usuarios,
contratos e demais dados ja cadastrados).

Antes de restaurar, **zere o schema criado no passo 3** para evitar conflito de dados
duplicados (sequences, seeds), pois o dump e um export completo, não incremental:

```powershell
docker exec -it dalba-postgres psql -U postgres -d DALBA -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
Get-Content .\database-dump\dalba-dump-*.sql | docker exec -i dalba-postgres psql -U postgres -d DALBA
```

## 5. Verificar

- Frontend: `http://<servidor>:4200/login`
- API/Swagger: `http://<servidor>:8080/swagger`
- Health check: `http://<servidor>:8080/health`

Login com os usuarios existentes no dump (os mesmos do ambiente de origem). Se preferir
comecar com usuarios seed padrao em vez do dump, pule o passo 4 e use:
`admin/Admin@123`, `financeiro/Financeiro@123`, `fornecedor/Fornecedor@123` (troque as
senhas apos o primeiro login).

## 6. Recomendacoes de seguranca para homologacao/producao

- Nao exponha a porta do Postgres (`5432`) publicamente; mantenha-a acessivel apenas
  internamente.
- Publique o frontend/API atras de um proxy reverso com HTTPS (Nginx, Caddy, IIS ou
  Traefik).
- Troque as senhas dos usuarios seed/importados assim que possivel.
- Configure SMTP/SMS/API Keys pela tela Admin "Configuracao" (nao vem no dump se o
  ambiente de origem nao tinha essas integracoes configuradas).
- Faca backup do volume `dalba-postgres-data` regularmente (ver `docs/banco-de-dados.md`
  no codigo-fonte deste pacote).

## Observacao sobre o frontend

O container `dalba-web` roda o Angular em modo de desenvolvimento (`npm start`), o
mesmo usado localmente pela equipe - nao ha build de producao (Nginx + `ng build`)
configurado neste projeto ainda. Funciona para homologacao, mas para producao externa
vale revisitar esse ponto com a equipe de desenvolvimento.
