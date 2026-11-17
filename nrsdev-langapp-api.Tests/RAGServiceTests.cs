using System.Text.Json;
using Data;
using Interfaces;
using Models;
using Moq;
using Services;
using Xunit;

namespace nrsdev_langapp_api.Tests;

/// <summary>
/// Tests for <see cref="RAGService"/>.
///
/// ASSUMPTIONS (adjust the helpers at the bottom if your types differ):
///  - IEmbeddingService.EmbedAsync(string) returns Task&lt;float[]&gt;
///  - ISessionEmbeddingRepository.SaveAsync(List&lt;SessionEmbedding&gt;) and
///    GetAllForUserAsync(string) returning a collection of SessionEmbedding
///  - QuestionResult(Question, CorrectAnswer, GivenAnswer)
///  - SessionEmbedding(UserName, Summary, Embedding, CreatedAt)
///  - SessionEmbeddingSimilarities(Current, Similar) where Similar is a collection
///  - EmbeddingService.CosineSimilarity is the real static implementation
/// </summary>
public class RAGServiceTests
{
    private const string UserName = "alice";

    private readonly Mock<IEmbeddingService> _embeddingService = new();
    private readonly Mock<ISessionEmbeddingRepository> _repository = new();

    private RAGService CreateSut() => new(_embeddingService.Object, _repository.Object);

