# BÁO CÁO THỰC NGHIỆM — AES VÀ RSA

**Môn học:** An toàn và bảo mật thông tin  
**Đề tài:** Mã hóa AES, RSA, SHA-256 và chữ ký số  
**Môi trường:** Windows, C# / .NET 8, Windows Forms

## 1. Mục tiêu

- Xây dựng ứng dụng Windows Forms để tạo, lưu và nhập khóa AES.
- Mã hóa/giải mã văn bản và tệp tin; đo thời gian, tốc độ và kiểm tra dữ liệu sau giải mã.
- Tạo cặp khóa RSA, xuất/nhập PEM, hiển thị byte DER và các tham số RSA.
- Mã hóa dữ liệu nhỏ bằng RSA; lưu ciphertext dạng Base64 và giải mã để so sánh.
- Đo thời gian thao tác khóa công khai/khóa riêng; tạo SHA-256 và chữ ký số.

## 2. Thiết kế AES

Ứng dụng sử dụng AES-256-GCM. Khóa có độ dài 32 byte. Mỗi tệp được chia thành các khối 1 MiB; mỗi khối có nonce riêng, tag xác thực 16 byte và dữ liệu liên kết để phát hiện sửa đổi. Cách xử lý theo luồng tránh nạp toàn bộ tệp lớn vào RAM.

Khóa AES được lưu dưới dạng 32 byte thô trong tệp .key. Cần giữ tệp khóa bí mật; nếu mất khóa thì không thể giải mã dữ liệu.

### 2.1 Quy trình kiểm thử

1. Sinh khóa AES-256 và lưu thành aes256.key.
2. Nhập lại khóa từ tệp.
3. Nhập văn bản, mã hóa thành Base64 rồi giải mã; so sánh với văn bản gốc.
4. Chọn tệp nguồn, mã hóa thành .aesgcm, rồi giải mã thành tệp mới.
5. So sánh kích thước, từng byte và SHA-256 của tệp nguồn với tệp giải mã.
6. Thử khóa sai hoặc sửa ciphertext; giải mã phải thất bại do xác thực GCM.

### 2.2 Kết quả thực nghiệm tệp 1 GiB

Lần chạy mới nhất: tệp kiểm thử 1 GiB (1,073,741,824 bytes), được tạo bằng cách đặt độ dài tệp trong hệ thống. Đây là dữ liệu thử nghiệm không đại diện cho mọi loại tệp thực tế.

| Phép đo | Kết quả |
|---|---:|
| Kích thước nguồn | 1,073,741,824 bytes |
| Thời gian mã hóa | 2.753 s |
| Tốc độ mã hóa | 371.97 MiB/s |
| Thời gian giải mã | 3.460 s |
| Tốc độ giải mã | 295.94 MiB/s |
| SHA-256 nguồn và giải mã | MATCH |
| SHA-256 giá trị | 49BC20DF15E412A64472421E13FE86FF1C5165E18B2AFCCF160D4DC19FE68A14 |

**Nhận xét:** Trong lần chạy này, mã hóa nhanh hơn giải mã. Đây là kết quả của một lần đo trên máy cụ thể, không phải quy luật cố định. Tốc độ có thể thay đổi theo CPU, ổ đĩa, dữ liệu và các tiến trình đang chạy.

## 3. Thiết kế RSA

Ứng dụng tạo khóa RSA 2048 bit, xuất khóa công khai ở định dạng SubjectPublicKeyInfo PEM (BEGIN PUBLIC KEY) và khóa riêng ở định dạng PKCS#8 PEM (BEGIN PRIVATE KEY).

- n: modulus.
- e: public exponent.
- d: private exponent.
- p, q, DP, DQ, InverseQ: các tham số riêng và tham số tối ưu CRT.
- PEM là định dạng văn bản có phần thân Base64; DER là mã hóa nhị phân ASN.1. Base64 không phải là mã hóa bảo mật.

### 3.1 Mã hóa/giải mã

Dùng RSA-OAEP với SHA-256. Với khóa RSA 2048 bit, dữ liệu đầu vào tối đa là **190 bytes**. Ciphertext được chuyển sang Base64 để hiển thị/lưu tệp. RSA phù hợp với dữ liệu nhỏ như khóa AES; không dùng RSA trực tiếp cho tệp GB.

