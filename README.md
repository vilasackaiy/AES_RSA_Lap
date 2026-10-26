Hแป va tรชn: Panyasack Vialasck

MSSV:LAOS225001

Lฦ P: K59KMT


# AES / RSA Security Lab

แปจng dแปฅng thแปฑc hร nh mรดn **An toร n vร  bแบฃo mแบญt thรดng tin**: mรฃ hรณa ฤ‘แป‘i xแปฉng AES-256-GCM, mรฃ hรณa khรณa cรดng khai RSA-2048, bฤm SHA-256 vร  chแปฏ kรฝ sแป‘ RSA-PSS. Giao diแปn desktop viแบฟt bแบฑng C# Windows Forms trรชn .NET 8.


## Nแปi dung

- [Tรญnh nฤng](#tรญnh-nฤng)
- [แบขnh chแปฅp giao diแปn](#แบฃnh-chแปฅp-giao-diแปn)
- [Yรชu cแบงu](#yรชu-cแบงu)
- [Cร i ฤ‘แบทt vร  chแบกy](#cร i-ฤ‘แบทt-vร -chแบกy)
- [Kiแปm thแปญ](#kiแปm-thแปญ)
- [ฤแปnh dแบกng vร  giแปi hแบกn](#ฤ‘แปnh-dแบกng-vร -giแปi-hแบกn)
- [Cแบฅu trรบc dแปฑ รกn](#cแบฅu-trรบc-dแปฑ-รกn)
- [Lฦฐu รฝ bแบฃo mแบญt](#lฦฐu-รฝ-bแบฃo-mแบญt)


## Bร i 1

### AES-256-GCM

- Tแบกo khรณa AES 256-bit vร  lฦฐu/nแบกp khรณa dแบกng tแปp.
- Mรฃ hรณa, giแบฃi mรฃ vฤn bแบฃn UTF-8; dแปฏ liแปu ฤ‘แบงu ra ฤ‘ฦฐแปฃc biแปu diแป…n bแบฑng Base64.
- Mรฃ hรณa/giแบฃi mรฃ tแปp lแปn theo tแปซng khแป‘i 1 MiB, khรดng cแบงn nแบกp toร n bแป tแปp vร o RAM.
- Hiแปn thแป thแปi gian thแปฑc hiแปn, dung lฦฐแปฃng ฤ‘แบงu vร o vร  tแป‘c ฤ‘แป xแปญ lรฝ.
- So sรกnh tแปp gแป‘c vแปi tแปp giแบฃi mรฃ bแบฑng tแปซng byte vร  SHA-256.


## แบขnh chแปฅp giao diแปn
1 AES โ€” mรฃ hรณa/giแบฃi mรฃ vฤn bแบฃn

<img width="960" height="540" alt="1" src="https://github.com/user-attachments/assets/8e456c07-2bf3-48d0-817c-615695e19735" />

2 AES โ€” tแปp lแปn, thแปi gian vร  so sรกnh



<img width="960" height="540" alt="3" src="https://github.com/user-attachments/assets/ffb513fb-7c2e-421a-9f38-ef4cf9442e42" />



<img width="960" height="540" alt="4" src="https://github.com/user-attachments/assets/6bd44d55-af3c-4ca5-81e3-6c9810a4c754" />



<img width="960" height="540" alt="5" src="https://github.com/user-attachments/assets/39e0d27d-8893-498d-a584-636d0d3246a3" />

## Bร i2
### RSA-2048, SHA-256 vร  chแปฏ kรฝ sแป‘

- Tแบกo cแบทp khรณa RSA-2048; xuแบฅt/nแบกp khรณa PEM cรดng khai vร  riรชng.
- Xem nแปi dung PEM, byte DER vร  tham sแป‘ RSA ฤ‘รฃ phรขn tรญch.
- Mรฃ hรณa RSA-OAEP-SHA256, xuแบฅt bแบฃn mรฃ Base64 vร  giแบฃi mรฃ bแบฑng khรณa riรชng.
- Tรญnh SHA-256 theo thแปi gian thแปฑc khi nแปi dung ฤ‘แบงu vร o thay ฤ‘แป•i.
- Kรฝ vร  xรกc minh chแปฏ kรฝ RSA-PSS-SHA256.
- ฤo hiแปu nฤng cรกc thao tรกc RSA.

## แบขnh chแปฅp giao diแปn


1 RSA โ€” mรฃ hรณa/giแบฃi mรฃ

<img width="483" height="355" alt="{C21C4813-FE0B-4F84-A48E-51FC53DBC687}" src="https://github.com/user-attachments/assets/87ecc342-d309-461c-b715-feb1cff88871" />


2 SHA-256 vร  chแปฏ kรฝ sแป‘


<img width="960" height="540" alt="{11543B14-AC7E-47EB-9C5E-589F50B3072D}" src="https://github.com/user-attachments/assets/f8d31d35-ce32-4d2b-adef-2b7c294fdf61" />



3 Trรฌnh xem PEM/DER

<img width="960" height="540" alt="{A8E23245-3039-43F4-A194-E1D2B9EE8D3F}" src="https://github.com/user-attachments/assets/effb9e67-fa46-4089-95a3-88a0e4689d1c" />





## Cร i ฤ‘แบทt vร  chแบกy

1. Tแบฃi hoแบทc clone repository vแป mรกy Windows.
2. Mแป thฦฐ mแปฅc dแปฑ รกn trong terminal.
3. Chแบกy `Build_All.bat` ฤ‘แป build solution.
4. Chแบกy `Run_AES.bat` ฤ‘แป mแป แปฉng dแปฅng AES hoแบทc `Run_RSA.bat` ฤ‘แป mแป แปฉng dแปฅng RSA.

Cรณ thแป chแบกy bแบฑng PowerShell:

```powershell
dotnet build AES_RSA_Lab.sln -c Release
dotnet run --project AESApp\AESApp.csproj
dotnet run --project RSAApp\RSAApp.csproj
```







