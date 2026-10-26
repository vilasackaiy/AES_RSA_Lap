@echo off
cd /d "%~dp0"
"%ProgramFiles%\dotnet\dotnet.exe" run --project "RSAApp\RSAApp.csproj" -c Release
if errorlevel 1 pause
