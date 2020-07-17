namespace IronBug.Security
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher PasswordHasher = new PasswordHasher();

        public static string Encrypt(this string password)
        {
            return PasswordHasher.HashPassword(password);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            return PasswordHasher.VerifyHashedPassword(hashedPassword, password);
        }
    }
}
