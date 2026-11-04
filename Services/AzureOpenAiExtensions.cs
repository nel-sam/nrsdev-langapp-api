using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;


namespace Services;

public static class AzureOpenAiExtensions
{
    public static void AddAzureOpenAi(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(sp =>
        {
            var endpoint = config["AzureOpenAi:Endpoint"] ?? throw new InvalidOperationException("AzureOpenAi:Endpoint missing");
            var apiKey = config["AzureOpenAi:ApiKey"] ?? throw new InvalidOperationException("AzureOpenAi:ApiKey missing");
            var deployment = config["AzureOpenAi:Deployment"] ?? throw new InvalidOperationException("AzureOpenAi:Deployment missing");

            return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
                .GetChatClient(deployment)
                .AsIChatClient();
        });
    }
}