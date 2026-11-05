
using Interfaces;
using Models;

namespace Routes;

internal sealed record ResultRequest(string TargetLanguage, string NativeLanguage, List<QuestionResult> Results);

public static class ResultRoutes
{
    public static void MapResultRoutes(this IEndpointRouteBuilder app)
    {
        // TODO: Add rate limiting
        app.MapPost("/results", async (ResultRequest request, IAnalysisService analysisService) => {
            var result = await analysisService.AnalyzeAsync(request.TargetLanguage, "A1", request.NativeLanguage, request.Results);
            return Results.Ok(result);
        });
    }
}