# ==============================================================================
# Ascend (Summit) Mod Build & Auto-Deploy Script
# Supports building Master DLL, Slave DLL, or Both into dist/
# Production Name: Ascend 2.0 | Author: Made by T.H.M.
# ==============================================================================

param(
    [ValidateSet("Master", "Slave", "Both")]
    [string]$Role = "Both",

    [ValidateSet("Master", "Slave")]
    [string]$DeployRole = "Master",

    [string]$Configuration = "Release",

    [switch]$SkipDeploy = $false
)

$ErrorActionPreference = "Stop"

# 1. Configure modern .NET 8 SDK environment
$dotnetRoot = "C:\Users\feldring\.dotnet"
$env:DOTNET_ROOT = $dotnetRoot
if ($env:PATH -notlike "*$dotnetRoot*") {
    $env:PATH = "$dotnetRoot;$env:PATH"
}

$workspaceDir = $PSScriptRoot
$projectPath = Join-Path $workspaceDir "Summit_Decompiled\Summit.csproj"
$gamePluginsDir = "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK\BepInEx\plugins"
$targetDll = Join-Path $gamePluginsDir "Summit.dll"
$targetPdb = Join-Path $gamePluginsDir "Summit.pdb"
$backupDir = Join-Path $workspaceDir "backups"
$distDir = Join-Path $workspaceDir "dist"
$binOutputDir = Join-Path $workspaceDir "Summit_Decompiled\bin\$Configuration\netstandard2.1"

if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "       Building Ascend 2.5 Mod ($Configuration)" -ForegroundColor Cyan
Write-Host "       Target Role(s): $Role | Deploy Role: $DeployRole" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 2. Check if PEAK game is running
$peakProc = Get-Process -Name "PEAK" -ErrorAction SilentlyContinue
if ($peakProc) {
    Write-Host "[NOTE] PEAK.exe (PID $($peakProc.Id)) is currently running." -ForegroundColor Yellow
    Write-Host "       Binaries will be compiled to dist/; close PEAK to update active in-game DLL." -ForegroundColor Yellow
}

# Helper to compile a specific role
function Build-ModRole([string]$targetRole, [string]$outputBaseName) {
    Write-Host "`n>>> Compiling $targetRole Mod..." -ForegroundColor Cyan
    $extraArgs = @("-c", $Configuration, "-p:SkipDeploy=true")
    if ($targetRole -eq "Slave") {
        $extraArgs += "-p:ModRole=Slave"
    }

    & "$dotnetRoot\dotnet.exe" build $projectPath @extraArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Build failed for $targetRole role with exit code $LASTEXITCODE." -ForegroundColor Red
        exit $LASTEXITCODE
    }

    $builtDll = Join-Path $binOutputDir "Summit.dll"
    $builtPdb = Join-Path $binOutputDir "Summit.pdb"
    $distDll = Join-Path $distDir "$outputBaseName.dll"
    $distPdb = Join-Path $distDir "$outputBaseName.pdb"

    Copy-Item $builtDll $distDll -Force
    if (Test-Path $builtPdb) {
        Copy-Item $builtPdb $distPdb -Force
    }

    Write-Host "[DIST] Output saved to: $distDll" -ForegroundColor Green
}

# 3. Compile Master
if ($Role -eq "Both" -or $Role -eq "Master") {
    Build-ModRole -targetRole "Master" -outputBaseName "Summit_Master"
}

# 4. Compile Slave
if ($Role -eq "Both" -or $Role -eq "Slave") {
    Build-ModRole -targetRole "Slave" -outputBaseName "AscentS"
    # Also mirror as Summit_Slave for backward compatibility
    Copy-Item (Join-Path $distDir "AscentS.dll") (Join-Path $distDir "Summit_Slave.dll") -Force
    if (Test-Path (Join-Path $distDir "AscentS.pdb")) {
        Copy-Item (Join-Path $distDir "AscentS.pdb") (Join-Path $distDir "Summit_Slave.pdb") -Force
    }
}

# 5. Generate distribution instructions in dist/
$readmePath = Join-Path $distDir "README.txt"
$readmeContent = @"
==============================================================================
                    ASCEND 2.0 - MOD NETWORKING SUITE
                             Made by T.H.M.
==============================================================================

