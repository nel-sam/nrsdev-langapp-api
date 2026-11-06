---
name: tune-analysis-prompt
description: 'Iterate on the LLM feedback prompt in Services/AnalysisService.cs and check output quality against sample practice sessions. Use when changing the analysis prompt, the AnalysisResult schema, feedback tone or language rules, or when feedback from POST /api/results looks wrong.'
argument-hint: 'what to change, e.g. "make feedback shorter" or "add difficulty estimate"'
---

# Tune Analysis Prompt

The prompt is a raw string literal inside `AnalyzeAsync` in
[Services/AnalysisService.cs](../../../Services/AnalysisService.cs). The response is
forced to JSON matching [Data/Models/AnalysisResult.cs](../../../Data/Models/AnalysisResult.cs)
via `ChatResponseFormat.ForJsonSchema(typeof(AnalysisResult))`.

## Hard constraints (keep these in any revision)

- Explanations in the learner's **native** language; examples in the **target** language.
- Address repeated patterns across the session, not one correction per question.
- Plain text only: no markdown, no `*` emphasis, no em dashes.
- 3–5 focus topics.
- Output must stay deserializable into `AnalysisResult`. If you add a property to the
  record, the LLM schema changes automatically; the prompt should still describe it.
- Prompt is sent as a single `ChatRole.System` message; user responses are
  interpolated via `string.Join("\n---\n", userResponses)` which relies on the
  `QuestionResult` record's `ToString()`.

## Procedure

1. Edit the prompt (or `AnalysisResult`) in `AnalysisService.cs`.
2. `dotnet build` to catch schema/record errors.
3. Start the API: `dotnet run` (listens on `http://localhost:5030`). Requires
   `AzureOpenAi:*` user secrets to be set.
4. Run the samples:
   `bash .github/skills/tune-analysis-prompt/scripts/run-samples.sh`
   Samples live in [assets/samples/](./assets/samples/) and use the `ResultRequest`
   body shape (`TargetLanguage`, `NativeLanguage`, `Results[]`).
   Pass a single file to run just one: `run-samples.sh assets/samples/mixed-en.json`
5. Review each response against the checklist below. Repeat from step 1.

**Rate limit:** `/api/results` allows **5 requests per 10 minutes per IP**. The three
bundled samples use three of them. Stop the app and wait, or temporarily raise
`TokenLimit` in `Program.cs` while tuning (revert before committing).

## Quality checklist

- [ ] Written entirely in the native language (sample files say which)
- [ ] Every example phrase is in the target language
- [ ] 3–5 topics, ordered by impact, each tied to a visible pattern in the input
- [ ] Doesn't enumerate each wrong answer individually
- [ ] Near-perfect session gets encouraging, forward-looking advice rather than invented errors
- [ ] No `*`, `#`, `-` bullets, or `—` in the text
- [ ] Level-appropriate (currently hardcoded to `"A1"` in `ResultRoutes.cs`)

## Adding a sample

Copy an existing file in `assets/samples/`, keep the `ResultRequest` shape, and
name it `<scenario>-<nativeLang>.json`. Aim for 6–10 results that exhibit one or
two deliberate recurring error patterns.
