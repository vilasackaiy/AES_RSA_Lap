@echo off
cd /d "%~dp0"
"%ProgramFiles%\dotnet\dotnet.exe" run --project "AESApp\AESApp.csproj" -c Release
if errorlevel 1 pause
