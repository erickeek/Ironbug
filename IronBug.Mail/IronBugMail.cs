using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace IronBug.Mail
{
    public class IronBugMail
    {
        private const string Url = "http://sendmail.ironbug.com.br/Email/Send";
        private const string MediaType = "application/json";

        private readonly string _key;

        public IronBugMail(string key)
        {
            _key = key;
        }

        public HttpResponseMessage Send(MailAddress[] tos, string subject, string body)
        {
            var mail = new Mail
            {
                Key = _key,
                Tos = tos,
                Subject = subject,
                Body = body
            };

            var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(mail), Encoding.UTF8, MediaType);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
            return client.PostAsync(Url, content).Result;
        }
    }
}
