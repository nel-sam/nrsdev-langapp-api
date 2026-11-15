using Data;
using Models;

namespace Interfaces;

public interface IRAGService
{
    Task SaveAsync(string userName, List<QuestionResult> questionResults);

    Task<IEnumerable<SessionEmbeddingSimilarities>> GetAsync(string userName, List<QuestionResult> questionResults);
}