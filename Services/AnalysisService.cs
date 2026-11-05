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
        if (targetLanguage == "EN")
            targetLanguage = "English";
        if (targetLanguage == "JP")
            targetLanguage = "Japanese";
        if (targetLanguage == "KR")
            targetLanguage = "Korean";

        if (nativeLanguage == "EN")
            nativeLanguage = "English";
        if (nativeLanguage == "ES")
            nativeLanguage = "Spanish";

        var prompt = $"""
            You are a language coach analyzing a practice session for a
            {level} learner of {targetLanguage}. Their native language
            is {nativeLanguage} so make sure to give the explanation in {nativeLanguage}.

            Based on the results from all the questions, identify the 3 to 5 most
            impactful grammar and/or vocabulary topics for the student to focus on next.
            Prioritize patterns that repeat over one-off typos. Explanations in the native language, examples
            in the target language. Do not give one correction for each question, instead address
            the overall patterns, not each mistake.

            Do not try to add formatting, just use plain text. Do not use * for emphasis, and definitely avoid em dashes.

            USER RESPONSES:
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
