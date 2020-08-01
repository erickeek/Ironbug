using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IronBug.Context
{
    public interface IDbContext
    {
        DatabaseFacade Database { get; }

        int SaveChanges();
    }
}