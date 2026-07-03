using System.Security.Cryptography;
using System.Text;

namespace IronbugCore.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string password);
}

public sealed class PasswordHasher : IPasswordHasher
{
    private const byte Version1 = 1;
    private const byte Version2 = 2;
    private const int SaltSize = 16; // 128 bits
    private const int PBKDF2Iterations = 600_000;
    private static HashAlgorithmName PBKDF2HashAlgorithm => HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var inArray = new byte[1 + SaltSize + 32]; // 32 bytes for SHA256 hash
        inArray[0] = Version2;
        Buffer.BlockCopy(salt, 0, inArray, 1, SaltSize);

        KeyDerivation(password, salt, inArray.AsSpan(1 + SaltSize));

        return Convert.ToBase64String(inArray);
    }

    public bool VerifyHashedPassword(string hashedPassword, string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        if (hashedPassword == null)
            return false;

        byte[] numArray;
        try
        {
            numArray = Convert.FromBase64String(hashedPassword);
        }
        catch
        {
            return false;
        }

        if (numArray.Length < 1)
            return false;

        var version = numArray[0];

        if (version == Version2)
        {
            if (numArray.Length < 1 + SaltSize + 32) return false;
            var salt = numArray.AsSpan(1, SaltSize);
            var hash = numArray.AsSpan(1 + SaltSize, 32);

            Span<byte> expectedHash = stackalloc byte[32];
            KeyDerivation(password, salt, expectedHash);

            return CryptographicOperations.FixedTimeEquals(expectedHash, hash);
        }

        if (version == Version1)
        {
            if (numArray.Length < 1 + SaltSize) return false;
            var salt = numArray.AsSpan(1, SaltSize);
            var bytes = numArray.AsSpan(1 + SaltSize);
            var hash = HashPasswordV1(password, salt.ToArray());
            return CryptographicOperations.FixedTimeEquals(hash, bytes);
        }

        return false;
    }

    private static void KeyDerivation(string password, ReadOnlySpan<byte> salt, Span<byte> output)
    {
        Rfc2898DeriveBytes.Pbkdf2(password, salt, output, PBKDF2Iterations, PBKDF2HashAlgorithm);
    }

    private static byte[] HashPasswordV1(string password, byte[] salt)
    {
        using var hashAlgorithm = SHA256.Create();
        var input = Encoding.UTF8.GetBytes(password);
        hashAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
        hashAlgorithm.TransformFinalBlock(input, 0, input.Length);
        return hashAlgorithm.Hash!;
    }
}