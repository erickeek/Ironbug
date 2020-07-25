using AutoMapper;

namespace IronBug.Mappers.Configuration
{
    public class MappingSource<TSource>
    {
        private readonly Profile _profile;

        public MappingSource(Profile profile)
        {
            _profile = profile;
        }

        public MappingDestination<TSource, TDestination> To<TDestination>()
        {
            return new MappingDestination<TSource, TDestination>(_profile);
        }
        public MappingDestination<TSource, TSource> ToSelf()
        {
            return To<TSource>();
        }
    }
}