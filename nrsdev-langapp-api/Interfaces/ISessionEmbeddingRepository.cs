using Models;

namespace Interfaces;

public interface ISessionEmbeddingRepository
{
    Task SaveAsync(IEnumerable<SessionEmbedding> sessionEmbeddings);

    Task<IEnumerable<SessionEmbedding>> GetAllForUserAsync(string userName);
}