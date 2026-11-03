using Data;

namespace Services;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration config)
    {
        // Sqlite setup
        var connectionString = config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Default ConnectionString is not configured.");
        }

        services.AddSingleton<IDbConnectionFactory>(new SqlLiteConnectionFactory(connectionString));
        return services;
    }

}
