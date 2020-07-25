using AutoMapper;
using IronBug.Mappers.Resolution;
using System;

namespace IronBug.Mappers
{
    public interface IAutoMapperService
    {
        IMapper Mapper { get; }
        IMapping Mapping<T>(T model);
    }

    public class AutoMapperService : IAutoMapperService
    {
        private AutoMapperService(IMapper mapper)
        {
            Mapper = mapper;
        }

        public static AutoMapperService Instance { get; private set; }
        public IMapper Mapper { get; }
        public IMapping Mapping<T>(T model)
        {
            return new Mapping<T>(Mapper, model);
        }

        public static void Initialize(Action<IMapperConfigurationExpression> configuration)
        {
            var config = new MapperConfiguration(configuration);

            Instance = new AutoMapperService(config.CreateMapper());
        }
    }
}