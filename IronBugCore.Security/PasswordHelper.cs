using System.Text;

namespace IronBugCore.Security
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher PasswordHasher = new PasswordHasher();
        private static readonly Random Random = new Random();

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
            var aToZ = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).ToArray();
            var vowels = "aeiou".ToCharArray();
            var consonant = aToZ.Where(w => !vowels.Contains(w)).ToArray();

            if (length < 3)
                throw new Exception("Poucos caracteres para uma senha");

            var password = new StringBuilder();
            var previousCharacterIsAConsonant = false;
            for (var i = 0; i < length - 1; i++)
            {
                if (Random.Next(0, 1) == 0 && !previousCharacterIsAConsonant)
                {
                    password.Append(consonant[Random.Next(0, consonant.Length - 1)]);
                    previousCharacterIsAConsonant = true;
                }
                else
                {
                    password.Append(vowels[Random.Next(0, vowels.Length - 1)]);
                    previousCharacterIsAConsonant = false;
                }
            }

            return $"{password}{MakeNumericPassword(1)}";
        }

        public static string MakeStrongPassword()
        {
            var letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).ToArray();
            var numbers = Enumerable.Range('0', '9' - '0' + 1).Select(i => (char)i).ToArray();
            var specialCharacters = "!@#$%¨&*()".ToCharArray();

            var password = new StringBuilder();
            for (var i = 0; i < 3; i++)
            {
                password.Append(numbers[Random.Next(0, numbers.Length - 1)]);
            }

            for (var i = 0; i < 5; i++)
            {
                password.Append(letters[Random.Next(0, letters.Length - 1)]);
            }

            for (var i = 0; i < 3; i++)
            {
                password.Append(specialCharacters[Random.Next(0, specialCharacters.Length - 1)]);
            }

            return password.ToString();
        }

        public static string MakeNumericPassword(int length)
        {
            var password = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                password.Append(Random.Next(0, 9));
            }
            return password.ToString();
        }
    }
}
