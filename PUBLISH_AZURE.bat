@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title SmartIT Pro v1.0.1 - Azure Package

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] .NET 8 SDK was not found.
    echo Install .NET 8 SDK and run this file again.
    pause
    exit /b 1
)

echo [1/4] Restoring packages...
dotnet restore SmartIT.Tests\SmartIT.Tests.csproj
if errorlevel 1 goto :failed
dotnet restore SmartIT.Web\SmartIT.Web.csproj
if errorlevel 1 goto :failed

echo.
echo [2/4] Building and testing Release configuration...
dotnet test SmartIT.Tests\SmartIT.Tests.csproj -c Release --no-restore
if errorlevel 1 goto :failed

echo.
echo [3/4] Publishing SmartIT.Web...
if exist azure-publish rmdir /s /q azure-publish
dotnet publish SmartIT.Web\SmartIT.Web.csproj -c Release --no-restore -o azure-publish
if errorlevel 1 goto :failed

echo.
echo [4/4] Creating Azure ZIP package...
if exist SmartIT-Pro-v1.0-Azure.zip del /q SmartIT-Pro-v1.0-Azure.zip
powershell -NoProfile -ExecutionPolicy Bypass -File ".\CREATE_AZURE_ZIP.ps1" -SourceDirectory ".\azure-publish" -DestinationFile ".\SmartIT-Pro-v1.0-Azure.zip"
if errorlevel 1 goto :failed

echo.
echo ============================================================
echo AZURE PACKAGE IS READY
echo File: SmartIT-Pro-v1.0-Azure.zip
echo Read AZURE_UPDATE_GUIDE.md before deploying.
echo ============================================================
echo.
pause
exit /b 0

:failed
echo.
echo ============================================================
echo [ERROR] Azure package could not be created.
echo Fix the first error shown above and run this file again.
echo ============================================================
echo.
pause
exit /b 1
