using System.ClientModel.Primitives;
using OpenAI.Embeddings;
using Azure.Identity;
using Azure.AI.OpenAI;
using Interfaces;

namespace Services;

public class EmbeddingService: IEmbeddingService
{
    private readonly EmbeddingClient _client;
    
    public EmbeddingService(IConfiguration config)
    {
        var endpoint = config["Embedding:Endpoint"] ?? throw new InvalidOperationException("Embedding:Endpoint missing");
        var deployment = config["Embedding:Model"] ?? throw new InvalidOperationException("Embedding:Model missing");

        _client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
            .GetEmbeddingClient(deployment);
    }

    public async Task<float[]> EmbedAsync(string text)
    {
        var result = await _client.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }

    static float CosineSimilarity(float[] a, float[] b)
    {
        float dotProduct = 0, magA = 0, magB = 0;

        for (int i =0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magA += MathF.Pow(a[i], 2);
            magB += MathF.Pow(b[i], 2);
        }

        return dotProduct / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}