#!/usr/bin/env python3
"""Automated checks for the rules in docs/ai/secure-compliance/ and docs/ai/engineering/ that can be
detected in source.

Runs the same way for every contributor and AI tool: in CI (.github/workflows/compliance-guards.yml),
in the git pre-commit hook (.githooks/pre-commit), or by hand.

  python3 docs/ai/compliance-guard.py              # scan the whole repo
  python3 docs/ai/compliance-guard.py --staged     # scan only files staged for commit
  python3 docs/ai/compliance-guard.py FILE ...     # scan the given files

A finding can be accepted only by a maintainer-approved exception (overview section 4), written as a
comment on the same line or the line above:

    // compliance-exception: SEC-WEB-02 Development-only CORS for the Angular dev server
    // engineering-exception: ENG-EF-06 synchronous EF Core override; the async path is used elsewhere

Known gaps that already exist in the template code are listed in compliance-guard-baseline.json with
a maximum count per file. New occurrences fail; fixing one means lowering the count in the baseline.

Standard library only. Exit code 1 when anything fails.
"""
from __future__ import annotations

import json
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BASELINE = Path(__file__).with_name("compliance-guard-baseline.json")
EXCEPTION = re.compile(r"(compliance-exception:\s*SEC|engineering-exception:\s*ENG)-[A-Z]+-\d+\s+\S")

SKIP_DIRS = {".git", "node_modules", "artifacts", "bin", "obj", "dist", ".angular", "TestResults"}
# The rule files quote the forbidden patterns on purpose.
SKIP_PREFIXES = ("docs/ai/",)
TEXT_EXT = {".cs", ".ts", ".html", ".js", ".json", ".csproj", ".props", ".targets", ".config",
            ".yml", ".yaml", ".md", ".razor", ".cshtml", ".sh", ".ps1", ".env", ".xml", ".http"}


@dataclass(frozen=True)
class Check:
    id: str
    rule: str
    message: str
    files: re.Pattern
    pattern: re.Pattern


def c(id_: str, rule: str, message: str, files: str, pattern: str, flags: int = 0) -> Check:
    return Check(id_, rule, message, re.compile(files), re.compile(pattern, flags))


