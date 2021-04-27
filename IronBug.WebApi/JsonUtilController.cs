using IronBug.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace IronBug.WebApi
{
    public abstract class JsonUtilController : BaseApiController
    {
        protected abstract Dictionary<string, Type> Types { get; }

        public virtual IHttpActionResult ListEnums(string typeName)
        {
            if (!Types.ContainsKey(typeName))
                return Json(new { });

            var type = Types[typeName];
            var values = Enum.GetValues(type);
            var objs = new List<object>();

            foreach (Enum value in values)
            {
                objs.Add(new
                {
                    Value = (int)(object)value,
                    Name = Enum.GetName(type, value),
                    Display = GetDisplayName(value)
                });
            }

            return Ok(objs);
        }

        protected virtual string GetDisplayName(Enum value)
        {
            return value.DisplayName();
        }
    }
}