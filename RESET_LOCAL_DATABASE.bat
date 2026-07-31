@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title SmartIT Pro - Reset Local Database

echo This will delete the local SmartIT database and all local records.
choice /C YN /M "Continue"
if errorlevel 2 exit /b 0

del /Q "SmartIT.Web\smartit-v1.db" 2>nul
del /Q "SmartIT.Web\smartit-v1.db-shm" 2>nul
del /Q "SmartIT.Web\smartit-v1.db-wal" 2>nul

echo.
echo Database reset completed.
echo Run START_SMARTIT.bat to create fresh demo data.
echo.
pause
