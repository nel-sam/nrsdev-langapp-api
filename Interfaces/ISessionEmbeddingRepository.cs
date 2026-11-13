using Models;

namespace Interfaces;

public interface ISessionEmbeddingRepository
{
    Task SaveAsync(List<SessionEmbedding> sessionEmbeddings);

    Task<IEnumerable<float[]>> Get();
}