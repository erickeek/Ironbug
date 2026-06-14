using System.Security.Cryptography;
using System.Text;

namespace IronbugCore.Security;

public static class PasswordHelper
{
    private static readonly PasswordHasher PasswordHasher = new();
    private static readonly char[] Vowels = "aeiou".ToCharArray();
    private static readonly char[] Consonants = "bcdfghjklmnpqrstvwxyz".ToCharArray();

    public static string Encrypt(this string password)
    {
        return PasswordHasher.HashPassword(password);
    }

    public static bool VerifyHashedPassword(string hashedPassword, string password)
    {
        return PasswordHasher.VerifyHashedPassword(hashedPassword, password);
    }

    public static string MakePronounceablePassword(int length)
    {
        if (length < 3)
            throw new ArgumentException("Poucos caracteres para uma senha", nameof(length));

        var password = new StringBuilder(length);
        var previousCharacterIsAConsonant = false;

        for (var i = 0; i < length - 1; i++)
        {
            // Using 50/50 chance for consonant if previous was not consonant
            if (GetRandomInt(0, 2) == 0 && !previousCharacterIsAConsonant)
            {
                password.Append(Consonants[GetRandomInt(0, Consonants.Length)]);
                previousCharacterIsAConsonant = true;
            }
            else
            {
                password.Append(Vowels[GetRandomInt(0, Vowels.Length)]);
                previousCharacterIsAConsonant = false;
            }
        }

        return $"{password}{MakeNumericPassword(1)}";
    }

    public static string MakeStrongPassword(int length = 12)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
        return string.Create(length, chars, (span, alphabet) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = alphabet[RandomNumberGenerator.GetInt32(0, alphabet.Length)];
            }
        });
    }

    public static string MakeNumericPassword(int length)
    {
        return string.Create<object?>(length, null, (span, _) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
            }
        });
    }

    private static int GetRandomInt(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }
}