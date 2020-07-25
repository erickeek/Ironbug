using IronBug.Mappers.Resolution;

namespace IronBug.Mappers
{
    public static class AutoMapperHelper
    {
        public static IMapping Map<TSource>(this TSource source)
        {
            return new Mapping<TSource>(AutoMapperService.Instance.Mapper, source);
        }
    }
}