# AES / RSA Security Lab

Two Windows Forms apps for the Information Security assignment.

## Requirements
- Windows 10/11
- .NET 8 SDK (installed by setup)
- VS Code with C# Dev Kit (recommended)

## Run
Open this folder in VS Code. Double-click `Run_AES.bat` or `Run_RSA.bat` to launch an app. Double-click `Build_All.bat` to build both apps, `Run_SelfTest.bat` to run functional tests, or `Run_LargeFile_Test.bat` to repeat the 1 GiB file test.

From a terminal:

```powershell
C:\PROGRA~1\dotnet\dotnet.exe build AES_RSA_Lab.sln -c Release
C:\PROGRA~1\dotnet\dotnet.exe run --project AESApp\AESApp.csproj
C:\PROGRA~1\dotnet\dotnet.exe run --project RSAApp\RSAApp.csproj
```

If `dotnet` works in a new terminal after Windows refreshes PATH, use `dotnet` instead of the full path.

## AES app
- Generates AES-256 keys and saves/imports 32 raw key bytes.
- Encrypts/decrypts UTF-8 text using AES-GCM.
- Encrypts/decrypts large files in 1 MiB authenticated chunks, without loading the whole file into RAM.
- Reports elapsed time, input bytes, and throughput.
- Compares original and decrypted files by SHA-256.

The custom .aesgcm format is specific to this app: 20-byte header, then repeated 4-byte chunk length + ciphertext + 16-byte authentication tag. Do not use another tool to decrypt it.

## RSA app
- Generates RSA-2048 keys and exports SubjectPublicKeyInfo public PEM + PKCS#8 private PEM.
- Imports PEM files, shows decoded DER bytes and RSA parameters n/e and private values when available.
- Encrypts small data using RSA-OAEP-SHA256 and stores ciphertext as Base64.
- Enforces the assignment's 190-byte maximum for a 2048-bit RSA key with OAEP-SHA256.
- Calculates SHA-256 live as input changes.
- Signs and verifies with RSA-PSS-SHA256; PSS's random salt means repeated signatures can differ while remaining valid.
- Benchmarks public/private encryption/decryption and signing/verification operations.

**Security:** keep private.pem and AES key files secret. These are educational lab apps; do not use them as a production key-management system.
