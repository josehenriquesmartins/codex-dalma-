param(
    [string]$OutputDir = "dist"
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
Set-Location $repoRoot

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$packageName = "dalba-deploy-quase-vazio-$timestamp"
$outDirFull = Join-Path $repoRoot $OutputDir
$stagingDir = Join-Path $outDirFull $packageName
$zipPath = Join-Path $outDirFull "$packageName.zip"

New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null

Write-Host "==> Exportando codigo-fonte versionado (git archive HEAD)..."
$sourceZip = Join-Path $outDirFull "_source-$timestamp.zip"
git archive --format=zip --output $sourceZip HEAD
Expand-Archive -Path $sourceZip -DestinationPath $stagingDir -Force
Remove-Item $sourceZip

Write-Host "==> Gerando .env de exemplo para homologacao..."
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

Copy-Item (Join-Path $repoRoot "scripts\INSTALL.md") (Join-Path $stagingDir "INSTALL.md") -Force

Write-Host "==> Compactando pacote final..."
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$stagingDir\*" -DestinationPath $zipPath
Remove-Item $stagingDir -Recurse -Force

Write-Host ""
Write-Host "Pacote gerado: $zipPath ($([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB)"
Write-Host "Banco quase vazio: execute scripts/homologacao-quase-vazio.sql apos subir os containers."
Write-Host "Backup diario: use scripts/registrar-backup-diario.ps1 no servidor."
