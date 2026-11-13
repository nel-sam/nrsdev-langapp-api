using Models;

namespace Interfaces;

public interface IRAGService
{
    Task SaveAsync(string userName, List<QuestionResult> questionResults);
}