# .NET Development Preferences

## General Philosophy

- Prefer simple, maintainable solutions over clever ones.
- Optimize for readability and ease of maintenance.
- Favor real-world engineering practices over academic patterns.
- Do not introduce abstraction until there is a demonstrated need.
- Assume an experienced senior developer audience.
- Skip beginner explanations unless explicitly requested.
- Explain tradeoffs when proposing significant architectural changes.

## Architecture

- Prefer straightforward architectures.
- Avoid over-engineering.
- Avoid unnecessary layers.
- Prefer vertical slices or feature-oriented organization when practical.
- Keep boundaries clear but lightweight.

## Dependency Injection

- Use built-in ASP.NET Core dependency injection.
- Avoid complex DI frameworks.
- Register services using standard patterns.
- Keep service lifetimes simple and intentional.

## APIs

- Prefer ASP.NET Core Minimal APIs for simple applications and services.
- Use Controllers when complexity, conventions, or team requirements justify them.
- Return appropriate HTTP status codes.
- Prefer explicit endpoint definitions over excessive magic.

## Data Access

- Use Dapper in this project.
- Use projections instead of loading entire entities when appropriate.
- Avoid premature micro-optimizations.
- Optimize only after identifying an actual performance concern.

## Models

- Prefer records for immutable DTOs.
- Use meaningful names.
- Keep DTOs and domain entities separate when it provides actual value.
- Avoid unnecessary mapping layers.

## C#

### Prefer modern language features

Use:

- records
- init properties
- primary constructors where they improve clarity
- pattern matching
- switch expressions
- file-scoped namespaces
- global using statements when appropriate
- nullable reference types

Example:

```csharp
public record UserDto(
    Guid Id,
    string Name,
    string Email);