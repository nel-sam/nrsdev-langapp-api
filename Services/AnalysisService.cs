using Interfaces;
using Microsoft.Extensions.AI;
using Models;
using System.Text.Json;

namespace Services;

public class AnalysisService(IChatClient chat) : IAnalysisService
{
    public async Task<AnalysisResult> AnalyzeAsync(
        string targetLanguage,
        string level,
        string nativeLanguage,
        List<QuestionResult> userResponses,
        CancellationToken ct = default)
    {
        var prompt = $"""
            You are a language coach analyzing a practice session for a
            {level} learner of {targetLanguage}. Their native language
            is {nativeLanguage}.

            Identify the 3 to 5 most impactful grammar and/or vocabulary
            topics to focus on next. Prioritize patterns that repeat over
            one-off typos. Explanations in the native language, examples
            in the target language.

            USER RESPONSE:
            {string.Join("\n---\n", userResponses)}
            """;

        var response = await chat.GetResponseAsync(
            [new ChatMessage(ChatRole.System, prompt)],
            new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema(typeof(AnalysisResult))
            },
            ct);

        return JsonSerializer.Deserialize<AnalysisResult>(
            response.Text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Empty analysis from LLM");

    }
}
