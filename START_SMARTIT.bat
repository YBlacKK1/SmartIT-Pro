@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title SmartIT Pro v1.0.1 - Local Server

where dotnet >nul 2>nul
if errorlevel 1 (
    echo.
    echo [ERROR] .NET SDK was not found.
    echo Install .NET 8 SDK and run this file again.
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

set "HAS_LOCAL_ADMIN="
for /f "delims=" %%i in ('dotnet user-secrets list --project SmartIT.Web\SmartIT.Web.csproj 2^>nul ^| findstr /B /C:"Seed:AdminPassword ="') do set "HAS_LOCAL_ADMIN=1"
if not defined HAS_LOCAL_ADMIN (
    echo.
    echo Local administrator account is not configured yet.
    call "%~dp0SETUP_LOCAL_ADMIN.bat"
    if errorlevel 1 goto :failed
)

echo ============================================================
echo   SmartIT Pro v1.0.1 - Foundation Update
echo ============================================================
echo.
echo [1/3] Restoring NuGet packages...
dotnet restore SmartIT.Web\SmartIT.Web.csproj
if errorlevel 1 goto :failed

echo.
echo [2/3] Building the web application...
dotnet build SmartIT.Web\SmartIT.Web.csproj -c Debug --no-restore
if errorlevel 1 goto :failed

echo.
echo [3/3] Starting SmartIT Pro at http://localhost:5101
echo The browser will open automatically. Keep this window open.
echo Press Ctrl+C to stop the application.
echo.

start "" powershell -NoProfile -WindowStyle Hidden -Command "$url='http://localhost:5101'; for($i=0; $i -lt 90; $i++){ try { $r=Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 2; if($r.StatusCode -ge 200){ Start-Process $url; exit } } catch {} Start-Sleep -Seconds 1 }; Start-Process $url"
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project SmartIT.Web\SmartIT.Web.csproj --no-build --urls "http://localhost:5101"
exit /b %errorlevel%

:failed
echo.
echo ============================================================
echo [ERROR] SmartIT Pro could not be prepared.
echo Read the error shown above, then run VERIFY_PROJECT.bat.
echo ============================================================
echo.
pause
exit /b 1