Ascend 2.0 Features:
- Detachable Items Spawner window (Pop Out / Dock button)
- Dynamic theming engine: 6 one-click presets + live RGB color sliders
- Master inventory manager: Live view of all scouts' hands, pockets, and backpacks
- Remote item delivery: Inject any item into a scout's hands or backpack
- Granular permission control: Master grants/revokes sections per slave or globally
- Hidden Ascend Network: Slaves require 5 clicks on PEAK logo to unlock

Files:
1. Summit_Master.dll (For Host / You):
   - Keeps track of all peers in your lobby running Ascend.
   - Master controls permissions, inspects inventories, and injects items.
   - Can pop out Item Spawner into its own independent draggable window.
   - Shows live scout details under About -> Ascend Network.

2. Summit_Slave.dll (For Friends / Peers to install):
   - Controlled by Master's permission matrix.
   - Silently reports presence and receives injected items into hands/gear.
   - Rename to "Summit.dll" and place into BepInEx/plugins/ on the peer's PC.

HOW TO INSTALL FOR PEERS:
1. Send them Summit_Slave.dll.
2. Tell them to rename it to Summit.dll.
3. Place it in: <PEAK_Install_Folder>\BepInEx\plugins\Summit.dll
4. When they join your lobby, your Master mod will immediately detect them!
"@
Set-Content -Path $readmePath -Value $readmeContent -Encoding UTF8

# 6. Deploy to active game directory
if (-not $SkipDeploy) {
    $deploySourceDll = if ($DeployRole -eq "Slave") { Join-Path $distDir "AscentS.dll" } else { Join-Path $distDir "Summit_Master.dll" }
    $deploySourcePdb = if ($DeployRole -eq "Slave") { Join-Path $distDir "AscentS.pdb" } else { Join-Path $distDir "Summit_Master.pdb" }

    if (Test-Path $deploySourceDll) {
        if (-not $peakProc) {
            # Create backup if Summit.dll exists in plugins
            if (Test-Path $targetDll) {
                if (-not (Test-Path $backupDir)) {
                    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
                }
                $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
                $backupPath = Join-Path $backupDir "Summit_$timestamp.dll"
                Copy-Item $targetDll $backupPath -Force
                Write-Host "`n[BACKUP] Saved backup of game DLL to: $backupPath" -ForegroundColor DarkGray
            }

            Copy-Item $deploySourceDll $targetDll -Force
            if (Test-Path $deploySourcePdb) {
                Copy-Item $deploySourcePdb $targetPdb -Force
            }
            Write-Host "[DEPLOY] Deployed $DeployRole DLL directly to game: $targetDll" -ForegroundColor Green
        } else {
            Write-Host "`n[DEPLOY SKIPPED] PEAK is running; close PEAK and re-run to deploy $DeployRole DLL to game plugins folder." -ForegroundColor Yellow
        }
    }

    # Sync Slave to test directory if present
    $slaveTestDir = "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK_Slave\BepInEx\plugins"
    if (Test-Path $slaveTestDir) {
        try {
            if (Test-Path (Join-Path $slaveTestDir "AscentS.dll")) {
                Remove-Item (Join-Path $slaveTestDir "AscentS.dll") -Force -ErrorAction SilentlyContinue
            }
            Copy-Item (Join-Path $distDir "AscentS.dll") (Join-Path $slaveTestDir "Summit.dll") -Force
            if (Test-Path (Join-Path $distDir "AscentS.pdb")) {
                Copy-Item (Join-Path $distDir "AscentS.pdb") (Join-Path $slaveTestDir "Summit.pdb") -Force
            }
            Write-Host "[DEPLOY] Synced Slave DLL (AscentS.dll -> Summit.dll) to PEAK_Slave test folder" -ForegroundColor Green
        } catch {
            Write-Host "[DEPLOY SKIPPED] PEAK_Slave is running; close PEAK_Slave to update active in-game Slave DLL." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "`n[DEPLOY SKIPPED] -SkipDeploy specified; compiled binaries remain in dist/." -ForegroundColor Yellow
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " [SUCCESS] Build process completed successfully!" -ForegroundColor Green
Write-Host " Master Binary : dist\Summit_Master.dll" -ForegroundColor Green
Write-Host " Slave Binary  : dist\AscentS.dll (and Summit_Slave.dll)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
