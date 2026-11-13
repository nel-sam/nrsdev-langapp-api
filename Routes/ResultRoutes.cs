using Interfaces;
using Models;

namespace Routes;

internal sealed record ResultRequest(string TargetLanguage, string NativeLanguage, List<QuestionResult> Results);

public static class ResultRoutes
{
    public static void MapResultRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/results", async (ResultRequest request, IAnalysisService analysisService, IRAGService ragService) => {
            var result = await analysisService.AnalyzeAsync(request.TargetLanguage, "A1", request.NativeLanguage, request.Results);
            
            var userName = "testuser@test.com"; // TODO: Get from auth provider
            await ragService.SaveAsync(userName, request.Results);
            
            return Results.Ok(result);
        })
        .RequireRateLimiting("limit");
    }
}