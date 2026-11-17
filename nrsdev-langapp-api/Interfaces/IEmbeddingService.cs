namespace Interfaces;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text);
    
}