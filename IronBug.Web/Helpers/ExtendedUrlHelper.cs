using System.Web;
using System.Web.Mvc;

namespace IronBug.Web.Helpers
{
    public static class ExtendedUrlHelper
    {
        public static string ContentVersioned(this UrlHelper helper, string contentPath)
        {
            var version = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly.GetName().Version;
            var versionedContentPath = $"{contentPath}?v={version}";
            return helper.Content(versionedContentPath);
        }
    }
}