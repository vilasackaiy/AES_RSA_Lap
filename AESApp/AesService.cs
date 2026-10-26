using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace AESApp;

internal static class AesService
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("AESGCM01");
    public const int ChunkSize = 1024 * 1024;

    public static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

    public static void SaveKey(string path, byte[] key) => File.WriteAllBytes(path, key);

    public static byte[] LoadKey(string path)
    {
        byte[] key = File.ReadAllBytes(path);
        if (key.Length != 32) throw new InvalidDataException("Tệp khóa AES-256 phải có chính xác 32 byte.");
        return key;
    }

    public static string EncryptText(string text, byte[] key)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] plain = Encoding.UTF8.GetBytes(text);
        byte[] cipher = new byte[plain.Length];
        byte[] tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plain, cipher, tag);
        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipher).ToArray());
    }

    public static string DecryptText(string base64, byte[] key)
    {
        byte[] all = Convert.FromBase64String(base64);
        if (all.Length < 28) throw new InvalidDataException("Bản mã quá ngắn.");
        byte[] plain = new byte[all.Length - 28];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(all.AsSpan(0, 12), all.AsSpan(28), all.AsSpan(12, 16), plain);
        return Encoding.UTF8.GetString(plain);
    }
    public static void EncryptFile(string inputPath, string outputPath, byte[] key, Action<long>? progress = null)
    {
        byte[] prefix = RandomNumberGenerator.GetBytes(8);
        byte[] header = new byte[20];
        Magic.CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(8, 4), ChunkSize);
        prefix.CopyTo(header, 12);

        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, FileOptions.SequentialScan);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, FileOptions.SequentialScan);
        output.Write(header);
        byte[] plain = new byte[ChunkSize];
        byte[] lenBuffer = new byte[4];
        long index = 0;
        using var aes = new AesGcm(key, 16);
        int read;
        while ((read = input.Read(plain, 0, plain.Length)) > 0)
        {
            byte[] nonce = MakeNonce(prefix, index);
            byte[] cipher = new byte[read];
            byte[] tag = new byte[16];
            byte[] aad = MakeAad(header, index, read);
            aes.Encrypt(nonce, plain.AsSpan(0, read), cipher, tag, aad);
            BinaryPrimitives.WriteInt32LittleEndian(lenBuffer, read);
            output.Write(lenBuffer);
            output.Write(cipher);
            output.Write(tag);
            index++;
            progress?.Invoke(input.Position);
        }
        output.Flush(true);
    }

    public static void DecryptFile(string inputPath, string outputPath, byte[] key, Action<long>? progress = null)
    {
        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, FileOptions.SequentialScan);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, FileOptions.SequentialScan);
        byte[] header = ReadExactly(input, 20);
        if (!header.AsSpan(0, 8).SequenceEqual(Magic)) throw new InvalidDataException("Đây không phải tệp AESGCM01 được hỗ trợ.");
        int chunkSize = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(8, 4));
        if (chunkSize != ChunkSize) throw new InvalidDataException("Kích thước khối không được hỗ trợ.");
        byte[] prefix = header.AsSpan(12, 8).ToArray();
        using var aes = new AesGcm(key, 16);
        long index = 0;
        while (input.Position < input.Length)
        {
            byte[] lenBytes = ReadExactly(input, 4);
            int len = BinaryPrimitives.ReadInt32LittleEndian(lenBytes);
            if (len < 1 || len > ChunkSize) throw new InvalidDataException("Độ dài khối mã hóa không hợp lệ.");
            byte[] cipher = ReadExactly(input, len);
            byte[] tag = ReadExactly(input, 16);
            byte[] plain = new byte[len];
            aes.Decrypt(MakeNonce(prefix, index), cipher, tag, plain, MakeAad(header, index, len));
            output.Write(plain);
            index++;
            progress?.Invoke(input.Position);
        }
        output.Flush(true);
    }

    private static byte[] MakeNonce(byte[] prefix, long index)
    {
        byte[] nonce = new byte[12];
        if (prefix.Length != 8 || index < 0 || index > uint.MaxValue) throw new InvalidDataException("Đã vượt giới hạn số khối của tệp AES-GCM.");
        prefix.CopyTo(nonce, 0);
        BinaryPrimitives.WriteUInt32BigEndian(nonce.AsSpan(8, 4), (uint)index);
        return nonce;
    }

    private static byte[] MakeAad(byte[] header, long index, int length)
    {
        byte[] aad = new byte[28];
        header.CopyTo(aad, 0);
        BinaryPrimitives.WriteUInt32BigEndian(aad.AsSpan(20, 4), checked((uint)index));
        BinaryPrimitives.WriteInt32LittleEndian(aad.AsSpan(24, 4), length);
        return aad;
    }

    private static byte[] ReadExactly(Stream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;
        while (offset < count)
        {
            int n = stream.Read(buffer, offset, count - offset);
            if (n == 0) throw new EndOfStreamException("Tệp mã hóa chưa đầy đủ hoặc bị hỏng.");
            offset += n;
        }
        return buffer;
    }
}
