#!/usr/bin/env bash
# Smoke test for nrsdev-langapp-api. See ../SKILL.md.
#   SKIP_LLM=1     skip the real POST /api/results (no Azure OpenAI secrets)
#   LANGAPP_HOST   override base URL (default http://localhost:5030)
set -uo pipefail

HOST="${LANGAPP_HOST:-http://localhost:5030}"
REPO="$(cd "$(dirname "$0")/../../../.." && pwd)"
SAMPLE="$REPO/.github/skills/tune-analysis-prompt/assets/samples/near-perfect-en.json"
OUT=/tmp/langapp-smoke.out
FAILS=0
APP_PID=""

pass() { echo "  PASS  $1"; }
fail() { echo "  FAIL  $1"; echo "        $(head -c 400 "$OUT")"; FAILS=$((FAILS+1)); }

json_py() { python3 -c "import json,sys; d=json.load(sys.stdin); $1" < "$OUT" 2>/dev/null; }

# expect_status <expected> <curl args...>
expect_status() {
  local want="$1"; shift
  local got
  got=$(curl -s -o "$OUT" -w '%{http_code}' "$@")
  [ "$got" = "$want" ]
}

# `dotnet run` forks the real server, so kill the listener on the port too.
cleanup() {
  if [ -n "$APP_PID" ]; then
    local port="${HOST##*:}"
    for pid in $(lsof -ti "tcp:$port" 2>/dev/null); do kill "$pid" 2>/dev/null; done
    kill "$APP_PID" 2>/dev/null
    wait "$APP_PID" 2>/dev/null
  fi
}
trap cleanup EXIT

# --- boot the app if needed -------------------------------------------------
if ! curl -s -o /dev/null "$HOST/api/questions/jp"; then
  echo "Starting API at $HOST ..."
  (cd "$REPO" && ASPNETCORE_URLS="$HOST" dotnet run --no-build >/tmp/langapp-smoke-app.log 2>&1) &
  APP_PID=$!
  for _ in $(seq 1 60); do
    curl -s -o /dev/null "$HOST/api/questions/jp" && break
    sleep 1
  done
  if ! curl -s -o /dev/null "$HOST/api/questions/jp"; then
    echo "API did not start. Log:"; tail -n 30 /tmp/langapp-smoke-app.log; exit 1
  fi
fi

echo "Smoke testing $HOST"

# --- 1. GET /api/questions/jp -------------------------------------------------
if expect_status 200 "$HOST/api/questions/jp" \
   && json_py "assert isinstance(d,list) and len(d)==8, len(d); assert all(set(q)=={'question','tested-abilities','correct-answer-index','choices'} for q in d)"; then
  pass "GET /api/questions/jp returns 8 questions"
else
  fail "GET /api/questions/jp"
fi

# --- 2. POST /api/results (real LLM call) -------------------------------------
if [ "${SKIP_LLM:-0}" = "1" ]; then
  echo "  SKIP  POST /api/results (SKIP_LLM=1)"
else
  if expect_status 200 -X POST "$HOST/api/results" -H 'Content-Type: application/json' --data-binary "@$SAMPLE" \
     && json_py "assert d['feedback'].strip()"; then
    pass "POST /api/results returns feedback"
  else
    fail "POST /api/results"
  fi
fi

# --- 3. rate limit: 5 tokens / 10 min, expect 429 by the 6th request ----------
got429=""
for i in $(seq 1 6); do
  code=$(curl -s -o "$OUT" -w '%{http_code}' -X POST "$HOST/api/results" \
         -H 'Content-Type: application/json' --data 'not json')
  if [ "$code" = "429" ]; then got429="$i"; break; fi
  if [ "$code" != "400" ]; then echo "        unexpected $code on request $i"; fi
done
if [ -n "$got429" ]; then
  pass "POST /api/results rate limited (429 on request $got429)"
else
  fail "rate limit never returned 429 after 6 requests"
fi

echo
if [ "$FAILS" -eq 0 ]; then echo "All checks passed"; else echo "$FAILS check(s) failed"; exit 1; fi
