namespace Models;

public class SessionEmbedding
{
    public string UserName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Embedding { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Constructor for Dapper mapping
    public SessionEmbedding (string UserName, string Summary, string Embedding, string CreatedAt)
    {
        this.UserName = UserName.ToString();
        this.Summary = Summary.ToString();
        this.Embedding = Embedding.ToString();
        this.CreatedAt = DateTime.Parse(CreatedAt);
    }

        public SessionEmbedding (string UserName, string Summary, string Embedding, DateTime CreatedAt)
    {
        this.UserName = UserName.ToString();
        this.Summary = Summary.ToString();
        this.Embedding = Embedding.ToString();
        this.CreatedAt = CreatedAt;
    }
}   
