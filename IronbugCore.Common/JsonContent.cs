using System.Text;
using Newtonsoft.Json;

namespace IronbugCore.Common;

/// <summary>
/// <see cref="StringContent"/> que serializa um objeto para JSON (Newtonsoft) com
/// <c>Content-Type: application/json; charset=utf-8</c> — para usar com <see cref="HttpClient"/>.
/// </summary>
public sealed class JsonContent : StringContent
{
    public JsonContent(string json) : base(json, Encoding.UTF8, "application/json")
    {
    }

    public JsonContent(object content) : this(JsonConvert.SerializeObject(content))
    {
    }

    public JsonContent(object content, JsonSerializerSettings settings) : this(JsonConvert.SerializeObject(content, settings))
    {
    }
}
