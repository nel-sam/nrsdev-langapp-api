#!/usr/bin/env bash
# POST each sample practice session to /api/results and print the feedback.
# Usage: run-samples.sh [sample.json ...]   (defaults to all files in assets/samples)
set -euo pipefail

HOST="${LANGAPP_HOST:-http://localhost:5030}"
DIR="$(cd "$(dirname "$0")/.." && pwd)"

if [ $# -gt 0 ]; then
  files=("$@")
else
  files=("$DIR"/assets/samples/*.json)
fi

for f in "${files[@]}"; do
  echo "=================================================================="
  echo "SAMPLE: $(basename "$f")"
  echo "------------------------------------------------------------------"
  status=$(curl -s -o /tmp/langapp-sample.out -w '%{http_code}' \
    -X POST "$HOST/api/results" \
    -H 'Content-Type: application/json' \
    --data-binary "@$f")
  echo "HTTP $status"
  if [ "$status" = "200" ]; then
    python3 -c 'import json,sys; print(json.load(sys.stdin)["feedback"])' < /tmp/langapp-sample.out
  else
    cat /tmp/langapp-sample.out; echo
    [ "$status" = "429" ] && echo "(rate limited: 5 requests / 10 min per IP)"
  fi
  echo
done
