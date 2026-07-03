using System.Security.Cryptography;
using System.Text;

namespace IronbugCore.Security;

public static class EncryptHelper
{
    private const int KeySize = 256;
    private const int DerivationIterations = 600_000; // OWASP 2023 para PBKDF2-HMAC-SHA256
    private const int SaltByteSize = 32;
    private const int IvByteSize = 16; // AES block size is fixed at 128 bits
    private static readonly HashAlgorithmName KdfHashAlgorithm = HashAlgorithmName.SHA256;

    public static string ToSha512(this string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return string.Empty;

        using var hashAlgorithm = SHA512.Create();
        var byteValue = Encoding.UTF8.GetBytes(password);
        var byteHash = hashAlgorithm.ComputeHash(byteValue);
        return Convert.ToBase64String(byteHash);
    }

    public static string Encrypt(string plainText, string passPhrase)
    {
        var saltStringBytes = RandomNumberGenerator.GetBytes(SaltByteSize);
        var ivStringBytes = RandomNumberGenerator.GetBytes(IvByteSize);
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, saltStringBytes, DerivationIterations, KdfHashAlgorithm, KeySize / 8);
        using var symmetricKey = Aes.Create();
        symmetricKey.KeySize = KeySize;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream();
        memoryStream.Write(saltStringBytes, 0, saltStringBytes.Length);
        memoryStream.Write(ivStringBytes, 0, ivStringBytes.Length);
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        var saltStringBytes = cipherTextBytesWithSaltAndIv.AsSpan(0, SaltByteSize).ToArray();
        var ivStringBytes = cipherTextBytesWithSaltAndIv.AsSpan(SaltByteSize, IvByteSize).ToArray();
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.AsSpan(SaltByteSize + IvByteSize).ToArray();

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, saltStringBytes, DerivationIterations, KdfHashAlgorithm, KeySize / 8);
        using var symmetricKey = Aes.Create();
        symmetricKey.KeySize = KeySize;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
        using var memoryStream = new MemoryStream(cipherTextBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        var plainTextBytes = new byte[cipherTextBytes.Length];
        var decryptedByteCount = 0;
        int read;
        while ((read = cryptoStream.Read(plainTextBytes, decryptedByteCount, plainTextBytes.Length - decryptedByteCount)) > 0)
            decryptedByteCount += read;
        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
    }
}