@echo off
cd /d "%~dp0"
"%ProgramFiles%\dotnet\dotnet.exe" run --project "TestData\SelfTest.csproj" -c Release
pause
