using Data;
using Models;

namespace Interfaces;

public interface IAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(
        string targetLanguage,
        string level,
        string nativeLanguage,
        IEnumerable<QuestionResult> userResponses,
        IEnumerable<SessionEmbeddingSimilarities> similarHistory,
        CancellationToken ct = default);
}