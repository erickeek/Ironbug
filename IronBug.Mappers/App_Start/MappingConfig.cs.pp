using IronBug.Mappers;
using $rootnamespace$.Mapping;

namespace $rootnamespace$
{
    internal static class MappingConfig
    {
        internal static void RegisterMappings()
        {
            AutoMapperService.Initialize(c =>
            {
                c.AddProfile<DomainToViewModelMappingProfile>();
                c.AddProfile<ViewModelToDomainMappingProfile>();
            });
        }
    }
}