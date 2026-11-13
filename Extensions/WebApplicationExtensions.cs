using Dapper;
using Data;

namespace Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeSchema(this WebApplication app)
    {
        using var connection = app.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();

        await connection.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS session_embeddings (
            Id INTEGER PRIMARY KEY,
            UserName NVARCHAR(200) NOT NULL,
            Summary TEXT NOT NULL,
            CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
            Embedding TEXT NOT NULL  -- the 1536 floats as JSON
            ); 
            """);
    }
}