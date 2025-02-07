using CineVault.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCineVaultDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CineVaultDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("CineVaultDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured");
            }

            options.UseInMemoryDatabase(connectionString);
        });

        return services;
    }
}
