@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title SmartIT Pro v1.0.1 - Verification

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] .NET 8 SDK is required.
    pause
    exit /b 1
)

echo [1/3] Restoring solution...
dotnet restore SmartIT.sln
if errorlevel 1 goto :failed
dotnet restore SmartIT.Tests\SmartIT.Tests.csproj
if errorlevel 1 goto :failed

echo.
echo [2/3] Building Release configuration...
dotnet build SmartIT.sln -c Release --no-restore
if errorlevel 1 goto :failed

echo.
echo [3/3] Running tests...
dotnet test SmartIT.Tests\SmartIT.Tests.csproj -c Release --no-build
if errorlevel 1 goto :failed

echo.
echo ============================================================
echo ALL CHECKS PASSED

echo SmartIT Pro v1.0.1 is ready to run with START_SMARTIT.bat.
echo ============================================================
echo.
pause
exit /b 0

:failed
echo.
echo ============================================================
echo VERIFICATION FAILED

echo Review the first red error shown above.
echo ============================================================
echo.
pause
exit /b 1
