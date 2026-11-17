using Models;

namespace Data;

public sealed record SessionEmbeddingSimilarities(SessionEmbedding SessionEmbedding, List<SessionEmbedding> SimilarSessionEmbeddings);