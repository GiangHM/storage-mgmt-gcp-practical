using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace storagedal.Infra.efcore
{
    public static class EfCoreExtensions
    {
        public static IServiceCollection AddPostgresDatabaseServer(this IServiceCollection services, Action<PostgresDbOptions> sqlOptions)
        {
            services.Configure(sqlOptions);
            services.AddDbContext<StorageDbContext>((services, options) =>
            {
                var dbOptions = services.GetRequiredService<IOptions<PostgresDbOptions>>();
                options.UseNpgsql(dbOptions.Value.ConnectionString);

                if (dbOptions.Value.EnableDetailedErrors)
                {
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                    options.EnableDetailedErrors();
                }
            }, ServiceLifetime.Transient, ServiceLifetime.Transient);

            return services;
        }
    }
}
