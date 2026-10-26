using System.Security.Cryptography;
using System.Text;
using AESApp;
using RSAApp;

string root = Path.Combine(Path.GetTempPath(), "AES_RSA_Lab_SelfTest");
Directory.CreateDirectory(root);
string source = Path.Combine(root, "source.bin");
string encrypted = Path.Combine(root, "source.bin.aesgcm");
string decrypted = Path.Combine(root, "source.restored.bin");
byte[] original = RandomNumberGenerator.GetBytes(2 * 1024 * 1024 + 12345);
File.WriteAllBytes(source, original);

byte[] aesKey = AesService.NewKey();
string text = "AES text test: ສະບາຍດີ";
string cipherText = AesService.EncryptText(text, aesKey);
if (AesService.DecryptText(cipherText, aesKey) != text) throw new Exception("AES text round-trip failed.");
AesService.EncryptFile(source, encrypted, aesKey);
AesService.DecryptFile(encrypted, decrypted, aesKey);
string h1 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)));
string h2 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(decrypted)));
if (h1 != h2) throw new Exception("AES file round-trip failed.");
string tampered = Path.Combine(root, "source.tampered.aesgcm");
string tamperedOutput = Path.Combine(root, "tampered-output.bin");
File.Copy(encrypted, tampered, true);
using (var fs = new FileStream(tampered, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
{
    fs.Position = fs.Length - 1;
    int last = fs.ReadByte();
    fs.Position = fs.Length - 1;
    fs.WriteByte((byte)(last ^ 0x01));
}
try { AesService.DecryptFile(tampered, tamperedOutput, aesKey); throw new Exception("AES-GCM failed to detect tampering."); }
catch (CryptographicException) { Console.WriteLine("PASS AES-GCM detects modified ciphertext/tag"); }
finally { if (File.Exists(tamperedOutput)) File.Delete(tamperedOutput); if (File.Exists(tampered)) File.Delete(tampered); }
Console.WriteLine("PASS AES text encrypt/decrypt");
Console.WriteLine($"PASS AES file round-trip: {original.Length:N0} bytes; SHA-256 matched");

using var rsa = new RsaService();
string privatePem = rsa.ExportPrivatePem();
string publicPem = rsa.ExportPublicPem();
if (!RsaService.PemBytesView(publicPem).Contains("SEQUENCE")) throw new Exception("PEM DER structure view failed.");
using var privateImported = new RsaService();
privateImported.ImportPem(privatePem);
byte[] data = Encoding.UTF8.GetBytes("RSA test payload");
byte[] rsaCipher = rsa.Encrypt(data);
if (!rsa.Decrypt(rsaCipher).SequenceEqual(data)) throw new Exception("RSA encrypt/decrypt failed.");
if (!privateImported.Decrypt(rsaCipher).SequenceEqual(data)) throw new Exception("Private PEM import/decrypt failed.");
byte[] sig1 = rsa.Sign(data);
byte[] sig2 = rsa.Sign(data);
if (!rsa.Verify(data, sig1)) throw new Exception("RSA signature verification failed.");
if (!rsa.Verify(data, sig2)) throw new Exception("RSA PSS second signature verification failed.");
if (sig1.SequenceEqual(sig2)) throw new Exception("RSA-PSS signatures unexpectedly identical; rerun test.");
using var publicOnly = new RsaService();
publicOnly.ImportPem(publicPem);
if (!publicOnly.Verify(data, sig1)) throw new Exception("Public-key signature verification failed.");
try { _ = rsa.Encrypt(new byte[191]); throw new Exception("RSA 191-byte limit was not enforced."); }
catch (ArgumentOutOfRangeException) { }
Console.WriteLine("PASS RSA OAEP-SHA256 encrypt/decrypt");
Console.WriteLine("PASS RSA-PSS SHA-256 sign/verify; repeated signatures differ");
Console.WriteLine("PASS public PEM import and 190-byte limit");
Console.WriteLine("RSA benchmark on this machine:");
Console.WriteLine(RsaBenchmarks.Run(rsa, data, 100));
Console.WriteLine("ALL SELF-TESTS PASSED");

if (args.Contains("--large", StringComparer.OrdinalIgnoreCase))
{
    string largeRoot = Path.Combine(Path.GetTempPath(), "AES_RSA_Lab_LargeTest");
    Directory.CreateDirectory(largeRoot);
    string largeSource = Path.Combine(largeRoot, "one_gib.bin");
    string largeCipher = Path.Combine(largeRoot, "one_gib.bin.aesgcm");
    string largeRestored = Path.Combine(largeRoot, "one_gib.restored.bin");
    try
    {
        const long oneGiB = 1024L * 1024 * 1024;
        using (var fs = new FileStream(largeSource, FileMode.Create, FileAccess.Write, FileShare.None))
            fs.SetLength(oneGiB);
        byte[] largeKey = AesService.NewKey();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        AesService.EncryptFile(largeSource, largeCipher, largeKey);
        sw.Stop();
        double encSeconds = sw.Elapsed.TotalSeconds;
        sw.Restart();
        AesService.DecryptFile(largeCipher, largeRestored, largeKey);
        sw.Stop();
        double decSeconds = sw.Elapsed.TotalSeconds;
        string largeHash1, largeHash2;
        using (var fs = File.OpenRead(largeSource)) largeHash1 = Convert.ToHexString(SHA256.HashData(fs));
        using (var fs = File.OpenRead(largeRestored)) largeHash2 = Convert.ToHexString(SHA256.HashData(fs));
        if (largeHash1 != largeHash2) throw new Exception("1 GiB file hash comparison failed.");
        Console.WriteLine($"PASS 1 GiB AES file round-trip; source={new FileInfo(largeSource).Length:N0} bytes");
        Console.WriteLine($"Encryption: {encSeconds:F3} s; {(oneGiB / 1048576d / encSeconds):F2} MiB/s");
        Console.WriteLine($"Decryption: {decSeconds:F3} s; {(oneGiB / 1048576d / decSeconds):F2} MiB/s");
        Console.WriteLine($"SHA-256: {largeHash1}; MATCH");
    }
    finally
    {
        try { Directory.Delete(largeRoot, true); } catch { }
    }
}
