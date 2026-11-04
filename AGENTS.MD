# nrsdev-langapp-api

ASP.NET Core 10 minimal API for a language-learning app. Serves multiple-choice
questions, accepts a learner's answers, and uses an LLM to produce study feedback.

## Stack

- .NET 10, minimal APIs only — no MVC controllers
- Dapper + `Microsoft.Data.Sqlite` for data access — **no Entity Framework**
- `Microsoft.Extensions.AI` (`IChatClient`) over Azure OpenAI for analysis
- `System.Text.Json` for serialization
- OpenAPI via `AddOpenApi()` / `MapOpenApi()` (Development only)

## Project Layout

| Folder | Purpose | Namespace |
|---|---|---|
| `Routes/` | Endpoint groups, one static class per resource | `Routes` |
| `Services/` | Business logic + DI registration extension methods | `Services` |
| `Interfaces/` | Service contracts | `Interfaces` |
| `Data/` | `IDbConnectionFactory` and SQLite implementation | `Data` |
| `Data/Models/` | Shared domain/data records | `Models` |
| `TempData/` | Static JSON seed data (e.g. `questions-jp.json`) | — |

Namespaces are flat folder names, **not** rooted in `nrsdev_langapp_api`. Follow
the existing namespace of the folder you're adding to.

## Conventions

### Endpoints
- Each resource gets a `static class XRoutes` with a
  `MapXRoutes(this IEndpointRouteBuilder app)` extension method.
- Register it in `Program.cs` under the `/api` group: `api.MapXRoutes();`
- Request/response DTOs specific to one route live in that route file as
  `internal sealed record`. Types shared across layers go in `Data/Models/`.
- Return `Results.*` helpers; inject dependencies as lambda parameters.

### Services & DI
- Register services through extension methods on `IServiceCollection`
  (see `CoreServiceCollectionExtensions.AddCore`, `AzureOpenAiExtensions.AddAzureOpenAi`),
  then call them from `Program.cs`. Don't register services inline in `Program.cs`.
- Services depend on interfaces from `Interfaces/`, never on concrete classes.
- Read config inside the extension method and fail fast with
  `InvalidOperationException` if a required value is missing.

### Data access
- Inject `IDbConnectionFactory` (singleton). Open a connection per operation:
  ```csharp
  using var connection = factory.CreateConnection();
  var rows = await connection.QueryAsync<T>(sql, new { Param });
  ```
- Always use Dapper parameters — never interpolate values into SQL.
- No migrations framework exists. Schema is created with `CREATE TABLE IF NOT EXISTS`
  at startup; update that SQL when adding tables.
- Models materialized by Dapper should be classes with a parameterless constructor
  and settable properties.

### C# style
- Prefer modern syntax: primary constructors, `record` for immutable data,
  `required` over `= string.Empty`, collection expressions (`[]`), raw string
  literals (`"""`) for prompts and SQL.
- Nullable reference types are enabled; don't suppress warnings with `!`.
- JSON keys in external data are kebab-case — map them with `[JsonPropertyName]`
  rather than a global naming policy (see `Data/Models/Question.cs`).

### LLM usage
- Code against `IChatClient`, not the Azure SDK directly.
- Prompts live in the service that uses them as raw string literals.
  Explanations go in the learner's native language; examples in the target language.

## Configuration & Secrets

| Key | Purpose |
|---|---|
| `ConnectionStrings:Default` | SQLite path, e.g. `Data Source=LangApps.db` |
| `AzureOpenAi:Endpoint` | Azure OpenAI resource URL |
| `AzureOpenAi:ApiKey` | **Secret** |
| `AzureOpenAi:Deployment` | Model deployment name |

Secrets go in **user secrets** (`dotnet user-secrets set ...`), never in
`appsettings*.json`. `*.db` files must not be committed.

## Build & Run

```sh
dotnet build
dotnet run                 # https://localhost:<port>/openapi/v1.json in Development
```

Exercise endpoints with `nrsdev-langapp-api.http`. There is no test project yet;
if you add one, use xUnit in a sibling `*.Tests` project.

## Known Gaps (don't "fix" silently — ask first)

- `Routes/ResultRoutes.cs` defines `QuestionResult`, which overlaps with
  `Models.UserResponse`. These should be consolidated.
- `/questions/{language}` ignores the `language` parameter and always reads
  `questions-jp.json`.
- `AnalysisService.AnalyzeAsync` does not yet return a value.
- Rate limiting is a TODO on the questions endpoint.