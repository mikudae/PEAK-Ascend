@echo off
setlocal
powershell.exe -ExecutionPolicy Bypass -File "%~dp0build_summit.ps1" %*
if %ERRORLEVEL% neq 0 (
    echo.
    echo Build failed with error code %ERRORLEVEL%.
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo Mod successfully built and deployed!
timeout /t 3 >nul
