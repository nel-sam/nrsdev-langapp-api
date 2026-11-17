using System.Threading.RateLimiting;
using Interfaces;
using Routes;
using Scalar.AspNetCore;
using Services;
using Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add core via extension method
builder.Services.AddCoreServices(builder.Configuration);

builder.Services.AddAzureOpenAi(builder.Configuration); 
builder.Services.AddSingleton<IAnalysisService, AnalysisService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("limit", context => 
        RateLimitPartition.GetTokenBucketLimiter(
             // TODO: Once we support user accounts, use user ID instead of IP address
            context.Connection.RemoteIpAddress?.ToString() ?? "unkown",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,
                TokensPerPeriod = 5,
                ReplenishmentPeriod = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
            }
        )
    );
});

var app = builder.Build();

await app.InitializeSchema();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Add a UI for Scalar API on localhost:5030/scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map API routes
var api = app.MapGroup("/api");
api.MapQuestionRoutes();
api.MapResultRoutes();

app.Run();
