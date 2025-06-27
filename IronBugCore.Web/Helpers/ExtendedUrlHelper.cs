using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace IronBugCore.Web.Helpers;

public static class ExtendedUrlHelper
{
    public static string ContentVersioned(this IUrlHelper helper, [LocalizationRequired(false), PathReference] string contentPath)
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "1.0";
        var versionedContentPath = $"{contentPath}?v={version}";
        return helper.Content(versionedContentPath);
    }
}