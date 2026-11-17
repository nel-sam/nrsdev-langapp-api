using Data;
using Interfaces;
using Services;

namespace Extensions;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        // Sqlite setup
        var connectionString = config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Default ConnectionString is not configured.");
        }

        services.AddSingleton<IDbConnectionFactory>(new SqlLiteConnectionFactory(connectionString));
        services.AddSingleton<ISessionEmbeddingRepository, SessionEmbeddingRepository>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IRAGService, RAGService>();
        return services;
    }

}
