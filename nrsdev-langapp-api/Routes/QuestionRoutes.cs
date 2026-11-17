using System.Text.Json;
using Models;

namespace Routes;

public static class QuestionRoutes
{
    public static void MapQuestionRoutes(this IEndpointRouteBuilder app)
    {
        // TOOD: add support for other languages
        app.MapGet("/questions/{language}", async (string language) =>
        {
            var json = await File.ReadAllTextAsync("TempData/questions-jp.json");
            var questions = JsonSerializer.Deserialize<List<Question>>(json) ?? [];
            
            // Pick 8 random questions
            questions = questions.OrderBy(_ => Guid.NewGuid()).Take(8).ToList();

            return questions;
        })
        .WithName("GetQuestions");
    }
}