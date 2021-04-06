using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IronBug.Context.Helpers
{
    public static class ModelBuilderHelper
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.ClrType.Name.Contains("Dictionary"))
                    continue;

                var tableName = entity.DisplayName();
                entity.SetTableName(tableName);
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