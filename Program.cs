using System.Text.Json;
using Interfaces;
using Models;
using Routes;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add core via extension method
builder.Services.AddCore(builder.Configuration);

builder.Services.AddAzureOpenAi(builder.Configuration); 
builder.Services.AddSingleton<IAnalysisService, AnalysisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map API routes
var api = app.MapGroup("/api");
api.MapQuestionRoutes();
api.MapResultRoutes();

app.Run();
