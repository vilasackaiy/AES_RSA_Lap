Họ va tên: Panyasack Vialasck

MSSV:LAOS225001

LƠP: K59KMT


# AES / RSA Security Lab

Ứng dụng thực hành môn **An toàn và bảo mật thông tin**: mã hóa đối xứng AES-256-GCM, mã hóa khóa công khai RSA-2048, băm SHA-256 và chữ ký số RSA-PSS. Giao diện desktop viết bằng C# Windows Forms trên .NET 8.


## Nội dung

- [Tính năng](#tính-năng)
- [Ảnh chụp giao diện](#ảnh-chụp-giao-diện)
- [Yêu cầu](#yêu-cầu)
- [Cài đặt và chạy](#cài-đặt-và-chạy)
- [Kiểm thử](#kiểm-thử)
- [Định dạng và giới hạn](#định-dạng-và-giới-hạn)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Lưu ý bảo mật](#lưu-ý-bảo-mật)


## Bài 1

### AES-256-GCM

- Tạo khóa AES 256-bit và lưu/nạp khóa dạng tệp.
- Mã hóa, giải mã văn bản UTF-8; dữ liệu đầu ra được biểu diễn bằng Base64.
- Mã hóa/giải mã tệp lớn theo từng khối 1 MiB, không cần nạp toàn bộ tệp vào RAM.
- Hiển thị thời gian thực hiện, dung lượng đầu vào và tốc độ xử lý.
- So sánh tệp gốc với tệp giải mã bằng từng byte và SHA-256.


## Ảnh chụp giao diện
1 AES — mã hóa/giải mã văn bản

<img width="960" height="540" alt="1" src="https://github.com/user-attachments/assets/8e456c07-2bf3-48d0-817c-615695e19735" />

2 AES — tệp lớn, thời gian và so sánh



<img width="960" height="540" alt="3" src="https://github.com/user-attachments/assets/ffb513fb-7c2e-421a-9f38-ef4cf9442e42" />



<img width="960" height="540" alt="4" src="https://github.com/user-attachments/assets/6bd44d55-af3c-4ca5-81e3-6c9810a4c754" />



<img width="960" height="540" alt="5" src="https://github.com/user-attachments/assets/39e0d27d-8893-498d-a584-636d0d3246a3" />

## Bài2
### RSA-2048, SHA-256 và chữ ký số

- Tạo cặp khóa RSA-2048; xuất/nạp khóa PEM công khai và riêng.
- Xem nội dung PEM, byte DER và tham số RSA đã phân tích.
- Mã hóa RSA-OAEP-SHA256, xuất bản mã Base64 và giải mã bằng khóa riêng.
- Tính SHA-256 theo thời gian thực khi nội dung đầu vào thay đổi.
- Ký và xác minh chữ ký RSA-PSS-SHA256.
- Đo hiệu năng các thao tác RSA.

## Ảnh chụp giao diện


1 RSA — mã hóa/giải mã

<img width="483" height="355" alt="{C21C4813-FE0B-4F84-A48E-51FC53DBC687}" src="https://github.com/user-attachments/assets/87ecc342-d309-461c-b715-feb1cff88871" />


2 SHA-256 và chữ ký số


<img width="960" height="540" alt="{11543B14-AC7E-47EB-9C5E-589F50B3072D}" src="https://github.com/user-attachments/assets/f8d31d35-ce32-4d2b-adef-2b7c294fdf61" />



3 Trình xem PEM/DER

<img width="960" height="540" alt="{A8E23245-3039-43F4-A194-E1D2B9EE8D3F}" src="https://github.com/user-attachments/assets/effb9e67-fa46-4089-95a3-88a0e4689d1c" />





## Cài đặt và chạy

1. Tải hoặc clone repository về máy Windows.
2. Mở thư mục dự án trong terminal.
3. Chạy `Build_All.bat` để build solution.
4. Chạy `Run_AES.bat` để mở ứng dụng AES hoặc `Run_RSA.bat` để mở ứng dụng RSA.

Có thể chạy bằng PowerShell:

```powershell
dotnet build AES_RSA_Lab.sln -c Release
dotnet run --project AESApp\AESApp.csproj
dotnet run --project RSAApp\RSAApp.csproj
```







