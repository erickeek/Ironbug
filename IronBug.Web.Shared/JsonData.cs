namespace IronBug.Web
{
    public class JsonData
    {
        public JsonData(object data = null)
        {
            Data = data;
        }

        public JsonData(bool status, string message)
        {
            Status = status;
            Message = message;
        }

        public bool Authenticated { get; set; } = true;
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
