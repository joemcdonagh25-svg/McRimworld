@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Restore.ps1" %*
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Restore failed -- see messages above.
)
pause
endlocal
