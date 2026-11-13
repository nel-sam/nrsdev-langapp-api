using System.Text.Json;
using Interfaces;
using Models;

namespace Services;

public class RAGService(IEmbeddingService embeddingService, ISessionEmbeddingRepository sessionEmbeddingRepository) : IRAGService
{
    public async Task SaveAsync(string userName, List<QuestionResult> questionResults)
    {
        var sessionEmbeddings = new List<SessionEmbedding>();

        foreach (var result in questionResults)
        {
            var summary = $"{result.Question} | correct: {result.CorrectAnswer} | given: {result.GivenAnswer}";
            var embeddings = await embeddingService.EmbedAsync(summary);
            var embeddingJson = JsonSerializer.Serialize(embeddings);

            sessionEmbeddings.Add(new SessionEmbedding(
                userName,
                summary,
                embeddingJson,
                DateTime.Now
            ));
        }

        await sessionEmbeddingRepository.SaveAsync(sessionEmbeddings);
    }
}