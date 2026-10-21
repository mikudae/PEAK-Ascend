# ==============================================================================
# Launch Slave PEAK Instance
# Launches second instance of PEAK running Summit_Slave / AscentS in windowed mode
# ==============================================================================

param(
    [switch]$SyncLatest = $false
)

$slaveDir = "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK_Slave"
$slaveExe = Join-Path $slaveDir "PEAK.exe"

if (-not (Test-Path $slaveExe)) {
    Write-Host "[ERROR] PEAK_Slave executable not found at: $slaveExe" -ForegroundColor Red
    exit 1
}

$slavePlugin = Join-Path $slaveDir "BepInEx\plugins\AscentS.dll"
$legacyPlugin = Join-Path $slaveDir "BepInEx\plugins\Summit.dll"

# Only sync dist binary if explicitly requested or if no mod DLL exists at all
if ($SyncLatest -or ((-not (Test-Path $slavePlugin)) -and (-not (Test-Path $legacyPlugin)))) {
    $distSlave = Join-Path $PSScriptRoot "dist\AscentS.dll"
    if (-not (Test-Path $distSlave)) {
        $distSlave = Join-Path $PSScriptRoot "dist\Summit_Slave.dll"
    }
    if (Test-Path $distSlave) {
        Copy-Item $distSlave $slavePlugin -Force
        Write-Host "[SYNC] Synced latest AscentS.dll to PEAK_Slave plugins folder." -ForegroundColor Green
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "     Launching Second Instance (Slave Mod)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Directory : $slaveDir" -ForegroundColor DarkGray
Write-Host "Mode      : Windowed (1280x720)`n" -ForegroundColor DarkGray

Start-Process -FilePath $slaveExe -ArgumentList "-screen-width 1280 -screen-height 720 -screen-fullscreen 0" -WorkingDirectory $slaveDir

Write-Host "[SUCCESS] Slave PEAK instance launched!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. In your Main (Master) game, host a lobby or start a run and check the Room Code." -ForegroundColor Yellow
Write-Host "2. In this Slave game, choose 'Join Game' and type the Master's Room Code." -ForegroundColor Yellow
Write-Host "3. Open the Larp menu (F1) on Master -> About tab -> 'Larp Network' to see the detected Slave!" -ForegroundColor Yellow
