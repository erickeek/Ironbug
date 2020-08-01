using IronBug.Context.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace IronBug.Context
{
    public abstract class DbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
    {
        public DbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected abstract Assembly ContextAssembly();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RemovePluralizingTableNameConvention();
            modelBuilder.MakeAllStringsNonUnicodeConvention();

            modelBuilder.ApplyConfigurationsFromAssembly(ContextAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateSoftDelete();
            return base.SaveChanges();
        }

        private void UpdateSoftDelete()
        {
            ChangeTracker.DetectChanges();

            var markedAsDeleted = ChangeTracker.Entries().Where(q => q.State == EntityState.Deleted);

            foreach (var item in markedAsDeleted)
            {
                if (!(item.Entity is IExclusaoLogica entity)) continue;

                item.State = EntityState.Unchanged;
                entity.Excluido = true;
            }
        }
    }
}