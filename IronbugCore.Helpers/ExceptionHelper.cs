namespace IronbugCore.Helpers;

public static class ExceptionHelper
{
    public static IEnumerable<string> Messages(this Exception ex)
    {
        if (ex == null)
        {
            yield break;
        }

        yield return ex.Message;

        var innerExceptions = Enumerable.Empty<Exception>();

        if (ex is AggregateException exception && exception.InnerExceptions.Any())
        {
            innerExceptions = exception.InnerExceptions;
        }
        else if (ex.InnerException != null)
        {
            innerExceptions = [ex.InnerException];
        }

        foreach (var innerEx in innerExceptions)
        {
            foreach (var msg in innerEx.Messages())
            {
                yield return msg;
            }
        }
    }
}