namespace IronBug.Mail
{
    public class Mail
    {
        public string Key { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailAddress[] Tos { get; set; }
    }
}
