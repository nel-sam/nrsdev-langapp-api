using Interfaces;
using Models;

namespace Routes;

internal sealed record ResultRequest(string TargetLanguage, string NativeLanguage, List<QuestionResult> Results);

public static class ResultRoutes
{
    public static void MapResultRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/results", async (ResultRequest request, IAnalysisService analysisService, IRAGService ragService) => {
            
            // The next few lines is where the RAG magic happens
            // Save results for use in future coaching sessions
            var userName = "testuser@test.com"; // TODO: Get from auth provider
            await ragService.SaveAsync(userName, request.Results);
           
           // Before getting the analysis, fetch past results and add them to the context of the
           // call to the LLM (via the analysisService)
           var similarHistory = await ragService.GetAsync(userName, request.Results);
           
            var result = await analysisService.AnalyzeAsync(request.TargetLanguage, "A1", request.NativeLanguage, request.Results, similarHistory);
           
            return Results.Ok(result);
        })
        .RequireRateLimiting("limit");
    }
}