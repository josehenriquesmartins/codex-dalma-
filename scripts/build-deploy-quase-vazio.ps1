param([string]$OutputDir = 'dist', [string]$PgHost = 'localhost', [int]$PgPort = 5432)
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
Set-Location $repoRoot
$settings = @{}
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') { $settings[$matches[1].Trim()] = $matches[2].Trim().Trim('"').Trim("'") }
}
$env:PGPASSWORD = $settings['POSTGRES_PASSWORD']
$env:PGCONNECT_TIMEOUT = '5'
$dbUser = $settings['POSTGRES_USER']
$sourceDb = $settings['POSTGRES_DB']
$temporaryDb = 'dalba_package_' + [guid]::NewGuid().ToString('N')
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$out = [IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDir))
$stage = Join-Path $out "dalba-deploy-quase-vazio-$timestamp"
$zipPath = "$stage.zip"
New-Item -ItemType Directory -Path $stage -Force | Out-Null
function Check-Exit { if ($LASTEXITCODE -ne 0) { throw 'Falha ao gerar/validar o pacote.' } }
try {
    git archive --format=zip --output "$out/source-$timestamp.zip" HEAD
    Check-Exit
    Expand-Archive "$out/source-$timestamp.zip" $stage
    Remove-Item -LiteralPath "$out/source-$timestamp.zip"
    New-Item -ItemType Directory -Path "$stage/deploy" -Force | Out-Null
    $schema = "$stage/deploy/schema.sql"
    $data = "$stage/deploy/config.sql"
    pg_dump -h $PgHost -p $PgPort -U $dbUser -d $sourceDb --schema-only --no-owner --no-privileges -f $schema
    Check-Exit
    pg_dump -h $PgHost -p $PgPort -U $dbUser -d $sourceDb --data-only --no-owner --no-privileges -t parametros_sistema -t categorias -t documentos_tipos -f $data
    Check-Exit
    createdb -h $PgHost -p $PgPort -U $dbUser $temporaryDb
    Check-Exit
    psql -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb -v ON_ERROR_STOP=1 -f $schema -f $data > "$out/validation-$timestamp.log"
    Check-Exit
    foreach ($migration in @('02-add-password-reset-tokens.sql', '04-add-notification-sender.sql', '05-require-contract-per-monthly-submission.sql', '06-add-boleto-financeiro-liberacoes.sql', '07-add-af-financeiro-liberacoes.sql')) {
        psql -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb -v ON_ERROR_STOP=1 -f "$repoRoot/database/$migration" >> "$out/validation-$timestamp.log"
        Check-Exit
    }
    psql -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb -v ON_ERROR_STOP=1 -f "$PSScriptRoot/homologacao-quase-vazio.sql" >> "$out/validation-$timestamp.log"
    Check-Exit
    $counts = psql -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb -At -c 'select (select count(*) from usuarios), (select count(*) from contratos), (select count(*) from documentos_enviados), (select count(*) from financeiro_liberacoes);'
    Check-Exit
    if ($counts.Trim() -ne '3|0|0|0') { throw "Contagens inesperadas: $counts" }
    $sourceConfig = @(psql -h $PgHost -p $PgPort -U $dbUser -d $sourceDb -At -c 'select md5(row_to_json(p)::text) from parametros_sistema p order by chave;')
    Check-Exit
    $targetConfig = @(psql -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb -At -c 'select md5(row_to_json(p)::text) from parametros_sistema p order by chave;')
    Check-Exit
    foreach ($item in $sourceConfig) { if ($item -notin $targetConfig) { throw 'Configuracao de origem nao preservada.' } }
    Write-Host "Validado: 3 usuarios, operacao vazia, $($sourceConfig.Count) configuracoes de origem preservadas."
    pg_dump -h $PgHost -p $PgPort -U $dbUser -d $temporaryDb --no-owner --no-privileges -f "$stage/deploy/database.sql"
    Check-Exit
    Remove-Item -LiteralPath $schema,$data
    Copy-Item "$PSScriptRoot/INSTALL.md" "$stage/INSTALL.md"
    $keyBytes = New-Object byte[] 48
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($keyBytes)
    $rng.Dispose()
    @"
POSTGRES_USER=postgres
POSTGRES_PASSWORD=DEFINA_UMA_SENHA
POSTGRES_DB=DALBA
POSTGRES_PORT=5432
API_PORT=8080
WEB_PORT=4200
JWT_ISSUER=Dalba.Financeiro.Api
JWT_AUDIENCE=Dalba.Financeiro.Frontend
JWT_KEY=$([Convert]::ToBase64String($keyBytes))
"@ | Set-Content "$stage/.env.homologacao.example" -Encoding UTF8
    git rev-parse HEAD | Set-Content "$stage/deploy/COMMIT.txt"
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($stage, $zipPath)
    Get-FileHash $zipPath -Algorithm SHA256 | Select-Object Hash | Format-List
    Write-Host "Pacote validado: $zipPath"
} finally {
    dropdb -h $PgHost -p $PgPort -U $dbUser --if-exists $temporaryDb
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
