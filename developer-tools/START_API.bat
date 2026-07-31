@echo off
setlocal
cd /d "%~dp0\.."
title SmartIT Pro API
echo Developer API: http://localhost:5201/swagger
set ASPNETCORE_ENVIRONMENT=Development
dotnet restore SmartIT.API\SmartIT.API.csproj && dotnet run --project SmartIT.API\SmartIT.API.csproj --urls "http://localhost:5201"
pause
