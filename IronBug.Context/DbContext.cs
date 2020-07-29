using IronBug.Context.Conventions;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IronBug.Context
{
    public abstract class DbContext : System.Data.Entity.DbContext
    {
        protected DbContext() { }

        protected DbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected abstract Assembly ContextAssembly();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add<MakeAllStringsNonUnicode>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Configurations.AddFromAssembly(ContextAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            try
            {
                UpdateSoftDelete();
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException($"Entity Validation Failed - errors follow:\n{sb}", ex);
            }
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