using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace IronBugCore.Web.Helpers;

public static class UrlHelper
{
    public static string AbsoluteContent(this IUrlHelper url, [LocalizationRequired(false), PathReference] string contentPath)
    {
        var request = url.ActionContext.HttpContext.Request;
        return new Uri(new Uri($"{request.Scheme}://{request.Host.Value}"), url.Content(contentPath)).ToString();
    }
}