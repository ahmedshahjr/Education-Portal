#!/usr/bin/env python3
"""Architecture diagram guard: a pull request that changes how the system is wired must update the matching
architecture flow diagram in README.md in the same pull request (AGENTS.md section 7, rule 8).

readme-guard.py only asks that README.md changes somewhere. This guard is stricter: it knows which README
section draws which part of the code (the RULES table below, mirrored by the "Diagram index" table in the
README) and fails when a change to that code leaves its section untouched.

Runs the same way for every contributor and AI tool: in CI (.github/workflows/compliance-guards.yml) on
every pull request, or by hand before you push:

  python3 docs/ai/architecture-diagram-guard.py --base origin/development
  python3 docs/ai/architecture-diagram-guard.py --base origin/development --pr-body-file body.md
  python3 docs/ai/architecture-diagram-guard.py --check-headings      # also runs on every push

--check-headings only checks that every README section named in RULES still exists, so renaming a
heading without updating this file fails too.

When a flagged change really does not change any diagram, write one line in the PR description:

    Diagrams: not needed - <reason, for example "bug fix inside TenantResolution, flow unchanged">

A reviewer checks that reason. Standard library only. Exit code 1 when the guard fails.
"""
from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOL = "Education Portal Full Stack App/"
README = "README.md"
OPT_OUT = re.compile(r"^\s*[-*]?\s*Diagrams?:\s*not needed\s*[-:–—]\s*\S.{9,}", re.IGNORECASE | re.MULTILINE)

# README sections (heading text without the #s). Each rule is satisfied when README.md changes inside ANY of
# its sections; a section includes its sub-sections.
OVERVIEW = "Architecture flow diagram"
RUNTIME = "The system at runtime"
LAYERS = "Clean Architecture layers"
REQUEST = "How a request flows"
TENANCY = "Multi-tenancy and permissions (step 2, in progress)"
ASPIRE = "Aspire orchestration flow"
MIDDLEWARE = "ASP.NET Core middleware pipeline"
TENANT = "Tenant resolution flow"
SIGN_IN = "Sign-in flow (ASP.NET Core Identity)"
AUTHZ = "Authorization and module check flow"
EFCORE = "EF Core and PostgreSQL flow"
SECRETS = "Secrets flow"
OBSERVABILITY = "Observability flow (OpenTelemetry)"
LOCALIZATION = "Localization flow"
ANGULAR = "Angular start-up flow"
NSWAG = "NSwag API client generation flow"
TESTS = "Test flow"
GUARDS = "Compliance guards flow"
ANY_DIAGRAM = ("Architecture overview", "Technology flows")


@dataclass(frozen=True)
class Rule:
    """A changed path that matches `path` (regex on the repo-relative path) needs one of `sections` updated.
    `added_or_removed`: only files added, deleted or renamed count. `diff`: only count the file when a changed
    line matches this regex, so ordinary edits inside the file do not trigger the guard."""
    path: str
    sections: tuple[str, ...]
    why: str
    added_or_removed: bool = False
    diff: str | None = None


