using AutoMapper;
using System;
using System.Linq.Expressions;

namespace IronBug.Mappers.Configuration
{
    public class MappingDestination<TSource, TDestination>
    {
        private readonly Profile _profile;
        internal readonly IMappingExpression<TSource, TDestination> Mapping;

        public MappingDestination(Profile profile)
        {
            _profile = profile;
            Mapping = profile.CreateMap<TSource, TDestination>();
            MaxDepth(5);
        }

        public MappingDestination<TSource, TDestination> MaxDepth(int depth)
        {
            Mapping.MaxDepth(depth);
            return this;
        }

        public MappingDestination<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            Mapping.AfterMap(afterFunction);
            return this;
        }

        public MappingDestination<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            Mapping.BeforeMap(beforeFunction);
            return this;
        }

        public MappingDestination<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>(Action<MappingDestination<TDerivedSource, TDerivedDestination>> mapping = null)
            where TDerivedSource : TSource
            where TDerivedDestination : TDestination
        {
            var m = new MappingSource<TDerivedSource>(_profile).To<TDerivedDestination>();

            m.Mapping.IncludeBase<TSource, TDestination>();

            mapping?.Invoke(m);
            return this;
        }

        public MappingDestination<TSource, TDestination> IncludeSource<TDerivedSource>(Action<MappingDestination<TDerivedSource, TDestination>> mapping = null)
            where TDerivedSource : TSource
        {
            return Include(mapping);
        }

        public MappingDestination<TSource, TDestination> IncludeDestination<TDerivedDestination>(Action<MappingDestination<TSource, TDerivedDestination>> mapping = null)
            where TDerivedDestination : TDestination
        {
            return Include(mapping);
        }

        public MappingMemberConfiguration<TSource, TDestination, TMember> Member<TMember>(Expression<Func<TDestination, TMember>> destinationMember)
        {
            return new MappingMemberConfiguration<TSource, TDestination, TMember>(
                this,
                destinationMember
            );
        }
    }
}