using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;

namespace IronBug.Context.Helpers
{
    public static class ModelBuilderHelper
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableNameAnnotation = (ConventionAnnotation)entity.FindAnnotation(RelationalAnnotationNames.TableName);
#pragma warning disable EF1001 // Internal EF Core API usage.
                var configurationSource = tableNameAnnotation.GetConfigurationSource();
#pragma warning restore EF1001 // Internal EF Core API usage.
                if (configurationSource != ConfigurationSource.Convention)
                {
                    continue;
                }
                entity.SetTableName(entity.DisplayName());
            }
        }

        public static void MakeAllStringsNonUnicodeConvention(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(string)))
            {
                property.SetIsUnicode(false);
            }
        }
    }
}