S = re.escape(SOL)
RULES: tuple[Rule, ...] = (
    Rule(rf"{S}src/AppHost/(?!Properties/).+", (RUNTIME, ASPIRE, OBSERVABILITY, OVERVIEW),
         "Aspire resources and start order"),
    Rule(rf"{S}src/Web/Program\.cs", (MIDDLEWARE, OVERVIEW),
         "middleware order in the request pipeline", diff=r"\bapp\.(Use|Map)\w*|builder\.\w+"),
    Rule(rf"{S}src/Web/Infrastructure/TenantResolution\.cs|{S}src/Infrastructure/Identity/TenantResolver\.cs",
         (TENANT, TENANCY), "tenant resolution from the host"),
    Rule(rf"{S}src/Infrastructure/Identity/(IdentityService|ApplicationUserClaimsPrincipalFactory|SessionLifetime)\.cs"
         rf"|{S}src/Web/Endpoints/Users\.cs", (SIGN_IN,),
         "sign-in, claims or sessions", diff=r"\bMap(Get|Post|Put|Delete|Patch)\b|SignIn|Claim|Cookie|Session|Lockout"),
    Rule(rf"{S}src/Application/Common/Behaviours/[^/]+\.cs", (REQUEST, AUTHZ, OVERVIEW),
         "MediatR pipeline behaviour added or removed", added_or_removed=True),
    Rule(rf"{S}src/Application/DependencyInjection\.cs", (REQUEST, OVERVIEW),
         "MediatR pipeline order", diff=r"AddOpenBehavior|AddBehavior"),
    Rule(rf"{S}src/Application/Common/Behaviours/AuthorizationBehaviour\.cs", (AUTHZ, TENANCY),
         "module, role or branch checks", diff=r"Module|Role|Policy|Branch|Forbidden|Unauthorized"),
    Rule(rf"{S}src/Infrastructure/Data/ApplicationDbContext\.cs", (EFCORE, TENANCY),
         "tenant or soft-delete query filters", diff=r"HasQueryFilter|IsMasterTenant|CurrentSchoolId|CurrentBranchId"),
    Rule(rf"{S}src/Infrastructure/Data/Interceptors/[^/]+\.cs", (EFCORE,),
         "SaveChanges interceptor added or removed", added_or_removed=True),
    Rule(rf"{S}src/Infrastructure/Secrets/[^/]+\.cs", (SECRETS,),
         "secret provider added or removed", added_or_removed=True),
    Rule(rf"{S}src/Infrastructure/Observability/[^/]+\.cs", (OBSERVABILITY,),
         "observability component added or removed", added_or_removed=True),
    Rule(rf"{S}src/Web/Infrastructure/TenantRequestLocalization\.cs", (LOCALIZATION,),
         "request culture providers", diff=r"Provider|Culture"),
    Rule(rf"{S}src/Web/ClientApp/src/(main\.ts|app/app\.config\.ts)", (ANGULAR,),
         "Angular start-up and providers", diff=r"provide\w*\(|loadTranslations|bootstrapApplication|APP_INITIALIZER|fetch\("),
    Rule(rf"{S}src/Web/ClientApp/nswag\.json", (NSWAG,), "API client generation"),
    Rule(rf"{S}.+\.csproj", (LAYERS, OVERVIEW), "project added or removed", added_or_removed=True),
    Rule(rf"{S}src/.+\.csproj", (LAYERS,), "project references between layers", diff=r"<ProjectReference"),
    Rule(rf"{S}tests/TestAppHost/.+", (TESTS,), "test host resources"),
    Rule(r"\.github/workflows/[^/]+\.ya?ml|\.githooks/[^/]+|docs/ai/[^/]+-guard\.py", (GUARDS,),
         "CI workflow, git hook or guard added or removed", added_or_removed=True),
    Rule(rf"{S}docs/decisions/ADR-(?!000)[^/]+\.md", ANY_DIAGRAM,
         "architecture decision record added", added_or_removed=True),
)


def git(*args: str) -> str:
    return subprocess.run(["git", *args], cwd=ROOT, check=True, capture_output=True, text=True).stdout


def changed_files(base: str) -> list[tuple[str, str]]:
    """(status letter, path) for each file changed since the merge base. Renames also list the old path."""
    result = []
    for line in git("diff", "--name-status", "--find-renames", f"{base}...HEAD").splitlines():
        parts = line.split("\t")
        result.append((parts[0][0], parts[-1]))
        if parts[0][0] == "R":
            result.append(("D", parts[1]))
    return result


def changed_lines(base: str, path: str) -> list[str]:
    out = git("diff", "-U0", f"{base}...HEAD", "--", path)
    return [l[1:] for l in out.splitlines() if l[:1] in "+-" and not l.startswith(("+++", "---"))]


