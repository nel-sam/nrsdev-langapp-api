---
name: question-seed-data
description: 'Author or extend multiple-choice question seed data in TempData/questions-{lang}.json. Use when adding questions, adding a new language file (Japanese, Korean, English...), fixing seed JSON, or validating question data against the Question model.'
argument-hint: 'language code and topic, e.g. "jp particles" or "kr new file"'
---

# Question Seed Data

Static question banks live in `TempData/questions-{lang}.json` and are deserialized
into `Models.Question` ([Data/Models/Question.cs](../../../Data/Models/Question.cs)).
The JSON keys are **kebab-case** and must match the `[JsonPropertyName]` attributes exactly.

## Schema (per question)

| Key | Type | Rules |
|---|---|---|
| `question` | string | Target-language sentence with a single `___` blank, or a short question |
| `tested-abilities` | string[] | 2–3 kebab-case tags; reuse existing tags before inventing new ones |
| `correct-answer-index` | int | 0-based index into `choices`, never the last item |
| `choices` | string[] | Exactly 6 items; last item is always `"unknown"` |

See [question.example.json](./assets/question.example.json) for a canonical entry.

## Content rules

- One unambiguous correct answer; distractors must be plausible for the level
  (same part of speech / same grammatical slot), not obviously wrong.
- Match the level of the existing file (current JP file is beginner / JLPT N5: hiragana
  only, no kanji unless the tag is `basic-kanji`).
- Target language only inside `question` and `choices`. No translations or hints in the text.
- Vary the correct index across the file; don't cluster on index 0.
- Cover a spread of abilities. Avoid near-duplicate tags (prefer `daily-life` over
  adding `daily-life-vocabulary`). Run the validator with `--tags` to see what exists.

## Procedure

1. Read the target file. If it doesn't exist, copy [question.example.json](./assets/question.example.json)
   as the starting array.
2. List existing tags: `python3 .github/skills/question-seed-data/scripts/validate.py TempData/questions-jp.json --tags`
3. Add or edit questions following the schema and content rules above.
4. Validate: `python3 .github/skills/question-seed-data/scripts/validate.py TempData/questions-jp.json`
   The script fails on bad keys, wrong choice count, out-of-range index, missing
   `"unknown"`, or a correct answer that is the last choice.
5. If this is a new language, remember `/questions/{language}` currently hardcodes
   `questions-jp.json` in [QuestionRoutes.cs](../../../Routes/QuestionRoutes.cs). Ask
   before changing that (it's a listed Known Gap in AGENTS.MD).
6. Run `dotnet run` and `GET /api/questions/{lang}` to confirm it deserializes.
