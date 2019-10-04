using System;
using System.Web.Mvc;

namespace IronBug.Web.Helpers
{
    public static class ControllerHelper
    {
        public static bool IsDefinedInActionOrController<T>(this ActionDescriptor actionDescriptor, bool inherit = true) where T : Attribute
        {
            var type = typeof(T);

            return actionDescriptor.IsDefined(type, inherit) ||
                   actionDescriptor.ControllerDescriptor.IsDefined(type, inherit);
        }
    }
}