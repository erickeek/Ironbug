using System.Data.Entity;

namespace IronBug.Context
{
    public interface IDbContext
    {
        Database Database { get; }

        int SaveChanges();
    }
}