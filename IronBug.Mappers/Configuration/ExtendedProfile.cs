using AutoMapper;

namespace IronBug.Mappers.Configuration
{
    public class ExtendedProfile : Profile
    {
        public MappingSource<TSource> Map<TSource>()
        {
            return new MappingSource<TSource>(this);
        }
    }
}