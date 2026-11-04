using Models;

namespace Interfaces;

public interface IAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(
        string targetLanguage,
        string level,
        string nativeLanguage,
        List<QuestionResult> userResponses,
        CancellationToken ct = default);
}