CS = r"\.cs$"
NG = r"\.(ts|html)$"
SRC_CS = r"^Education Portal Full Stack App/src/.*\.cs$"
CLIENT = r"^Education Portal Full Stack App/src/Web/ClientApp/src/"
CHECKS = [
    c("cors-any-origin", "SEC-WEB-02", "AllowAnyOrigin outside Development", CS, r"\bAllowAnyOrigin\s*\("),
    c("raw-sql", "SEC-INPUT-02", "raw SQL API; use LINQ or FromSql/ExecuteSql with interpolation", CS,
      r"\b(FromSqlRaw|ExecuteSqlRaw(Async)?|SqlQueryRaw)\s*[<(]"),
    c("ignore-query-filters", "SEC-AUTHZ-03", "IgnoreQueryFilters bypasses tenant isolation", CS,
      r"\bIgnoreQueryFilters\s*\("),
    c("allow-anonymous", "SEC-AUTHN-01", "anonymous endpoint; list it in the PR and add an exception comment", CS,
      r"\bAllowAnonymous\b"),
    c("weak-crypto", "SEC-CRYPTO-01", "weak or forbidden cryptography", CS,
      r"\b(MD5|SHA1|DES|TripleDES|RC2)\s*\.\s*(Create|HashData)\b|\b(MD5|SHA1)CryptoServiceProvider\b|CipherMode\.ECB"),
    c("disable-tls-validation", "SEC-CRYPTO-03", "certificate validation disabled", r"\.(cs|json)$",
      r"DangerousAcceptAnyServerCertificateValidator|ServerCertificateCustomValidationCallback\s*=\s*\(?.*=>\s*true|TrustServerCertificate\s*=\s*true",
      re.I),
    c("log-request-object", "SEC-LOG-02", "destructured request object in logs may contain personal data", CS,
      r"\{@Request\}"),
    c("console-write", "SEC-LOG-01", "use ILogger, not Console", r"^Education Portal Full Stack App/src/.*\.cs$",
      r"\bConsole\s*\.\s*Write(Line)?\s*\("),
    c("unsafe-deserialization", "SEC-INPUT-06", "polymorphic type handling from input", CS,
      r"TypeNameHandling\s*\.\s*(All|Auto|Objects|Arrays)|\bBinaryFormatter\b"),
    c("angular-bypass-sanitizer", "SEC-INPUT-03", "Angular sanitizer bypass", NG, r"\bbypassSecurityTrust\w*\s*\("),
    c("angular-raw-html", "SEC-INPUT-03", "raw HTML or dynamic code execution", NG,
      r"\[innerHTML\]|\.(innerHTML|outerHTML)\s*=|insertAdjacentHTML\s*\(|\beval\s*\(|\bnew\s+Function\s*\("),
    c("package-version-in-csproj", "SEC-DEP-01", "package versions belong in Directory.Packages.props", r"\.csproj$",
      r"<PackageReference\b[^>]*\bVersion\s*="),
    c("nuget-audit-suppressed", "SEC-DEP-02", "NuGet vulnerability warnings must not be suppressed",
      r"\.(csproj|props|targets)$", r"NU190[1-4]"),
    c("secret-in-config", "SEC-SECRET-01", "credential in a config file; use user-secrets or Key Vault",
      r"(appsettings[^/]*\.json|environment[^/]*\.ts|launchSettings\.json|\.env|web\.config)$",
      r"(?i)\b(password|pwd|secret|api[_-]?key|client[_-]?secret|access[_-]?key)\b\"?\s*[=:]\s*\"?(?!\s*[\"'{$<]|\s*$)[^\"'\s;]{3,}"),
    c("private-key", "SEC-SECRET-01", "private key committed", r".*",
      r"-----BEGIN (RSA |EC |DSA |OPENSSH |ENCRYPTED )?PRIVATE KEY-----"),
    c("card-data-field", "SEC-PCI-02", "card data must never reach our code; use the processor's hosted page and tokens",
      r"\.(cs|ts|html)$",
      r"(?i)\b(credit_?card_?number|card_?number|primary_?account_?number|cvv2?|cvc2?|card_?verification\w*|card_?security_?code|track_?[12]_?data|track_?data|mag_?stripe)\b"),
    c("certification-claim", "SEC-AI-04", "certification or compliance claim; use 'designed to support ...'",
      r"\.(md|html|ts|cs|json)$",
      r"(?i)\b(is|are|we're|we are|fully|officially)\s+(fedramp|soc ?2|iso(/iec)? ?27001|govramp|stateramp|gdpr|ferpa|coppa|pci( ?dss)?)[ -]?(authori[sz]ed|certified|compliant|accredited)\b"),
    # Engineering rules (docs/ai/engineering/), checked the same way.
    c("empty-catch", "ENG-CODE-05", "empty catch swallows the error; handle it, or add context and rethrow", r"\.(cs|ts)$",
      r"\bcatch\s*(\([^)]*\))?\s*\{\s*\}"),
    c("sync-over-async", "ENG-EF-06", "blocking on async code; await it and pass the CancellationToken", SRC_CS,
      r"\.(Result|Wait\(\s*\))(?![\w(])|\.GetAwaiter\(\)\s*\.\s*GetResult\(\)"),
    c("datetime-now", "ENG-ENTITY-08", "use an injected TimeProvider and DateTimeOffset (UTC)", SRC_CS,
      r"\bDateTime(Offset)?\s*\.\s*(Utc)?Now\b"),
    c("enum-stored-as-string", "ENG-ENUM-03", "store enums as their number with a lookup table, not as strings", SRC_CS,
      r"HasConversion<string>\(\)|\bEnumToStringConverter\b"),
    c("int-route", "ENG-ID-02", "routes take the Guid public id, never the internal int key", SRC_CS,
      r"\{\w*[iI]d:int\}"),
    c("angular-ngmodule", "ENG-UI-01", "use standalone components, not NgModules", CLIENT + r".*\.ts$",
      r"@NgModule\s*\(|\bstandalone\s*:\s*false\b"),
    c("literal-validation-message", "ENG-I18N-22", "validation messages come from ValidationMessages (resx), read lazily: WithMessage(_ => ValidationMessages.X)",
      SRC_CS, r"\.WithMessage\(\s*\$?\""),
    c("localize-without-id", "ENG-I18N-32", "give every $localize message a custom id: $localize`:@@area.key:Text`",
      CLIENT + r".*\.ts$", r"\$localize`(?:(?![:\s`])|:(?![^`:]*@@))"),
    c("i18n-without-id", "ENG-I18N-32", "give every i18n / i18n-* marker a custom id: i18n=\"@@area.key\"",
      CLIENT + r".*\.(html|ts)$", r"\bi18n(-[\w-]+)?=\"(?![^\"]*@@)|<[\w-]+[^<>]*\si18n(-[\w-]+)?(?=[\s>])"),
    c("angular-date-pipe", "ENG-I18N-34", "show dates with the localDate pipe (user's time zone and page culture), not Angular's date pipe",
      CLIENT + r".*\.(html|ts)$", r"\|\s*date\b"),
    c("physical-css-direction", "ENG-I18N-36", "use logical CSS (margin-inline-start, inset-inline-end, text-align: start) so right-to-left layouts mirror",
      CLIENT + r".*\.(scss|css|ts)$",
      r"\b(margin|padding|border)-(left|right)\s*:|^\s*(left|right)\s*:|\btext-align\s*:\s*(left|right)\b|\bfloat\s*:\s*(left|right)\b"),
    c("angular-legacy-control-flow", "ENG-UI-03", "use @if / @for / @switch, not *ngIf / *ngFor / *ngSwitch",
      CLIENT + r".*\.(html|ts)$", r"\*ng(If|For|Switch\w*)\b"),
]


