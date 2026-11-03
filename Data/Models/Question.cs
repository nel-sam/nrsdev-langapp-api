using System.Text.Json.Serialization;

namespace Models;

public class Question
{
    [JsonPropertyName("question")]
    public required string Text { get; set; }

    [JsonPropertyName("tested-abilities")]
    public List<string> TestedAbilities { get; set; } = [];

    [JsonPropertyName("correct-answer-index")]
    public int CorrectAnswerIndex { get; set; }

    [JsonPropertyName("choices")]
    public List<string> Choices { get; set; } = [];
}