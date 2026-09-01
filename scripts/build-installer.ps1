<#
.SYNOPSIS
    Gera o pacote de instalacao do DALBA para o servidor de homologacao do cliente.

.DESCRIPTION
    Empacota o codigo-fonte versionado (via git archive), o dump completo do banco
    de dados local atual e um passo a passo de instalacao em um unico .zip pronto
    para ser transferido e executado no servidor do cliente via Docker Compose.

.PARAMETER OutputDir
    Pasta onde o pacote sera gerado. Padrao: dist (ja esta no .gitignore).

.PARAMETER PgContainer
    Nome do container Docker do PostgreSQL de onde o banco sera exportado (dump roda
    via 'docker exec', dentro da rede do container). Padrao: dalba-postgres.
    ATENCAO: o banco com os dados de fornecedores importados fica no container Docker,
    NAO no PostgreSQL nativo do Windows (pg17) usado pela API em modo 'dotnet run' -
    os dois bancos DALBA tem conteudos diferentes neste ambiente (ver docs/deploy.md).

.PARAMETER PgUser / PgDatabase
    Usuario/banco dentro do container. Padrao: postgres / DALBA.

.PARAMETER SkipDump
    Gera apenas o pacote de codigo/instalacao, sem exportar o banco de dados.

.EXAMPLE
    ./scripts/build-installer.ps1
#>
param(
    [string]$OutputDir = "dist",
    [string]$PgContainer = "dalba-postgres",
    [string]$PgUser = "postgres",
    [string]$PgDatabase = "DALBA",
    [switch]$SkipDump
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
Set-Location $repoRoot

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$packageName = "dalba-instalador-$timestamp"
$outDirFull = Join-Path $repoRoot $OutputDir
$stagingDir = Join-Path $outDirFull $packageName
$zipPath = Join-Path $outDirFull "$packageName.zip"

New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null

# 1. Codigo-fonte versionado (evita lixo local/gitignored, ex.: node_modules, bin/obj)
Write-Host "==> Exportando codigo-fonte versionado (git archive HEAD)..."
$sourceZip = Join-Path $outDirFull "_source-$timestamp.zip"
git archive --format=zip --output $sourceZip HEAD
Expand-Archive -Path $sourceZip -DestinationPath $stagingDir -Force
Remove-Item $sourceZip

# 2. Dump do banco de dados atual (container Docker, que e onde os dados reais - ex.
#    fornecedores importados - realmente estao; o pg17 nativo do Windows e um banco
#    separado usado so pela API em 'dotnet run', ver docs/deploy.md)
if (-not $SkipDump) {
    Write-Host "==> Exportando dump do banco '$PgDatabase' via container '$PgContainer'..."

    $running = docker ps --filter "name=^/$PgContainer$" --format "{{.Names}}"
    if (-not $running) {
        throw "Container '$PgContainer' nao esta em execucao. Suba-o com 'docker compose up -d postgres' antes de gerar o instalador, ou use -SkipDump."
    }

    $dumpDir = Join-Path $stagingDir "database-dump"
    New-Item -ItemType Directory -Force -Path $dumpDir | Out-Null
    $dumpFile = Join-Path $dumpDir "dalba-dump-$timestamp.sql"

    docker exec $PgContainer pg_dump -U $PgUser -d $PgDatabase --no-owner --no-privileges |
        Out-File -FilePath $dumpFile -Encoding utf8
    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump (via docker exec) falhou com codigo $LASTEXITCODE"
    }
    Write-Host "    Dump gerado: $dumpFile ($([math]::Round((Get-Item $dumpFile).Length / 1MB, 2)) MB)"
} else {
    Write-Host "==> -SkipDump ativo: pacote gerado sem dump de banco de dados."
}

# 3. .env de exemplo para o cliente, com chave JWT forte e unica gerada agora
Write-Host "==> Gerando .env de exemplo para o servidor de homologacao..."
$jwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 48 | ForEach-Object { [char]$_ })
@"
POSTGRES_USER=postgres
POSTGRES_PASSWORD=troque_esta_senha_antes_de_subir
POSTGRES_DB=DALBA
POSTGRES_PORT=5432
API_PORT=8080
WEB_PORT=4200
JWT_ISSUER=Dalba.Financeiro.Api
JWT_AUDIENCE=Dalba.Financeiro.Frontend
JWT_KEY=$jwtKey
"@ | Set-Content -Path (Join-Path $stagingDir ".env.homologacao.example") -Encoding utf8

# 4. Passo a passo de instalacao
Copy-Item (Join-Path $repoRoot "scripts\INSTALL.md") (Join-Path $stagingDir "INSTALL.md") -Force

# 5. Compactar pacote final
Write-Host "==> Compactando pacote final..."
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$stagingDir\*" -DestinationPath $zipPath
Remove-Item $stagingDir -Recurse -Force

Write-Host ""
Write-Host "Pacote gerado: $zipPath ($([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB)"
Write-Host "Contem codigo-fonte, docker-compose.yml, scripts SQL, dump do banco atual e INSTALL.md."
