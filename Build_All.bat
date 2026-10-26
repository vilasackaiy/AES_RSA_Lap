@echo off
cd /d "%~dp0"
"%ProgramFiles%\dotnet\dotnet.exe" build "AES_RSA_Lab.sln" -c Release
pause
