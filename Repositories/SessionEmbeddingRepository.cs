using System.Text.Json;
using Dapper;
using Data;
using Interfaces;
using Models;

public class SessionEmbeddingRepository(IDbConnectionFactory factory) : ISessionEmbeddingRepository
{
    public async Task SaveAsync(IEnumerable<SessionEmbedding> sessionEmbeddings)
    {
        using var connection = factory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            """
            INSERT INTO session_embeddings (UserName, Summary, CreatedAt, Embedding)
            VALUES (@UserName, @Summary, @CreatedAt, @Embedding)
            """,
            sessionEmbeddings,
            transaction
        );

        transaction.Commit();
    }

    public async Task<IEnumerable<SessionEmbedding>> GetAllForUserAsync(string userName)
    {
        using var connection = factory.CreateConnection();

        return await connection.QueryAsync<SessionEmbedding>(
           """
            SELECT UserName, Summary, Embedding, CreatedAt
            FROM session_embeddings
            WHERE UserName = @UserName
            """,
           new { UserName = userName }
       );
    }
}