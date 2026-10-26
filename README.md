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

## Tính năng

### AES-256-GCM

- Tạo khóa AES 256-bit và lưu/nạp khóa dạng tệp.
- Mã hóa, giải mã văn bản UTF-8; dữ liệu đầu ra được biểu diễn bằng Base64.
- Mã hóa/giải mã tệp lớn theo từng khối 1 MiB, không cần nạp toàn bộ tệp vào RAM.
- Hiển thị thời gian thực hiện, dung lượng đầu vào và tốc độ xử lý.
- So sánh tệp gốc với tệp giải mã bằng từng byte và SHA-256.

### RSA-2048, SHA-256 và chữ ký số

- Tạo cặp khóa RSA-2048; xuất/nạp khóa PEM công khai và riêng.
- Xem nội dung PEM, byte DER và tham số RSA đã phân tích.
- Mã hóa RSA-OAEP-SHA256, xuất bản mã Base64 và giải mã bằng khóa riêng.
- Tính SHA-256 theo thời gian thực khi nội dung đầu vào thay đổi.
- Ký và xác minh chữ ký RSA-PSS-SHA256.
- Đo hiệu năng các thao tác RSA.

## Ảnh chụp giao diện

Ảnh minh chứng cần được chụp trực tiếp từ ứng dụng đang chạy trên Windows, sau khi thực hiện thao tác tương ứng. Không dùng hình minh họa giả lập làm bằng chứng kiểm thử. Ảnh giao diện chưa được đính kèm trong phiên bản hiện tại; cần bổ sung sau khi chạy và kiểm tra thực tế các màn hình sau:

- AES: mã hóa/giải mã văn bản.
- AES: mã hóa/giải mã tệp lớn và thông tin hiệu năng.
- RSA: mã hóa/giải mã dữ liệu.
- RSA: SHA-256, ký và xác minh chữ ký hợp lệ.
- RSA: trình xem cấu trúc PEM/DER.

## Yêu cầu

- Windows 10 hoặc Windows 11.
- .NET 8 SDK (Windows Forms).
- Visual Studio 2022 hoặc VS Code có C# Dev Kit (không bắt buộc nếu dùng các tệp `.bat`).

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

## Kiểm thử

- Chạy `Run_SelfTest.bat` để chạy kiểm thử chức năng tự động.
- Chạy `Run_LargeFile_Test.bat` để thực hiện bài thử tệp lớn 1 GiB; thao tác này có thể mất thời gian và cần dung lượng đĩa trống.
- Báo cáo kết quả và hướng dẫn chụp ảnh được ghi trong [`BAO_CAO_THU_NGHIEM.md`](BAO_CAO_THU_NGHIEM.md).
- Thời gian benchmark phụ thuộc cấu hình máy; không xem số liệu từ một máy là kết quả cố định cho mọi thiết bị.

## Định dạng và giới hạn

- AES dùng AES-GCM để bảo đảm tính bí mật và xác thực dữ liệu. Định dạng `.aesgcm` là định dạng riêng của ứng dụng: header 20 byte, sau đó là các khối gồm độ dài 4 byte, ciphertext và authentication tag 16 byte.
- RSA-OAEP-SHA256 với khóa 2048-bit giới hạn dữ liệu đầu vào tối đa **190 byte**. Dùng AES cho dữ liệu/tệp lớn.
- RSA public key xuất theo SubjectPublicKeyInfo PEM; private key theo PKCS#8 PEM.
- Chữ ký số dùng RSA-PSS với SHA-256. Do salt ngẫu nhiên, hai chữ ký cho cùng dữ liệu có thể khác nhau nhưng vẫn hợp lệ.

## Cấu trúc dự án

```text
AES_RSA_Lab/
├── AESApp/                 # Ứng dụng AES-256-GCM
├── RSAApp/                 # Ứng dụng RSA, SHA-256 và chữ ký số
├── TestData/               # Mã kiểm thử tự động
├── screenshots/            # Ảnh chụp từ ứng dụng thực tế
├── AES_RSA_Lab.sln         # Solution .NET
├── BAO_CAO_THU_NGHIEM.md   # Báo cáo thử nghiệm tiếng Việt
├── Build_All.bat
├── Run_AES.bat
├── Run_RSA.bat
├── Run_SelfTest.bat
└── Run_LargeFile_Test.bat
```

## Lưu ý bảo mật

- **Không commit** `private.pem`, khóa AES, tệp dữ liệu nhạy cảm hoặc khóa thật lên GitHub.
- Đây là dự án học tập để minh họa thuật toán và quy trình; không thay thế hệ thống quản lý khóa an toàn trong môi trường production.

## Tác giả / học phần

Dự án thực hành phục vụ học phần **An toàn và bảo mật thông tin**.
