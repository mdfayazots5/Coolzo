param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot "Backend\Coolzo.Api\Coolzo.Api.csproj"
$appHostConfig = Join-Path $repoRoot "Backend\.vs\Coolzo\config\applicationhost.config"
$siteName = "Coolzo.Api"
$appPool = "Coolzo.Api AppPool"
$iisExpressPath = "C:\Program Files\IIS Express\iisexpress.exe"

$existing = Get-CimInstance Win32_Process -Filter "Name='iisexpress.exe'" |
    Where-Object { $_.CommandLine -like "*$siteName*" }

foreach ($process in $existing) {
    Stop-Process -Id $process.ProcessId -Force
}

Start-Sleep -Seconds 2

if (-not $SkipBuild) {
    & dotnet build $apiProject
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed for $apiProject"
    }
}

Start-Process -FilePath $iisExpressPath -ArgumentList @(
    "/config:$appHostConfig",
    "/site:$siteName",
    "/apppool:$appPool"
)

Start-Sleep -Seconds 4

$portCheck = Get-NetTCPConnection -LocalPort 44394 -State Listen -ErrorAction SilentlyContinue
if (-not $portCheck) {
    throw "IIS Express did not bind https://localhost:44394"
}

Write-Host "Coolzo.Api restarted on https://localhost:44394"
