#!/usr/bin/env python3
"""Validate a TempData/questions-*.json file against the Models.Question schema.

Usage:
  validate.py <file.json>          # validate, exit 1 on any error
  validate.py <file.json> --tags   # print existing tested-abilities tags with counts
"""
import json
import sys
from collections import Counter

REQUIRED_KEYS = {"question", "tested-abilities", "correct-answer-index", "choices"}
CHOICE_COUNT = 6
LAST_CHOICE = "unknown"


def main() -> int:
    if len(sys.argv) < 2:
        print(__doc__)
        return 2

    path = sys.argv[1]
    with open(path, encoding="utf-8") as f:
        questions = json.load(f)

    if "--tags" in sys.argv:
        tags = Counter(t for q in questions for t in q.get("tested-abilities", []))
        for tag, n in sorted(tags.items()):
            print(f"{n:3}  {tag}")
        return 0

    errors: list[str] = []
    seen_text: set[str] = set()

    for i, q in enumerate(questions):
        where = f"[{i}] {str(q.get('question', ''))[:40]!r}"
        keys = set(q)
        if keys != REQUIRED_KEYS:
            errors.append(f"{where}: keys {sorted(keys ^ REQUIRED_KEYS)} unexpected/missing")
            continue

        text = q["question"]
        if not isinstance(text, str) or not text.strip():
            errors.append(f"{where}: empty question")
        elif text in seen_text:
            errors.append(f"{where}: duplicate question text")
        seen_text.add(text)

        abilities = q["tested-abilities"]
        if not isinstance(abilities, list) or not abilities:
            errors.append(f"{where}: tested-abilities must be a non-empty list")
        else:
            for t in abilities:
                if not isinstance(t, str) or t != t.lower() or " " in t:
                    errors.append(f"{where}: tag {t!r} must be lowercase kebab-case")

        choices = q["choices"]
        if not isinstance(choices, list) or len(choices) != CHOICE_COUNT:
            got = len(choices) if isinstance(choices, list) else type(choices).__name__
            errors.append(f"{where}: expected {CHOICE_COUNT} choices, got {got}")
        else:
            if choices[-1] != LAST_CHOICE:
                errors.append(f"{where}: last choice must be {LAST_CHOICE!r}")
            if len(set(choices)) != len(choices):
                errors.append(f"{where}: duplicate choices")

        idx = q["correct-answer-index"]
        if not isinstance(idx, int) or isinstance(idx, bool):
            errors.append(f"{where}: correct-answer-index must be an int")
        elif isinstance(choices, list):
            if not 0 <= idx < len(choices):
                errors.append(f"{where}: correct-answer-index {idx} out of range")
            elif idx == len(choices) - 1:
                errors.append(f"{where}: correct answer cannot be {LAST_CHOICE!r}")

    if errors:
        print("\n".join(errors))
        print(f"\n{len(errors)} error(s) in {len(questions)} questions")
        return 1

    idx_dist = Counter(q["correct-answer-index"] for q in questions)
    print(f"OK: {len(questions)} questions, correct-index distribution {dict(sorted(idx_dist.items()))}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
