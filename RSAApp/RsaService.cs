using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace RSAApp;

internal sealed class RsaService : IDisposable
{
    private RSA _rsa = RSA.Create(2048);
    public RSAParameters ExportParameters(bool includePrivate) => _rsa.ExportParameters(includePrivate);
    public string ExportPublicPem() => _rsa.ExportSubjectPublicKeyInfoPem();
    public string ExportPrivatePem() => _rsa.ExportPkcs8PrivateKeyPem();

    public void ImportPem(string pem)
    {
        var replacement = RSA.Create();
        try { replacement.ImportFromPem(pem); }
        catch { replacement.Dispose(); throw; }
        _rsa.Dispose();
        _rsa = replacement;
    }

    public byte[] Encrypt(byte[] data)
    {
        if (data.Length > 190)
            throw new ArgumentOutOfRangeException(nameof(data), "For a 2048-bit RSA key with OAEP-SHA256, data must be at most 190 bytes.");
        return _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public byte[] Decrypt(byte[] data) => _rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);

    public byte[] Sign(byte[] data) => _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    public bool Verify(byte[] data, byte[] signature) =>
        _rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

    public static string PemBytesView(string pem)
    {
        string body = string.Join("", pem.Split('\n').Where(x => !x.StartsWith("-----", StringComparison.Ordinal)).Select(x => x.Trim()));
        byte[] der = Convert.FromBase64String(body);
        var sb = new StringBuilder();
        sb.AppendLine($"Độ dài dữ liệu DER trong PEM: {der.Length} bytes");
        sb.AppendLine("PEM gồm nhãn BEGIN/END và phần Base64. Base64 là mã hóa biểu diễn, không phải mã hóa bảo mật.");
        sb.AppendLine("DER là định dạng nhị phân ASN.1. Mỗi TLV gồm Tag (thẻ), Length (độ dài), Value (giá trị).");
        sb.AppendLine("Offset   Hex bytes");
        for (int i = 0; i < der.Length; i += 16)
            sb.AppendLine($"{i:X6}   {Convert.ToHexString(der.AsSpan(i, Math.Min(16, der.Length - i)))}");
        sb.AppendLine();
        sb.AppendLine("Cấu trúc ASN.1 DER (offset tính theo byte trong DER đã giải mã):");
        try { AppendDerTree(sb, der, 0, der.Length, 0); }
        catch (Exception ex) { sb.AppendLine("Không thể phân tích đầy đủ cây TLV: " + ex.Message); }
        sb.AppendLine();
        sb.AppendLine("Bảng thẻ: 0x30 SEQUENCE; 0x02 INTEGER; 0x03 BIT STRING; 0x04 OCTET STRING; 0x05 NULL; 0x06 OBJECT IDENTIFIER.");
        sb.AppendLine("Cấu trúc khóa công khai RSA chứa n và e. Dữ liệu khóa riêng còn chứa d, p, q và các giá trị CRT.");
        return sb.ToString();
    }