### 3.2 SHA-256

Ứng dụng tính lại SHA-256 khi nội dung đầu vào thay đổi. Thử đổi một ký tự rồi ghi lại hai giá trị hash. Hash đầu ra có độ dài 256 bit (32 byte), hiển thị thành 64 ký tự hex.

### 3.3 Chữ ký số

Dùng RSA-PSS với SHA-256. PSS sử dụng salt ngẫu nhiên; vì vậy cùng một dữ liệu và cùng một khóa riêng có thể tạo chữ ký khác nhau ở các lần ký, trong khi chữ ký vẫn xác minh hợp lệ. Không nên tự triển khai chữ ký bằng cách “mã hóa hash bằng private key”; cần dùng API chữ ký số chuẩn.

## 4. Kết quả kiểm thử tự động

- AES: mã hóa/giải mã văn bản UTF-8 — PASS.
- AES: mã hóa/giải mã tệp nhị phân 2,109,497 bytes — PASS; SHA-256 khớp.
- AES-GCM: sửa đổi ciphertext/tag bị phát hiện — PASS.
- AES: tệp 1 GiB mã hóa/giải mã thành công — PASS; SHA-256 MATCH.
- RSA-OAEP-SHA256: mã hóa/giải mã — PASS.
- PEM: nhập khóa công khai và khóa riêng — PASS.
- RSA-PSS-SHA256: xác minh chữ ký và tạo chữ ký khác nhau qua hai lần ký — PASS.
- Giới hạn RSA-OAEP: 191 bytes bị từ chối — PASS.
- Build Release cho AESApp và RSAApp — PASS trong lần build đã kiểm tra.

## 5. Bảng ghi số liệu RSA

Benchmark thực hiện 100 vòng cho mỗi thao tác với đầu vào 16 bytes. Các số dưới đây là kết quả lần chạy mới nhất trong self-test trên máy này.

| Phép đo | Thời gian trung bình |
|---|---:|
| Public-key encryption (RSA-OAEP-SHA256) | 0.0190 ms |
| Private-key decryption (RSA-OAEP-SHA256) | 0.6021 ms |
| Private-key signing (RSA-PSS-SHA256) | 0.6266 ms |
| Public-key verification (RSA-PSS-SHA256) | 0.0210 ms |

**Nhận xét:** Trong lần đo này, public-key encryption nhanh hơn private-key decryption; public-key verification nhanh hơn private-key signing. Đây là kết quả của 100 vòng trên máy kiểm thử cụ thể, không phải tỷ lệ cố định; lần chạy khác có thể cho số liệu khác nhau do CPU và trạng thái máy. Các phép đo so sánh theo cặp vì encryption/decryption và signing/verification thực hiện những phép toán khác nhau.

## 6. Ảnh chụp màn hình cần thêm khi nộp

Chụp trực tiếp từ ứng dụng đang chạy trên Windows, lưu ảnh PNG trong thư mục `screenshots/` với các tên sau. Không sử dụng hình minh họa làm bằng chứng kiểm thử.

1. `screenshots/01-aes-encrypt.png` — giao diện AES và mã hóa/giải mã văn bản.
2. `screenshots/02-aes-file-test.png` — mã hóa/giải mã tệp lớn, thời gian và so sánh bytes/hash.
3. `screenshots/03-rsa-encrypt.png` — RSA mã hóa/giải mã dữ liệu.
4. `screenshots/04-rsa-sign-verify.png` — SHA-256, hai chữ ký khác nhau và Verify = VALID.
5. `screenshots/05-rsa-key-viewer.png` — PEM/DER viewer và tham số khóa.

## 7. Kết luận

Ứng dụng minh họa hai vai trò khác nhau: AES là mã hóa đối xứng hiệu quả cho dữ liệu lớn; RSA hỗ trợ trao đổi dữ liệu nhỏ và chữ ký số. SHA-256 tạo dấu vân tay dữ liệu, còn RSA-PSS cho phép xác minh chữ ký với khóa công khai. Thực nghiệm AES 1 GiB trong lần chạy mới nhất mất 2.753 s khi mã hóa và 3.460 s khi giải mã; SHA-256 khớp. Trước khi nộp, cần bổ sung ảnh chụp màn hình thực tế.

