using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OpenBullet2.Core
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var dbContextBuilder = new DbContextOptionsBuilder();

            var sensitiveLogging = false;

            try
            {
                sensitiveLogging = bool.Parse(configuration.GetSection("Logging")
                    .GetSection("EntityFrameworkCore")["EnableSensitiveDataLogging"]);
            }
            catch
            {

            }

            dbContextBuilder.EnableSensitiveDataLogging(sensitiveLogging);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            dbContextBuilder.UseSqlite(connectionString);

            return new ApplicationDbContext(dbContextBuilder.Options);
        }
    }
}
