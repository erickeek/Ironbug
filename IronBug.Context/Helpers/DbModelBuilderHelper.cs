using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;

namespace IronBug.Context.Helpers
{
    public static class DbModelBuilderHelper
    {
        public static void RegisterAllTypeConfigurations(this DbModelBuilder modelBuilder, Assembly assembly)
        {
            var typesToRegister = assembly.GetTypes()
                .Where(type => !string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var configurationInstance in typesToRegister.Select(Activator.CreateInstance))
            {
                modelBuilder.Configurations.Add((dynamic)configurationInstance);
            }
        }
    }
}