    private static void AppendDerTree(StringBuilder sb, byte[] data, int start, int end, int depth)
    {
        int pos = start;
        while (pos < end)
        {
            int offset = pos;
            byte tag = data[pos++];
            if (pos >= end) throw new InvalidDataException("Thiếu độ dài DER.");
            int firstLength = data[pos++];
            int length;
            if ((firstLength & 0x80) == 0) length = firstLength;
            else
            {
                int count = firstLength & 0x7F;
                if (count == 0 || count > 4 || pos + count > end) throw new InvalidDataException("Độ dài DER không hợp lệ.");
                length = 0;
                for (int i = 0; i < count; i++) length = checked((length << 8) | data[pos++]);
            }
            int valueStart = pos;
            int valueEnd = checked(valueStart + length);
            if (valueEnd > end) throw new InvalidDataException("Giá trị DER vượt quá độ dài phần tử cha.");
            string name = tag switch
            {
                0x30 => "SEQUENCE",
                0x31 => "SET",
                0x02 => "INTEGER",
                0x03 => "BIT STRING",
                0x04 => "OCTET STRING",
                0x05 => "NULL",
                0x06 => "OBJECT IDENTIFIER",
                0xA0 => "CONTEXT [0]",
                _ => "tag"
            };
            sb.AppendLine($"{new string(' ', depth * 2)}@{offset:X6}: 0x{tag:X2} {name}, length={length}");
            bool parsedChild = false;
            if ((tag & 0x20) != 0)
            {
                AppendDerTree(sb, data, valueStart, valueEnd, depth + 1);
                parsedChild = true;
            }
            else if (tag == 0x03 && length > 1 && data[valueStart] == 0)
            {
                try { AppendDerTree(sb, data, valueStart + 1, valueEnd, depth + 1); parsedChild = true; }
                catch { }
            }
            else if (tag == 0x04 && length > 2 && data[valueStart] == 0x30)
            {
                try { AppendDerTree(sb, data, valueStart, valueEnd, depth + 1); parsedChild = true; }
                catch { }
            }
            if (!parsedChild && length > 0)
            {
                int preview = Math.Min(length, 24);
                sb.AppendLine($"{new string(' ', depth * 2 + 2)}value: {Convert.ToHexString(data.AsSpan(valueStart, preview))}{(length > preview ? "..." : "")}");
            }
            pos = valueEnd;
        }
    }

    public static string ParametersView(RSAParameters p, bool includePrivate)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Các thành phần khóa RSA (số nguyên unsigned big-endian; DER INTEGER có thể thêm byte 0 ở đầu):");
        Add(sb, "n (modulus)", p.Modulus);
        Add(sb, "e (public exponent)", p.Exponent);
        if (includePrivate)
        {
            Add(sb, "d (private exponent)", p.D);
            Add(sb, "p", p.P); Add(sb, "q", p.Q);
            Add(sb, "DP", p.DP); Add(sb, "DQ", p.DQ);
            Add(sb, "InverseQ", p.InverseQ);
        }
        return sb.ToString();
    }

    private static void Add(StringBuilder sb, string name, byte[]? value)
    {
        if (value is not null) sb.AppendLine($"{name}: {Convert.ToHexString(value)} ({value.Length} bytes)");
    }

    public void Dispose() => _rsa.Dispose();
}
internal static class RsaBenchmarks
{
    public static string Run(RsaService service, byte[] data, int rounds = 100)
    {
        byte[] cipher = service.Encrypt(data);
        byte[] signature = service.Sign(data);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < rounds; i++) _ = service.Encrypt(data);
        sw.Stop(); double encryptMs = sw.Elapsed.TotalMilliseconds / rounds;

        sw.Restart();
        for (int i = 0; i < rounds; i++) _ = service.Decrypt(cipher);
        sw.Stop(); double decryptMs = sw.Elapsed.TotalMilliseconds / rounds;

        sw.Restart();
        for (int i = 0; i < rounds; i++) _ = service.Sign(data);
        sw.Stop(); double signMs = sw.Elapsed.TotalMilliseconds / rounds;

        sw.Restart();
        for (int i = 0; i < rounds; i++) _ = service.Verify(data, signature);
        sw.Stop(); double verifyMs = sw.Elapsed.TotalMilliseconds / rounds;

        return $"Số lần lặp mỗi phép toán: {rounds}\r\nĐầu vào: {data.Length} bytes\r\n" +
               $"Mã hóa bằng khóa công khai RSA-OAEP-SHA256 trung bình: {encryptMs:F4} ms\r\n" +
               $"Giải mã bằng khóa riêng RSA-OAEP-SHA256 trung bình: {decryptMs:F4} ms\r\n" +
               $"Ký bằng khóa riêng RSA-PSS-SHA256 trung bình: {signMs:F4} ms\r\n" +
               $"Xác minh bằng khóa công khai RSA-PSS-SHA256 trung bình: {verifyMs:F4} ms\r\n\r\n" +
               "Lưu ý: so sánh mã hóa với giải mã và ký với xác minh theo từng cặp. " +
               "Kết quả phụ thuộc CPU, kích thước khóa, hệ điều hành và runtime; không có tỷ lệ cố định.";
}
}
