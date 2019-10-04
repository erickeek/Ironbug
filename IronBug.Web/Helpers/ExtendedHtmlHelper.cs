using Newtonsoft.Json;
using System.Web.Mvc;

namespace IronBug.Web.Helpers
{
    public static class ExtendedHtmlHelper
    {
        public static string ToJson(this HtmlHelper html, object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}