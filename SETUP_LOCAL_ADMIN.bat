@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title SmartIT Pro - Local Admin Setup

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] .NET 8 SDK was not found.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File ".\SETUP_LOCAL_SECRETS.ps1"
if errorlevel 1 (
    echo.
    echo [ERROR] Local administrator setup failed.
    pause
    exit /b 1
)

echo.
echo Local administrator settings were saved securely.
exit /b 0
