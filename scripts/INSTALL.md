# Instalacao DALBA - Servidor de Homologacao

Este pacote contem tudo que e necessario para subir o sistema DALBA (API .NET 9 +
frontend Angular + PostgreSQL) em um novo servidor via Docker Compose.

Este pacote foi preparado com banco quase vazio: mantem apenas os 3 usuarios padrao
(`admin`, `financeiro`, `fornecedor`) e os parametros globais de configuracao.

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

## 4. Preparar banco quase vazio

Depois que os containers estiverem no ar, execute:

```powershell
Get-Content .\scripts\homologacao-quase-vazio.sql | docker exec -i dalba-postgres psql -U postgres -d DALBA
```

Isso remove dados operacionais de homologacao/desenvolvimento e deixa apenas:

- Usuario Admin: `admin / Admin@123`
- Usuario Custos: `financeiro / Financeiro@123`
- Usuario Fornecedor: `fornecedor / Fornecedor@123`
- Parametros de configuracao do sistema
- Cadastros minimos necessarios para o usuario fornecedor existir

## 5. Configurar backup diario

O pacote inclui dois scripts:

- `scripts/backup-dalba.ps1`: executa backup do banco e do sistema.
- `scripts/registrar-backup-diario.ps1`: cria/atualiza tarefa diaria do Windows.

Para agendar backup diario as 23:00:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\registrar-backup-diario.ps1 -ProjectDir "C:\Projetos\Dalba" -BackupDir "C:\Backups\Dalba" -Time "23:00"
```

Para executar um backup manual:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\backup-dalba.ps1 -ProjectDir "C:\Projetos\Dalba" -BackupDir "C:\Backups\Dalba"
```

O backup gera:

- Dump SQL do banco PostgreSQL.
- ZIP do sistema sem `node_modules`, `bin`, `obj`, `dist`, `.git`, `.angular` e logs.
- Retencao padrao: 14 dias.

## 6. Verificar

- Frontend: `http://<servidor>:4200/login`
- API/Swagger: `http://<servidor>:8080/swagger`
- Health check: `http://<servidor>:8080/health`

Troque as senhas dos usuarios padrao apos o primeiro login.

## 7. Recomendacoes de seguranca para homologacao/producao

- Nao exponha a porta do Postgres (`5432`) publicamente; mantenha-a acessivel apenas
  internamente.
- Publique o frontend/API atras de um proxy reverso com HTTPS (Nginx, Caddy, IIS ou
  Traefik).
- Troque as senhas dos usuarios seed assim que possivel.
- Configure SMTP/SMS/API Keys pela tela Admin "Configuracao" (nao vem no dump se o
  ambiente de origem nao tinha essas integracoes configuradas).
- Faca backup do volume `dalba-postgres-data` regularmente (ver `docs/banco-de-dados.md`
  no codigo-fonte deste pacote).

## Observacao sobre o frontend

O container `dalba-web` gera o build do Angular durante a construcao da imagem e
publica os arquivos estaticos via Nginx. A porta externa continua definida por
`WEB_PORT` no `.env`; dentro do container o Nginx escuta na porta `80`.
