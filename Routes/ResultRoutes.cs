
using Interfaces;
using Models;

namespace Routes;

internal sealed record ResultRequest(string targetLanguage, string nativeLanguage, List<QuestionResult> Results);

public static class ResultRoutes
{
    public static void MapResultRoutes(this IEndpointRouteBuilder app)
    {
        // TODO: Add rate limiting
        app.MapPost("/results", async (ResultRequest request, IAnalysisService analysisService) => {
            var result = await analysisService.AnalyzeAsync(request.targetLanguage, "A1", request.nativeLanguage, request.Results);
            return Results.Ok(result);
        });
    }
}