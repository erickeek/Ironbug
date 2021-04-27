namespace IronBug.Mail
{
    public class MailAddress
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public MailAddress(string email, string name)
        {
            Email = email;
            Name = name;
        }
    }
}
