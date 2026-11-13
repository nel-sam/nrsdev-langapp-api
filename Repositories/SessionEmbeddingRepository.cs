using System.Text.Json;
using Dapper;
using Data;
using Interfaces;
using Models;

public class SessionEmbeddingRepository(IDbConnectionFactory factory) : ISessionEmbeddingRepository
{
    public async Task SaveAsync(List<SessionEmbedding> sessionEmbeddings)
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

    public Task<IEnumerable<float[]>> Get()
    {
        throw new NotImplementedException();
    }
}