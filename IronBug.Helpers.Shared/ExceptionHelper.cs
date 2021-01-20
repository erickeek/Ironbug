using System;
using System.Collections.Generic;
using System.Linq;

namespace IronBug.Helpers.Shared
{
    public static class ExceptionHelper
    {
        public static IEnumerable<string> Messages(this Exception ex)
        {
            if (ex == null)
            {
                yield break;
            }

            yield return ex.Message;

            IEnumerable<Exception> innerExceptions = Enumerable.Empty<Exception>();

            if (ex is AggregateException && (ex as AggregateException).InnerExceptions.Any())
            {
                innerExceptions = (ex as AggregateException).InnerExceptions;
            }
            else if (ex.InnerException != null)
            {
                innerExceptions = new Exception[] {ex.InnerException};
            }

            foreach (var innerEx in innerExceptions)
            {
                foreach (string msg in innerEx.Messages())
                {
                    yield return msg;
                }
            }
        }
    }
}