ENUM_FILES = re.compile(SRC_CS)
ENUM_CHECK = Check("enum-implicit-value", "ENG-ENUM-02", "give every enum member an explicit number (Name = 1)",
                   ENUM_FILES, re.compile(""))
CHECKS.append(ENUM_CHECK)
ENUM_START = re.compile(r"\benum\s+\w+(\s*:\s*\w+)?\s*$|\benum\s+\w+(\s*:\s*\w+)?\s*\{")
ENUM_MEMBER = re.compile(r"^\s*([A-Za-z_]\w*)\s*(=\s*[^,]+)?,?\s*(//.*)?$")


def implicit_enum_members(lines: list[str]) -> list[tuple[int, str]]:
    """(line number, line) of each enum member declared without an explicit value (ENG-ENUM-02)."""
    found, inside, depth = [], False, 0
    for i, line in enumerate(lines, start=1):
        stripped = line.strip()
        if not inside:
            if ENUM_START.search(line):
                inside, depth = True, line.count("{") - line.count("}")
            continue
        depth += line.count("{") - line.count("}")
        if depth <= 0 and "}" in line:
            inside = False
            continue
        if not stripped or stripped.startswith(("//", "///", "[", "{", "/*", "*")):
            continue
        member = ENUM_MEMBER.match(line)
        if member and member.group(2) is None:
            found.append((i, line))
    return found


def iter_files(args: list[str]) -> list[Path]:
    if "--staged" in args:
        out = subprocess.run(["git", "diff", "--cached", "--name-only", "--diff-filter=ACMR", "-z"],
                             cwd=ROOT, capture_output=True, check=True).stdout.decode()
        return [ROOT / p for p in out.split("\0") if p]
    explicit = [a for a in args if not a.startswith("--")]
    if explicit:
        return [Path(a).resolve() for a in explicit]
    out = subprocess.run(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard"],
                         cwd=ROOT, capture_output=True, check=True).stdout.decode()
    return [ROOT / p for p in out.split("\0") if p]


def scannable(rel: str, path: Path) -> bool:
    if rel.startswith(SKIP_PREFIXES) or any(part in SKIP_DIRS for part in Path(rel).parts):
        return False
    name = path.name
    return path.suffix.lower() in TEXT_EXT or name.startswith(".env") or name.endswith(".mdc")


def main() -> int:
    args = sys.argv[1:]
    baseline: dict[str, dict[str, int]] = json.loads(BASELINE.read_text(encoding="utf-8")).get("allowed", {})
    counts: dict[tuple[str, str], list[str]] = {}

    for path in iter_files(args):
        if not path.is_file():
            continue
        rel = path.relative_to(ROOT).as_posix()
        if not scannable(rel, path):
            continue
        try:
            lines = path.read_text(encoding="utf-8").splitlines()
        except (UnicodeDecodeError, OSError):
            continue
        for check in CHECKS:
            if check is ENUM_CHECK or not check.files.search(rel):
                continue
            for i, line in enumerate(lines):
                if not check.pattern.search(line):
                    continue
                if EXCEPTION.search(line) or (i > 0 and EXCEPTION.search(lines[i - 1])):
                    continue
                counts.setdefault((check.id, rel), []).append(f"{rel}:{i + 1}: {line.strip()[:160]}")

        if ENUM_FILES.search(rel):
            for line_no, line in implicit_enum_members(lines):
                if EXCEPTION.search(line) or (line_no > 1 and EXCEPTION.search(lines[line_no - 2])):
                    continue
                counts.setdefault((ENUM_CHECK.id, rel), []).append(f"{rel}:{line_no}: {line.strip()[:160]}")

    failures, shrunk = [], []
    by_id = {ch.id: ch for ch in CHECKS}
    for (cid, rel), hits in sorted(counts.items()):
        allowed = baseline.get(cid, {}).get(rel, 0)
        if len(hits) > allowed:
            ch = by_id[cid]
            for h in hits:
                failures.append(f"[{ch.rule}] {ch.message} ({cid})\n    {h}")
    if "--staged" not in args and not [a for a in args if not a.startswith("--")]:
        for cid, files in baseline.items():
            for rel, allowed in files.items():
                found = len(counts.get((cid, rel), []))
                if found < allowed:
                    shrunk.append(f"{cid} in {rel}: baseline allows {allowed}, found {found}. Lower it in {BASELINE.name}.")

    for f in failures:
        print("FAIL " + f)
    for s in shrunk:
        print("FAIL baseline out of date: " + s)
    if failures or shrunk:
        print(f"\n{len(failures) + len(shrunk)} finding(s). See docs/ai/secure-compliance/ (SEC-*) and "
              "docs/ai/engineering/ (ENG-*) for the rules; exceptions need maintainer approval (overview section 4).")
        return 1
    print("compliance guard: OK")
    return 0


if __name__ == "__main__":
    sys.exit(main())
