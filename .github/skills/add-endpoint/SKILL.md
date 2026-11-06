---
name: add-endpoint
description: 'Add a new minimal API endpoint or route group to this project following the house pattern (static XRoutes class, MapXRoutes extension, /api group registration, .http file entry). Use when asked to add, create, or expose a new API route, endpoint, or resource.'
argument-hint: 'resource and verbs, e.g. "sessions GET/POST"'
---

# Add Endpoint

Minimal APIs only; no controllers. One static class per resource in `Routes/`.
Reference implementations: [QuestionRoutes.cs](../../../Routes/QuestionRoutes.cs) (GET)
and [ResultRoutes.cs](../../../Routes/ResultRoutes.cs) (POST with DTO + service + rate limit).

## Procedure

1. **Create `Routes/<Resource>Routes.cs`** from [XRoutes.cs.template](./assets/XRoutes.cs.template).
   - `namespace Routes;`, `public static class <Resource>Routes`,
     `public static void Map<Resource>Routes(this IEndpointRouteBuilder app)`.
   - Paths are relative to the `/api` group: write `"/sessions"`, not `"/api/sessions"`.
   - Inject dependencies as lambda parameters; return `Results.*` helpers
     (`Results.Ok`, `Results.Created`, `Results.NotFound`, `Results.ValidationProblem`).
   - Add `.WithName("...")` for OpenAPI; add `.RequireRateLimiting("limit")` on any
     endpoint that calls the LLM or is otherwise expensive.
2. **DTOs**: request/response types used only by this route go in the same file as
   `internal sealed record`. Types shared with services go in `Data/Models/` with
   `namespace Models;`. Dapper-materialized models are classes with settable properties.
3. **Register** in [Program.cs](../../../Program.cs) next to the others:
   `api.Map<Resource>Routes();`
4. **Service needed?** Add an interface in `Interfaces/`, implementation in `Services/`,
   and register via an `IServiceCollection` extension method (see
   `CoreServiceCollectionExtensions.AddCore`) or alongside the existing
   `AddSingleton<IAnalysisService, AnalysisService>()` line. Never register inline
   elsewhere. Data access goes through `IDbConnectionFactory` with Dapper parameters;
   new tables are added to the startup `CREATE TABLE IF NOT EXISTS` SQL.
5. **Add a request** to [nrsdev-langapp-api.http](../../../nrsdev-langapp-api.http)
   using `{{nrsdev_langapp_api_HostAddress}}`, separated by `###`.
6. `dotnet build`, then `dotnet run` and exercise the new request from the `.http`
   file or check it appears at `http://localhost:5030/scalar/v1`.
7. Run the smoke test if it exists: `bash .github/skills/api-smoke-test/scripts/smoke.sh`
   and extend it when the new endpoint is stable.

## Checklist

- [ ] Route file in `Routes/`, namespace `Routes`
- [ ] Registered under the `/api` group in `Program.cs`
- [ ] DTOs `internal sealed record` (route-local) or in `Data/Models` (shared)
- [ ] `.WithName()` set; rate limit applied if expensive
- [ ] `.http` entry added
- [ ] Nullable warnings resolved without `!`
