---
name: api-smoke-test
description: 'Run an end-to-end smoke test of the running API: GET /api/questions, POST /api/results, and the 429 rate-limit behaviour. Use after changing routes, Program.cs, rate limiting, or seed data, or when asked to verify, smoke test, or sanity check the API since there is no test project.'
---

# API Smoke Test

There is no xUnit project yet, so this script is the regression safety net.
It boots the API if nothing is listening on port 5030, runs the checks below, and
stops the app it started.

## Checks

| # | Request | Expectation |
|---|---|---|
| 1 | `GET /api/questions/jp` | 200, JSON array of exactly 8 `Question` objects with kebab-case keys |
| 2 | `POST /api/results` (valid body) | 200, JSON with non-empty `feedback`. Skipped when `SKIP_LLM=1` (no Azure OpenAI secrets) |
| 3 | `POST /api/results` with malformed JSON, repeated | 400 until the token bucket (5 per 10 min per IP) is empty, then 429 by request 6 |

Check 3 uses malformed bodies on purpose: the rate limiter runs before model binding,
so tokens are consumed without paying for LLM calls.

## Procedure

1. `dotnet build` first if code changed.
2. `bash .github/skills/api-smoke-test/scripts/smoke.sh`
   - `SKIP_LLM=1 bash ...smoke.sh` when Azure OpenAI isn't configured.
   - `LANGAPP_HOST=http://localhost:1234 bash ...smoke.sh` to target another port.
3. Exit code 0 means all checks passed; failures print the offending response.

**Side effect:** the test drains the `/api/results` rate-limit bucket for the
calling IP. Wait 10 minutes or restart the app before manual testing.

## Extending

When adding an endpoint via the `add-endpoint` skill, append a `check` call in
[smoke.sh](./scripts/smoke.sh) using the existing helpers (`expect_status`, `json_py`).
Keep checks independent of LLM output text; assert on shape and status only.