    // ---------------------------------------------------------------------
    // SaveAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SaveAsync_EmbedsEachResultAndSavesOneEmbeddingPerResult()
    {
        var results = new List<QuestionResult>
        {
            MakeResult("Q1", "A1", "G1"),
            MakeResult("Q2", "A2", "G2")
        };
        var emb1 = new float[] { 1f, 0f };
        var emb2 = new float[] { 0f, 1f };

        _embeddingService.Setup(e => e.EmbedAsync("Q1 | correct: A1 | given: G1")).ReturnsAsync(emb1);
        _embeddingService.Setup(e => e.EmbedAsync("Q2 | correct: A2 | given: G2")).ReturnsAsync(emb2);

        List<SessionEmbedding>? saved = null;
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<IEnumerable<SessionEmbedding>>()))
            .Callback<IEnumerable<SessionEmbedding>>(list => saved = list.ToList())
            .Returns(Task.CompletedTask);

        await CreateSut().SaveAsync(UserName, results);

        Assert.NotNull(saved);
        Assert.Equal(2, saved!.Count);

        Assert.Equal(UserName, UserOf(saved[0]));
        Assert.Equal("Q1 | correct: A1 | given: G1", SummaryOf(saved[0]));
        Assert.Equal(JsonSerializer.Serialize(emb1), saved[0].Embedding);

        Assert.Equal(UserName, UserOf(saved[1]));
        Assert.Equal("Q2 | correct: A2 | given: G2", SummaryOf(saved[1]));
        Assert.Equal(JsonSerializer.Serialize(emb2), saved[1].Embedding);

        _repository.Verify(r => r.SaveAsync(It.IsAny<List<SessionEmbedding>>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_WithNoResults_SavesEmptyListWithoutEmbedding()
    {
        List<SessionEmbedding>? saved = null;
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<IEnumerable<SessionEmbedding>>()))
            .Callback<IEnumerable<SessionEmbedding>>(list => saved = list.ToList())
            .Returns(Task.CompletedTask);

        await CreateSut().SaveAsync(UserName, new List<QuestionResult>());

        Assert.NotNull(saved);
        Assert.Empty(saved!);
        _embeddingService.Verify(e => e.EmbedAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_StampsEmbeddingsWithCurrentTime()
    {
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 1f });

        List<SessionEmbedding>? saved = null;
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<IEnumerable<SessionEmbedding>>()))
            .Callback<IEnumerable<SessionEmbedding>>(list => saved = list.ToList())
            .Returns(Task.CompletedTask);

        var before = DateTime.Now;
        await CreateSut().SaveAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") });
        var after = DateTime.Now;

        var created = CreatedAtOf(saved!.Single());
        Assert.InRange(created, before, after);
    }

    // ---------------------------------------------------------------------
    // GetAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_WithNoHistory_ReturnsOneEntryPerQuestionWithNoSimilar()
    {
        var results = new List<QuestionResult>
        {
            MakeResult("Q1", "A1", "G1"),
            MakeResult("Q2", "A2", "G2")
        };
        var embedding = new float[] { 1f, 0f };

        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(embedding);
        SetupHistory();

        var output = (await CreateSut().GetAsync(UserName, results)).ToList();

        Assert.Equal(2, output.Count);
        Assert.All(output, o => Assert.Empty(SimilarOf(o)));
        Assert.Equal("Q1 | correct: A1 | given: G1", SummaryOf(CurrentOf(output[0])));
        Assert.Equal("Q2 | correct: A2 | given: G2", SummaryOf(CurrentOf(output[1])));
    }

    [Fact]
    public async Task GetAsync_NewSessionEmbedding_ContainsUserSummaryAndSerializedEmbedding()
    {
        var embedding = new float[] { 0.5f, 0.25f };
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(embedding);
        SetupHistory();

        var output = (await CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") })).Single();
        var current = CurrentOf(output);

        Assert.Equal(UserName, UserOf(current));
        Assert.Equal("Q | correct: A | given: G", SummaryOf(current));
        Assert.Equal(JsonSerializer.Serialize(embedding), current.Embedding);
    }

    [Fact]
    public async Task GetAsync_IncludesOnlyEmbeddingsAboveSimilarityThreshold()
    {
        var current = new float[] { 1f, 0f };
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(current);

        var similar = MakeStored("similar", new float[] { 1f, 0f });      // cosine = 1.0
        var dissimilar = MakeStored("dissimilar", new float[] { 0f, 1f }); // cosine = 0.0
        SetupHistory(similar, dissimilar);

        var output = (await CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") })).Single();

        var matches = SimilarOf(output).ToList();
        Assert.Single(matches);
        Assert.Equal("similar", SummaryOf(matches[0]));
    }

    [Fact]
    public async Task GetAsync_WhenSimilarityIsBelowThreshold_ExcludesEmbedding()
    {
        var current = new float[] { 1f, 0f };
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(current);

        // cosine similarity between (1,0) and (1,1) is ~0.707, which is below 0.9
        SetupHistory(MakeStored("below", new float[] { 1f, 1f }));

        var output = (await CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") })).Single();

        Assert.Empty(SimilarOf(output));
    }

    [Fact]
    public async Task GetAsync_LimitsSimilarResultsToTopK()
    {
        var current = new float[] { 1f, 0f };
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(current);

        var stored = Enumerable.Range(1, 5)
            .Select(i => MakeStored($"match-{i}", new float[] { 1f, 0f }))
            .ToArray();
        SetupHistory(stored);

        var output = (await CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") })).Single();

        var matches = SimilarOf(output).ToList();
        Assert.Equal(3, matches.Count);
        Assert.Equal(new[] { "match-1", "match-2", "match-3" }, matches.Select(SummaryOf));
    }

    [Fact]
    public async Task GetAsync_AppliesTopKLimitPerQuestion()
    {
        var current = new float[] { 1f, 0f };
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(current);

        SetupHistory(Enumerable.Range(1, 5)
            .Select(i => MakeStored($"match-{i}", new float[] { 1f, 0f }))
            .ToArray());

        var results = new List<QuestionResult>
        {
            MakeResult("Q1", "A1", "G1"),
            MakeResult("Q2", "A2", "G2")
        };

        var output = (await CreateSut().GetAsync(UserName, results)).ToList();

        Assert.Equal(2, output.Count);
        Assert.All(output, o => Assert.Equal(3, SimilarOf(o).Count()));
    }

    [Fact]
    public async Task GetAsync_EmbedsEachQuestionSummaryAndQueriesRepositoryOnce()
    {
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 1f, 0f });
        SetupHistory();

        var results = new List<QuestionResult>
        {
            MakeResult("Q1", "A1", "G1"),
            MakeResult("Q2", "A2", "G2"),
            MakeResult("Q3", "A3", "G3")
        };

        await CreateSut().GetAsync(UserName, results);

        _embeddingService.Verify(e => e.EmbedAsync("Q1 | correct: A1 | given: G1"), Times.Once);
        _embeddingService.Verify(e => e.EmbedAsync("Q2 | correct: A2 | given: G2"), Times.Once);
        _embeddingService.Verify(e => e.EmbedAsync("Q3 | correct: A3 | given: G3"), Times.Once);
        _repository.Verify(r => r.GetAllForUserAsync(UserName), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithNoQuestionResults_ReturnsEmpty()
    {
        SetupHistory(MakeStored("existing", new float[] { 1f, 0f }));

        var output = await CreateSut().GetAsync(UserName, new List<QuestionResult>());

        Assert.Empty(output);
        _embeddingService.Verify(e => e.EmbedAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenEmbeddingServiceReturnsNull_ThrowsInvalidOperationException()
    {
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync((float[])null!);
        SetupHistory();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") }));

        Assert.Equal("Summary embedding not found", ex.Message);
    }

    [Fact]
    public async Task GetAsync_WhenStoredEmbeddingDeserializesToNull_ThrowsInvalidOperationException()
    {
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 1f, 0f });

        // The JSON literal "null" deserializes to a null float[]
        SetupHistory(MakeStoredRaw("bad", "null"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") }));

        Assert.Equal("Error deserializing embeddings", ex.Message);
    }

    [Fact]
    public async Task GetAsync_WhenStoredEmbeddingIsMalformedJson_ThrowsJsonException()
    {
        _embeddingService.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 1f, 0f });
        SetupHistory(MakeStoredRaw("bad", "not-json"));

        await Assert.ThrowsAsync<JsonException>(() =>
            CreateSut().GetAsync(UserName, new List<QuestionResult> { MakeResult("Q", "A", "G") }));
    }

    // ---------------------------------------------------------------------
    // Helpers - tweak these if your model shapes differ from the assumptions
    // ---------------------------------------------------------------------

    private void SetupHistory(params SessionEmbedding[] stored)
    {
        _repository
            .Setup(r => r.GetAllForUserAsync(UserName))
            .ReturnsAsync(stored.ToList());
    }

    private static QuestionResult MakeResult(string question, string correct, string given)
        => new(question, correct, given);

    private static SessionEmbedding MakeStored(string summary, float[] embedding)
        => MakeStoredRaw(summary, JsonSerializer.Serialize(embedding));

    private static SessionEmbedding MakeStoredRaw(string summary, string embeddingJson)
        => new(UserName, summary, embeddingJson, DateTime.Now);

    private static string UserOf(SessionEmbedding se) => se.UserName;
    private static string SummaryOf(SessionEmbedding se) => se.Summary;
    private static DateTime CreatedAtOf(SessionEmbedding se) => se.CreatedAt;

    private static SessionEmbedding CurrentOf(SessionEmbeddingSimilarities s) => s.SessionEmbedding;
    private static IEnumerable<SessionEmbedding> SimilarOf(SessionEmbeddingSimilarities s) => s.SimilarSessionEmbeddings;
}