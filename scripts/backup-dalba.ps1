param(
    [string]$ProjectDir = "C:\Projetos\Dalba",
    [string]$BackupDir = "C:\Backups\Dalba",
    [string]$PgContainer = "dalba-postgres",
    [string]$ApiContainer = "dalba-api",
    [string]$PgUser = "postgres",
    [string]$PgDatabase = "DALBA",
    [ValidateRange(1,3650)][int]$RetentionDays = 14
)
$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $ProjectDir).Path.TrimEnd('\')
$backupRoot = [IO.Path]::GetFullPath($BackupDir).TrimEnd('\')
if ($backupRoot.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase) -or $backupRoot -eq $root) {
    throw 'Use uma pasta de backup fora do projeto.'
}
$destination = Join-Path $backupRoot (Get-Date -Format 'yyyyMMdd-HHmmss')
New-Item -ItemType Directory -Path $destination -Force | Out-Null
function Check-Exit { if ($LASTEXITCODE -ne 0) { throw 'Comando Docker falhou; backup incompleto. Retencao nao executada.' } }
try {
    docker exec $PgContainer pg_dump -U $PgUser -d $PgDatabase -Fc --no-owner --no-privileges -f /tmp/dalba-backup.dump
    Check-Exit
    docker cp "${PgContainer}:/tmp/dalba-backup.dump" (Join-Path $destination 'database.dump')
    Check-Exit
    docker exec $ApiContainer tar -czf /tmp/dalba-uploads.tar.gz -C /app/storage uploads
    Check-Exit
    docker cp "${ApiContainer}:/tmp/dalba-uploads.tar.gz" (Join-Path $destination 'uploads.tar.gz')
    Check-Exit
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::Open((Join-Path $destination 'system.zip'), 'Create')
    try {
        Get-ChildItem -LiteralPath $root -Recurse -File -Force | Where-Object {
            $_.FullName -notmatch '\\(\.git|\.vs|node_modules|bin|obj|dist|\.angular|logs)\\'
        } | ForEach-Object {
            $relative = $_.FullName.Substring($root.Length + 1).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $relative) | Out-Null
        }
    } finally { $zip.Dispose() }
    Get-ChildItem -LiteralPath $destination -File | Get-FileHash -Algorithm SHA256 |
        Select-Object Hash,@{Name='File';Expression={Split-Path $_.Path -Leaf}} |
        ConvertTo-Json | Set-Content (Join-Path $destination 'manifest.json') -Encoding UTF8
    'Backup completo' | Set-Content (Join-Path $destination 'SUCCESS')
    Get-ChildItem -LiteralPath $backupRoot -Directory | Where-Object {
        $_.Name -match '^\d{8}-\d{6}$' -and $_.CreationTime -lt (Get-Date).AddDays(-$RetentionDays) -and
        (Test-Path -LiteralPath (Join-Path $_.FullName 'SUCCESS'))
    } | ForEach-Object {
        $target = [IO.Path]::GetFullPath($_.FullName)
        if ([IO.Path]::GetDirectoryName($target) -ne $backupRoot) { throw 'Destino de retencao invalido.' }
        Remove-Item -LiteralPath $target -Recurse -Force
    }
    Write-Host "Backup concluido: $destination"
} catch {
    $_.Exception.Message | Set-Content (Join-Path $destination 'FAILED.txt')
    throw
}
