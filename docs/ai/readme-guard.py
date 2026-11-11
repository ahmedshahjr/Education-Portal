#!/usr/bin/env python3
"""README guard: a pull request that changes the technologies, flows, setup or configuration of the
product must update README.md in the same pull request (AGENTS.md section 7, rule 8).

Runs the same way for every contributor and AI tool: in CI (.github/workflows/compliance-guards.yml)
on every pull request, or by hand before you push:

  python3 docs/ai/readme-guard.py --base origin/development
  python3 docs/ai/readme-guard.py --base origin/development --pr-body-file body.md

It compares the branch with the merge base of --base and lists the changes that a README reader
would care about. If any are found and README.md is unchanged, it fails.

When a flagged change really does not affect the README, write one line in the PR description:

    README: not needed - <reason, for example "only a bug fix inside AppHost, no new resource">

A reviewer checks that reason. Standard library only. Exit code 1 when the guard fails.
"""
from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOL = "Education Portal Full Stack App/"
README = "README.md"
OPT_OUT = re.compile(r"^\s*[-*]?\s*README:\s*not needed\s*[-:–—]\s*\S.{9,}", re.IGNORECASE | re.MULTILINE)

ADDED_OR_REMOVED = {"A", "D", "R"}


def git(*args: str) -> str:
    return subprocess.run(["git", *args], cwd=ROOT, check=True, capture_output=True, text=True).stdout


def changed_files(base: str) -> list[tuple[str, str]]:
    """(status letter, path) for each file changed since the merge base. Renames give the new path."""
    out = git("diff", "--name-status", "--find-renames", f"{base}...HEAD")
    result = []
    for line in out.splitlines():
        parts = line.split("\t")
        result.append((parts[0][0], parts[-1]))
        if parts[0][0] == "R":
            result.append(("D", parts[1]))
    return result


def show(rev: str, path: str) -> str:
    try:
        return git("show", f"{rev}:{path}")
    except subprocess.CalledProcessError:
        return ""


def package_names_props(text: str) -> set[str]:
    return set(re.findall(r'<PackageVersion\s+Include="([^"]+)"', text))


def package_names_npm(text: str) -> set[str]:
    try:
        data = json.loads(text) if text else {}
    except json.JSONDecodeError:
        return set()
    return {name for key in ("dependencies", "devDependencies") for name in data.get(key, {})}


def package_change(base: str, path: str, names) -> str | None:
    """Only a package added or removed counts; version bumps (Dependabot) do not."""
    merge_base = git("merge-base", base, "HEAD").strip()
    before, after = names(show(merge_base, path)), names(show("HEAD", path))
    added, removed = sorted(after - before), sorted(before - after)
    if not added and not removed:
        return None
    bits = []
    if added:
        bits.append("added " + ", ".join(added))
    if removed:
        bits.append("removed " + ", ".join(removed))
    return "; ".join(bits)


def triggers(base: str, files: list[tuple[str, str]]) -> list[str]:
    found: list[str] = []
    for status, path in files:
        rel = path[len(SOL):] if path.startswith(SOL) else None

        if rel == "Directory.Packages.props":
            detail = package_change(base, path, package_names_props)
            if detail:
                found.append(f"{path}: NuGet packages {detail} (Tech stack)")
        elif rel == "src/Web/ClientApp/package.json":
            detail = package_change(base, path, package_names_npm)
            if detail:
                found.append(f"{path}: npm packages {detail} (Tech stack)")
        elif rel in ("global.json", "Directory.Build.props"):
            found.append(f"{path}: SDK or build settings (Tech stack, Prerequisites)")
        elif rel is not None and rel.startswith("src/AppHost/") and not rel.startswith("src/AppHost/Properties/"):
            found.append(f"{path}: Aspire resources (The system at runtime, Configuration and ports)")
        elif rel is not None and re.fullmatch(r"src/[^/]+/appsettings(\.[^/]+)?\.json", rel):
            found.append(f"{path}: configuration (Configuration and ports)")
        elif rel == "src/Web/Program.cs":
            found.append(f"{path}: request pipeline (How a request flows)")
        elif status in ADDED_OR_REMOVED and path.endswith(".csproj"):
            found.append(f"{path}: project added or removed (Repository layout, Clean Architecture layers)")
        elif status in ADDED_OR_REMOVED and rel is not None and re.fullmatch(r"src/Web/Endpoints/[^/]+\.cs", rel):
            found.append(f"{path}: endpoint group added or removed (Architecture overview)")
        elif status in ADDED_OR_REMOVED and rel is not None and re.fullmatch(r"src/Domain/Entities/[^/]+\.cs", rel):
            found.append(f"{path}: entity added or removed (Architecture overview, Data and database)")
        elif status in ADDED_OR_REMOVED and rel is not None and rel.startswith("docs/decisions/"):
            found.append(f"{path}: decision record added or removed (Architecture overview)")
        elif status in ADDED_OR_REMOVED and (path.startswith(".github/workflows/") or path.startswith(".githooks/")):
            found.append(f"{path}: CI workflow or git hook added or removed (Development process)")
    return found


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--base", required=True, help="branch the PR merges into, e.g. origin/development")
    parser.add_argument("--pr-body-file", help="file holding the PR description (for the opt-out line)")
    args = parser.parse_args()

    files = changed_files(args.base)
    if any(path == README for _, path in files):
        print("README guard: README.md is updated in this change.")
        return 0

    found = triggers(args.base, files)
    if not found:
        print("README guard: no technology, flow, setup or configuration change found.")
        return 0

    body = Path(args.pr_body_file).read_text(encoding="utf-8") if args.pr_body_file else ""
    match = OPT_OUT.search(body)
    if match:
        print(f"README guard: skipped by the PR description: {match.group(0).strip()}")
        for item in found:
            print(f"  - {item}")
        return 0

    print("README guard: these changes affect what README.md describes, but README.md is not changed:")
    for item in found:
        print(f"  - {item}")
    print("Update the README sections named in brackets in this PR (AGENTS.md section 7, rule 8),")
    print('or, if the README really is unaffected, add "README: not needed - <reason>" to the PR description.')
    if "GITHUB_ACTIONS" in os.environ:
        print("::error::README.md must be updated in the same PR as technology, flow, setup or configuration changes (AGENTS.md section 7, rule 8).")
    return 1


if __name__ == "__main__":
    sys.exit(main())