def readme_sections(text: str) -> dict[str, tuple[int, int]]:
    """Heading text -> (first line, last line), 1-based, including sub-sections. Ignores fenced code."""
    headings: list[tuple[int, int, str]] = []
    fenced = False
    lines = text.splitlines()
    for number, line in enumerate(lines, start=1):
        if line.startswith("```"):
            fenced = not fenced
        elif not fenced and (m := re.match(r"^(#{1,6})\s+(.+?)\s*$", line)):
            headings.append((number, len(m.group(1)), m.group(2)))
    sections = {}
    for i, (start, level, title) in enumerate(headings):
        end = next((h[0] - 1 for h in headings[i + 1:] if h[1] <= level), len(lines))
        sections[title] = (start, end)
    return sections


def readme_hunks(base: str) -> list[tuple[int, int]]:
    """Line ranges of README.md (new side) touched by the diff. A pure deletion counts at the line it was at."""
    hunks = []
    for m in re.finditer(r"^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@", git("diff", "-U0", f"{base}...HEAD", "--", README), re.MULTILINE):
        start, count = int(m.group(1)), int(m.group(2) or "1")
        hunks.append((start, start + max(count, 1) - 1))
    return hunks


def check_headings(sections: dict[str, tuple[int, int]]) -> list[str]:
    wanted = {s for rule in RULES for s in rule.sections}
    return sorted(s for s in wanted if s not in sections)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--base", help="branch the PR merges into, e.g. origin/development")
    parser.add_argument("--pr-body-file", help="file holding the PR description (for the opt-out line)")
    parser.add_argument("--check-headings", action="store_true", help="only check that the README sections exist")
    args = parser.parse_args()

    sections = readme_sections((ROOT / README).read_text(encoding="utf-8"))
    missing = check_headings(sections)
    if missing:
        print("Architecture diagram guard: README.md no longer has these sections named in docs/ai/architecture-diagram-guard.py:")
        for title in missing:
            print(f"  - {title}")
        print("Rename them back, or update RULES in the guard and the Diagram index table in the README together.")
        if "GITHUB_ACTIONS" in os.environ:
            print("::error::README diagram sections and docs/ai/architecture-diagram-guard.py are out of sync.")
        return 1
    if args.check_headings:
        print(f"Architecture diagram guard: all {len({s for r in RULES for s in r.sections})} README diagram sections exist.")
        return 0
    if not args.base:
        parser.error("--base is required unless --check-headings is given")

    files = changed_files(args.base)
    hunks = readme_hunks(args.base) if any(p == README for _, p in files) else []

    def touched(title: str) -> bool:
        start, end = sections[title]
        return any(a <= end and b >= start for a, b in hunks)

    stale: list[str] = []
    for status, path in files:
        for rule in RULES:
            if not re.fullmatch(rule.path, path):
                continue
            if rule.added_or_removed and status not in {"A", "D", "R"}:
                continue
            if rule.diff and not any(re.search(rule.diff, l) for l in changed_lines(args.base, path)):
                continue
            if not any(touched(s) for s in rule.sections):
                names = " or ".join(f'"{s}"' for s in rule.sections)
                stale.append(f"{path}: {rule.why} -> update {names}")

    if not stale:
        print("Architecture diagram guard: every architecture change has its README diagram section updated.")
        return 0

    body = Path(args.pr_body_file).read_text(encoding="utf-8") if args.pr_body_file else ""
    match = OPT_OUT.search(body)
    if match:
        print(f"Architecture diagram guard: skipped by the PR description: {match.group(0).strip()}")
        for item in dict.fromkeys(stale):
            print(f"  - {item}")
        return 0

    print("Architecture diagram guard: these changes alter what a README architecture diagram shows,")
    print("but its section in README.md is not changed:")
    for item in dict.fromkeys(stale):
        print(f"  - {item}")
    print("Update the diagram (and its walkthrough) in the named README section in this PR (AGENTS.md section 7, rule 8),")
    print('or, if the flow really is unchanged, add "Diagrams: not needed - <reason>" to the PR description.')
    if "GITHUB_ACTIONS" in os.environ:
        print("::error::README architecture diagrams must be updated in the same PR as the code they draw (AGENTS.md section 7, rule 8).")
    return 1


if __name__ == "__main__":
    sys.exit(main())
