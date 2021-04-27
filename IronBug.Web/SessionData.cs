using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace IronBug.Web
{
    public class SessionData<T> where T : class
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public SessionData(string key)
        {
            Key = key;
        }

        public string Key { get; }
        public T Value
        {
            get
            {
                var value = HttpContext.Current.Request.GetOwinContext().Authentication.User.GetClaimValue(Key);
                return value == null ? null : JsonConvert.DeserializeObject<T>(value, _settings);
            }
            set
            {
                var user = HttpContext.Current.Request.GetOwinContext().Authentication.User;
                if (value == null)
                {
                    var existingClaim = user.FindFirst(Key);
                    if (existingClaim != null)
                        ((ClaimsIdentity)user.Identity).RemoveClaim(existingClaim);

                    return;
                }

                user.AddUpdateClaim(Key, JsonConvert.SerializeObject(value, _settings));
            }
        }
    }

    internal static class Extensions
    {
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            if (!(currentPrincipal.Identity is ClaimsIdentity identity))
                return;

            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);

            identity.AddClaim(new Claim(key, value));
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                IsPersistent = true
            });
        }

        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            if (!(currentPrincipal.Identity is ClaimsIdentity identity))
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim?.Value ?? "";
        }
    }
}
