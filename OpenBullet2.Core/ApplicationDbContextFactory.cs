using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenBullet2.Core
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder();

            var sensitiveLogging = false;
            var connectionString = "Data Source=UserData/OpenBullet.db;";

            dbContextBuilder.EnableSensitiveDataLogging(sensitiveLogging);
            dbContextBuilder.UseSqlite(connectionString);

            return new ApplicationDbContext(dbContextBuilder.Options);
        }
    }
}
