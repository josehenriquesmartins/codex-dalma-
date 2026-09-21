param(
    [string]$ProjectDir = "C:\Projetos\Dalba",
    [string]$BackupDir = "C:\Backups\Dalba",
    [string]$TaskName = "Dalba Backup Diario",
    [string]$Time = "23:00"
)

$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $ProjectDir "scripts\backup-dalba.ps1"
if (-not (Test-Path $scriptPath)) {
    throw "Script de backup nao encontrado em $scriptPath"
}

$actionArgs = "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -ProjectDir `"$ProjectDir`" -BackupDir `"$BackupDir`""
$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument $actionArgs
$trigger = New-ScheduledTaskTrigger -Daily -At $Time
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Force | Out-Null

Write-Host "Tarefa agendada criada/atualizada: $TaskName"
Write-Host "Horario diario: $Time"
Write-Host "Destino dos backups: $BackupDir"
