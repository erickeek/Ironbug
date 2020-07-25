using AutoMapper;
using System;
using System.Linq.Expressions;

namespace IronBug.Mappers.Configuration
{
    public class MappingMemberConfiguration<TSource, TDestination, TDestinationMember>
    {
        private readonly MappingDestination<TSource, TDestination> _mapping;
        private readonly Expression<Func<TDestination, TDestinationMember>> _destinationMember;

        public MappingMemberConfiguration(
            MappingDestination<TSource, TDestination> mapping,
            Expression<Func<TDestination, TDestinationMember>> destinationMember)
        {
            _mapping = mapping;
            _destinationMember = destinationMember;
        }

        public MappingDestination<TSource, TDestination> Options(Action<IMemberConfigurationExpression<TSource, TDestination, TDestinationMember>> options)
        {
            _mapping.Mapping.ForMember(_destinationMember, options);
            return _mapping;
        }

        public MappingDestination<TSource, TDestination> Ignore()
        {
            return Options(opt => opt.Ignore());
        }

        public MappingDestination<TSource, TDestination> Skip()
        {
            return Options(opt =>
            {
                opt.Ignore();
                opt.UseDestinationValue();
            });
        }

        public MappingDestination<TSource, TDestination> As<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            return Options(opt => opt.MapFrom(sourceMember));
        }
    }
}