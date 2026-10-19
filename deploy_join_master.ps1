# ==============================================================================
# Deploy Standalone JoinMasterLobby Mod to PEAK_Slave
# Backs up Summit.dll if present, and sets up pure JoinMasterLobby testing
# ==============================================================================

param(
    [switch]$RestoreSummit
)

$slavePluginDir = "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK_Slave\BepInEx\plugins"
$distDir = Join-Path $PSScriptRoot "dist"
$joinMasterDll = Join-Path $distDir "JoinMasterLobby.dll"
$summitDll = Join-Path $slavePluginDir "Summit.dll"
$summitDisabled = Join-Path $slavePluginDir "Summit.dll.disabled"
$targetJoinMaster = Join-Path $slavePluginDir "JoinMasterLobby.dll"

if ($RestoreSummit) {
    if (Test-Path $targetJoinMaster) {
        Remove-Item $targetJoinMaster -Force
        Write-Host "[REMOVED] Removed JoinMasterLobby.dll from PEAK_Slave." -ForegroundColor Yellow
    }
    if (Test-Path $summitDisabled) {
        Rename-Item $summitDisabled "Summit.dll" -Force
        Write-Host "[RESTORED] Restored Summit.dll in PEAK_Slave." -ForegroundColor Green
    }
    exit 0
}

# Deploy JoinMasterLobby
if (-not (Test-Path $joinMasterDll)) {
    Write-Host "[ERROR] dist\JoinMasterLobby.dll not found. Please build first." -ForegroundColor Red
    exit 1
}

# Disable Summit.dll if active so only JoinMasterLobby runs
if (Test-Path $summitDll) {
    Rename-Item $summitDll "Summit.dll.disabled" -Force
    Write-Host "[ISOLATED] Renamed Summit.dll -> Summit.dll.disabled (Ascend mod disabled on Slave)." -ForegroundColor Cyan
}

Copy-Item $joinMasterDll $targetJoinMaster -Force
Write-Host "[DEPLOYED] Deployed pure JoinMasterLobby.dll to PEAK_Slave\BepInEx\plugins\." -ForegroundColor Green
Write-Host "PEAK_Slave is now running 100% standalone JoinMasterLobby without any Summit features." -ForegroundColor Green
Write-Host "To restore Summit later, run: .\deploy_join_master.ps1 -RestoreSummit" -ForegroundColor DarkGray
