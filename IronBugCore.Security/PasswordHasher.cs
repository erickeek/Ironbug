using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace IronBugCore.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string hashedPassword, string password);
    }

    public sealed class PasswordHasher : IPasswordHasher
    {
        private static byte Version => 1;
        private static int SaltSize => 128 / 8; // 128 bits
        private HashAlgorithmName HashAlgorithmName { get; } = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = HashPasswordWithSalt(password, salt);

            var inArray = new byte[1 + SaltSize + hash.Length];
            inArray[0] = Version;
            Buffer.BlockCopy(salt, 0, inArray, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, inArray, 1 + SaltSize, hash.Length);

            return Convert.ToBase64String(inArray);
        }

        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (hashedPassword == null)
                return false;

            Span<byte> numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length < 1)
                return false;

            var version = numArray[0];
            if (version > Version)
                return false;

            var salt = numArray.Slice(1, SaltSize).ToArray();
            var bytes = numArray.Slice(1 + SaltSize).ToArray();

            var hash = HashPasswordWithSalt(password, salt);

            return FixedTimeEquals(hash, bytes);
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using var hashAlgorithm = HashAlgorithm.Create(HashAlgorithmName.Name);
            var input = Encoding.UTF8.GetBytes(password);
            hashAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
            hashAlgorithm.TransformFinalBlock(input, 0, input.Length);
            var hash = hashAlgorithm.Hash;

            return hash;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool FixedTimeEquals(IReadOnlyList<byte> left, IReadOnlyList<byte> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            var length = left.Count;
            var accum = 0;

            for (var i = 0; i < length; i++)
            {
                accum |= left[i] - right[i];
            }

            return accum == 0;
        }
    }
}