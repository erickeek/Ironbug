using IronBug.Mappers;
using IronBug.WebApi.Mapping;

namespace IronBug.WebApi
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