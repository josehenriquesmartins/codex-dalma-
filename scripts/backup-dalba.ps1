param(
    [string]$ProjectDir = "C:\Projetos\Dalba",
    [string]$BackupDir = "C:\Backups\Dalba",
    [string]$PgContainer = "dalba-postgres",
    [string]$PgUser = "postgres",
    [string]$PgDatabase = "DALBA",
    [int]$RetentionDays = 14
)

$ErrorActionPreference = "Stop"

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$dayDir = Join-Path $BackupDir $timestamp
$dbDir = Join-Path $dayDir "database"
$systemDir = Join-Path $dayDir "system"

New-Item -ItemType Directory -Force -Path $dbDir, $systemDir | Out-Null

$dbDump = Join-Path $dbDir "dalba-$timestamp.sql"
Write-Host "Gerando backup do banco em $dbDump"
docker exec $PgContainer pg_dump -U $PgUser -d $PgDatabase --no-owner --no-privileges |
    Out-File -FilePath $dbDump -Encoding utf8

if ($LASTEXITCODE -ne 0) {
    throw "Falha ao executar pg_dump no container $PgContainer."
}

$systemZip = Join-Path $systemDir "dalba-sistema-$timestamp.zip"
Write-Host "Gerando backup do sistema em $systemZip"
$exclude = @(
    "\\.git\\",
    "\\node_modules\\",
    "\\bin\\",
    "\\obj\\",
    "\\dist\\",
    "\\.angular\\",
    "\\logs\\"
)

$files = Get-ChildItem -Path $ProjectDir -Recurse -File | Where-Object {
    $path = $_.FullName
    -not ($exclude | Where-Object { $path -match $_ })
}

Compress-Archive -Path $files.FullName -DestinationPath $systemZip -Force

Write-Host "Removendo backups com mais de $RetentionDays dias em $BackupDir"
Get-ChildItem -Path $BackupDir -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-$RetentionDays) } |
    Remove-Item -Recurse -Force

Write-Host "Backup concluido: $dayDir"
