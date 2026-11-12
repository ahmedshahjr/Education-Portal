#!/usr/bin/env python3
"""Review checklist guard: every pull request answers the six review questions (REVIEW-01..06) in the
"Review checklist" section of its description, and ticks each one (AGENTS.md section 7, rule 9).

Runs the same way for every contributor and AI tool: in CI (.github/workflows/compliance-guards.yml)
on every pull request, or by hand on a saved PR description before you open the PR:

  python3 docs/ai/review-checklist-guard.py --pr-body-file body.md

The questions come from .github/pull_request_template.md. Each line must keep its tag, e.g.
"(REVIEW-03)", so the wording can be improved without breaking the check. A ticked box means the
author asked the question honestly of the change, read the code (not the PR description) to answer
it, and wrote any finding that is not obvious from the diff under "What and why".

Standard library only. Exit code 1 when the guard fails.
"""
from __future__ import annotations

import argparse
import os
import re
import sys
from pathlib import Path

SECTION = "Review checklist"

QUESTIONS = {
    "REVIEW-01": "Does it actually do what it claims? Read the logic, not the PR description.",
    "REVIEW-02": "Does it fit the architecture, or is it a local shortcut?",
    "REVIEW-03": "What does this change break somewhere else?",
    "REVIEW-04": "Are nulls, empty inputs, errors and retries handled?",
    "REVIEW-05": "Do the tests cover real failures, or only the happy path?",
    "REVIEW-06": "Is it over-engineered or under-built for the real constraints?",
}

HEADING = re.compile(r"^\s{0,3}#{1,6}\s+(.*?)\s*#*\s*$")
CHECKBOX = re.compile(r"^\s*[-*+]\s+\[(?P<mark>[ xX])\]\s+(?P<text>.*)$")
TAG = re.compile(r"\bREVIEW-\d{2}\b")


def section_lines(body: str) -> list[str] | None:
    """Lines under the 'Review checklist' heading, up to the next heading of any level. None if absent."""
    lines = body.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    start = None
    for i, line in enumerate(lines):
        m = HEADING.match(line)
        if not m:
            continue
        if start is not None:
            return lines[start:i]
        if SECTION.lower() in m.group(1).lower():
            start = i + 1
    return lines[start:] if start is not None else None


def problems(body: str) -> list[str]:
    lines = section_lines(body)
    if lines is None:
        return [f"the PR description has no '{SECTION}' section (copy it from .github/pull_request_template.md)"]

    # Comments in the template (<!-- ... -->) are not answers.
    text = re.sub(r"<!--.*?-->", "", "\n".join(lines), flags=re.DOTALL)
    found: dict[str, list[bool]] = {}
    for line in text.split("\n"):
        m = CHECKBOX.match(line)
        if not m:
            continue
        for tag in TAG.findall(m.group("text")):
            found.setdefault(tag, []).append(m.group("mark") in "xX")

    result = []
    for tag, question in QUESTIONS.items():
        marks = found.get(tag)
        if not marks:
            result.append(f"{tag} is missing: {question}")
        elif len(marks) > 1:
            result.append(f"{tag} appears {len(marks)} times; keep exactly one line for it")
        elif not marks[0]:
            result.append(f"{tag} is not ticked: {question}")
    for tag in sorted(set(found) - set(QUESTIONS)):
        result.append(f"{tag} is not a known review question (known: {', '.join(QUESTIONS)})")
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--pr-body-file", required=True, help="file holding the PR description")
    args = parser.parse_args()

    path = Path(args.pr_body_file)
    if not path.is_file():
        print(f"Review checklist guard: PR description file not found: {path}")
        return 1
    body = path.read_text(encoding="utf-8")
    found = problems(body)
    if not found:
        print(f"Review checklist guard: all {len(QUESTIONS)} review questions are answered and ticked.")
        return 0

    print("Review checklist guard: the PR description does not answer every review question:")
    for item in found:
        print(f"  - {item}")
    print("Ask each question of the change, read the code to answer it, fix what you find or explain it")
    print("under 'What and why', then tick the box (AGENTS.md section 7, rule 9).")
    if "GITHUB_ACTIONS" in os.environ:
        print(f"::error::Every PR must answer and tick the {len(QUESTIONS)} questions in the '{SECTION}' (AGENTS.md section 7, rule 9).")
    return 1


if __name__ == "__main__":
    sys.exit(main())
