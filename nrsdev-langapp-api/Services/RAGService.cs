using System.Text.Json;
using Data;
using Interfaces;
using Models;

namespace Services;

public class RAGService(IEmbeddingService embeddingService, ISessionEmbeddingRepository sessionEmbeddingRepository) : IRAGService
{
    const int TOP_K_RETRIEVAL_COUNT = 3;

    public async Task SaveAsync(string userName, List<QuestionResult> questionResults)
    {
        var sessionEmbeddings = new List<SessionEmbedding>();

        foreach (var result in questionResults)
        {
            var summary = GetSummary(result);
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

    private static string GetSummary(QuestionResult result)
    {
        return $"{result.Question} | correct: {result.CorrectAnswer} | given: {result.GivenAnswer}";
    }

    public async Task<IEnumerable<SessionEmbeddingSimilarities>> GetAsync(string userName, List<QuestionResult> questionResults)
    {
        // TODO: for now we are just pulling from the Coach history (summaries) but eventually we
        // want to get exercise result history too
        var qrSummaries = questionResults.Select(r => GetSummary(r)).ToList();
        var sessionEmbeddings = await sessionEmbeddingRepository.GetAllForUserAsync(userName);
        var nearbySessionEmbeddings = new List<SessionEmbeddingSimilarities>();

        foreach (var currentSummary in qrSummaries)
        {
            var currentEmbedding = await embeddingService.EmbedAsync(currentSummary);

            if (currentEmbedding == null)
            {
                throw new InvalidOperationException("Summary embedding not found");
            }

            var similarToCurrent = new List<SessionEmbedding>();

            foreach (var se in sessionEmbeddings)
            {
                if (similarToCurrent.Count >= TOP_K_RETRIEVAL_COUNT)
                {
                    break;
                }

                // TODO: move this into a Select() on the GetAllForUserAsync call for improved performance
                var seEmbeddingArr = JsonSerializer.Deserialize<float[]>(se.Embedding);

                if (seEmbeddingArr == null)
                {
                    throw new InvalidOperationException("Error deserializing embeddings");
                }

                if (EmbeddingService.CosineSimilarity(currentEmbedding, seEmbeddingArr) > 0.9)
                {
                    similarToCurrent.Add(se);
                }
            }

            var newSE = new SessionEmbedding(userName, currentSummary, JsonSerializer.Serialize(currentEmbedding), DateTime.Now);
            nearbySessionEmbeddings.Add(new SessionEmbeddingSimilarities(newSE, similarToCurrent));
        }

        return nearbySessionEmbeddings;
    }
}