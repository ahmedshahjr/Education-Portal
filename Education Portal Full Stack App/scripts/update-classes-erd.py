#!/usr/bin/env python3
"""Keeps src/Domain/ClassesErdDiagram.cd in step with the public types in src/Domain.

The .cd file is a Visual Studio class diagram. Visual Studio refreshes the members of the types that are already on it,
but it never adds a new type or drops a deleted one, so this script does:

  * a type that no longer exists in src/Domain is removed from the diagram;
  * a public type that is missing from the diagram is added (collapsed, in a free spot under the existing ones);
  * the file name of a type that moved is corrected.

Types that are already on the diagram keep their position, size and collapse state, so a diagram that was laid out by
hand stays as it was. Visual Studio recomputes the hash codes when the diagram is opened.

  python3 scripts/update-classes-erd.py          # update the file (the pre-commit hook does this and stages it)
  python3 scripts/update-classes-erd.py --check  # exit 1 when the file is out of date (CI)

Run from anywhere; paths are found from the location of this script.
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
DOMAIN = ROOT / "src" / "Domain"
DIAGRAM = DOMAIN / "ClassesErdDiagram.cd"

KINDS = {"class": "Class", "record": "Class", "struct": "Struct", "interface": "Interface", "enum": "Enum"}
EMPTY_HASH = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="

NAMESPACE = re.compile(r"^\s*namespace\s+([\w.]+)\s*[;{]", re.M)
DECLARATION = re.compile(
    r"^[ \t]*public\s+(?:(?:static|sealed|abstract|partial|readonly|unsafe)\s+)*(class|record|struct|interface|enum)\s+(?:class\s+|struct\s+)?(\w+)",
    re.M,
)
ELEMENT = re.compile(r'^  <(Class|Enum|Interface|Struct) Name="([^"]+)"[^>]*?(?:/>|>.*?^  </\1>)\n', re.M | re.S)
FILE_NAME = re.compile(r"<FileName>[^<]*</FileName>")


def domain_types() -> dict[str, tuple[str, str]]:
    """Full type name -> (diagram element, file name relative to src/Domain) for every public top-level type."""
    found: dict[str, tuple[str, str]] = {}
    for path in sorted(DOMAIN.rglob("*.cs")):
        relative = path.relative_to(DOMAIN)
        if relative.parts[0] in ("obj", "bin"):
            continue
        text = path.read_text(encoding="utf-8-sig")
        namespace = NAMESPACE.search(text)
        if namespace is None:
            continue
        for declaration in DECLARATION.finditer(text):
            kind, name = declaration.groups()
            found[f"{namespace.group(1)}.{name}"] = (KINDS[kind], str(relative).replace("/", "\\"))
    return found


def new_element(kind: str, name: str, file_name: str, x: float, y: float) -> str:
    return (
        f'  <{kind} Name="{name}" Collapsed="true">\n'
        f'    <Position X="{x:g}" Y="{y:g}" Width="1.75" />\n'
        f"    <TypeIdentifier>\n"
        f"      <HashCode>{EMPTY_HASH}</HashCode>\n"
        f"      <FileName>{file_name}</FileName>\n"
        f"    </TypeIdentifier>\n"
        f"  </{kind}>\n"
    )


def update(text: str, types: dict[str, tuple[str, str]]) -> str:
    on_diagram: dict[str, str] = {}
    for match in ELEMENT.finditer(text):
        on_diagram[match.group(2)] = match.group(1)

    # Drop the types that no longer exist (or whose kind changed: they are re-added below as the new kind).
    def keep(match: re.Match[str]) -> str:
        name = match.group(2)
        if name not in types or types[name][0] != match.group(1):
            return ""
        block = match.group(0)
        return FILE_NAME.sub(lambda _: f"<FileName>{types[name][1]}</FileName>", block, count=1)

    text = ELEMENT.sub(keep, text)

    present = {m.group(2) for m in ELEMENT.finditer(text)}
    missing = sorted(name for name in types if name not in present)
    if not missing:
        return text

    # New types go in rows under everything that is already there.
    y_values = [float(v) for v in re.findall(r'<Position X="[\d.]+" Y="([\d.]+)"', text)]
    top = (max(y_values) + 2.5) if y_values else 0.5
    columns, step_x, step_y = 12, 2.25, 0.75
    additions = []
    for index, name in enumerate(missing):
        kind, file_name = types[name]
        additions.append(new_element(kind, name, file_name, 0.5 + (index % columns) * step_x, top + (index // columns) * step_y))

    font = re.search(r"^  <Font ", text, re.M)
    insert_at = font.start() if font else text.rindex("</ClassDiagram>")
    return text[:insert_at] + "".join(additions) + text[insert_at:]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--check", action="store_true", help="do not write; exit 1 when the diagram is out of date")
    args = parser.parse_args()

    raw = DIAGRAM.read_bytes()
    bom = raw.startswith(b"\xef\xbb\xbf")
    original = raw.decode("utf-8-sig")
    newline = "\r\n" if "\r\n" in original else "\n"
    current = original.replace("\r\n", "\n")

    updated = update(current, domain_types())

    if updated == current:
        print("ClassesErdDiagram.cd is up to date.")
        return 0

    if args.check:
        print(
            "ClassesErdDiagram.cd is out of date. Run `python3 scripts/update-classes-erd.py` "
            "(from 'Education Portal Full Stack App') and commit the result.",
            file=sys.stderr,
        )
        return 1

    DIAGRAM.write_bytes((b"\xef\xbb\xbf" if bom else b"") + updated.replace("\n", newline).encode("utf-8"))
    print("ClassesErdDiagram.cd updated.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
