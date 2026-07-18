using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace storagedal.Infra.efcore
{
    /// <summary>
    /// Design-time factory used by EF Core CLI tools (dotnet ef migrations add, etc.)
    /// to create a <see cref="StorageDbContext"/> without a running host.
    /// </summary>
    internal sealed class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
    {
        public StorageDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<StorageDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=storagemanagement;Username=postgres;Password=***")
                .Options;

            return new StorageDbContext(options);
        }
    }
}
