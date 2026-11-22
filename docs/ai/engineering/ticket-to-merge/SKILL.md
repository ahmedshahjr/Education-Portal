---
name: ticket-to-merge
description: The end-to-end delivery pipeline for one ticket (load the rules, plan, branch from development, layered build, tests including a Playwright whole-flow scenario, local gates, pull request with the review checklist, separate AI review, merge gate, user documentation, report, stop conditions). Use when an AI agent or swarm takes a ticket from start to merged, or when any contributor wants the full sequence and its gates. Part of the mandatory Education Portal engineering rules.
---

# Ticket to merge (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. Security and compliance rules win when the two disagree.

One ticket is one pipeline run and one pull request into `development`. The steps run in this order. A step that fails sends the run back to the step that caused it, never forward.

## Pipeline

| ID | Rule |
|---|---|
| ENG-FLOW-01 | **Load the rules first.** Before any code: read `AGENTS.md`, the [compliance overview](../../secure-compliance/SKILL.md) and the [engineering overview](../SKILL.md), then every topic skill that matches the ticket. Where Ruflo is installed, run `memory_search` (namespace `education-portal`) first. Do not invent requirements or anything marked TODO: ask on the ticket (ENG-ALL-05). If the ticket breaks a rule, stop and name it. |
| ENG-FLOW-02 | **Plan before code.** Restate the acceptance criteria as numbered, testable statements. List the layers touched, the `ModuleCode`, the role rights (View, Create, Edit, Delete, Approve, Export) and the campus rules. List the denial tests that will prove it (another school, another campus, module off, missing right). Decide whether an ADR is needed (ENG-PROC-07). Post the plan on the ticket. |
| ENG-FLOW-03 | **Branch from the latest `origin/development`**: `feature/<ticket-id>-<short-slug>`. Never commit to `development`, `staging` or `main`. A pushed branch is never rebased or force-pushed; bring it up to date with `git merge origin/development` (AGENTS.md section 7, rule 10). |
| ENG-FLOW-04 | **Build in layers, test first.** Domain, then the Application use case (request, validator, `...Handler`, ADR-025), then Infrastructure (EF configuration, schema mapping, enum lookup tables, a generated migration), then the Web controller, then Angular (including the client mirror of every server validation and the translations), then tests. Each use case starts with a failing test (ENG-AGENT-05). Changes are surgical (ENG-CODE-07). |
| ENG-FLOW-05 | **Tests follow the [testing skill](../testing/SKILL.md)**: unit, functional with the denial cases, Angular tests, and one Playwright + Reqnroll scenario for the whole flow in the ticket. A flow that crosses the API is not done without it (ENG-UI-15). |
| ENG-FLOW-06 | **Local gates are all green before the pull request**, and the real results are reported (ENG-ALL-04): `dotnet build`, `dotnet test`, the Angular build and tests, `ng extract-i18n`, `docs/ai/readme-guard.py`, `docs/ai/architecture-diagram-guard.py`, `scripts/update-classes-erd.py --check`, and `docs/ai/sync-ai-wrappers.py --check` when a skill changed. The compliance pre-commit hook stays on. Never `--no-verify`, never lower a baseline (ENG-ALL-03). README and diagrams change in the same PR when the guards say so. |
| ENG-FLOW-07 | **One pull request, honest checklist.** Target `development`. The body has the summary, "What and why", each acceptance criterion with the test that proves it, the security and engineering checklists, a `README:` line, a `Diagrams:` line and the Review checklist `REVIEW-01` to `REVIEW-06`. A box is ticked only after the question was asked of the code (AGENTS.md section 7, rule 9). Commit messages name the ticket. |
| ENG-FLOW-08 | **A different reviewer reviews.** The author never reviews its own change. The reviewer starts with fresh context, reads the diff and the tests (not the description), answers the six review questions, and checks the compliance skills that match the change (tenant isolation, authorization, personal data, input validation, logging). Findings are comments marked blocking or nit and cite rule IDs (ENG-PROC-06). The author fixes, pushes and re-runs the gates. After 3 rounds without agreement, stop and ask a human. |
| ENG-FLOW-09 | **Merge gate.** Merge only when CI is green, no blocking comment is open, every review box is ticked and no rule was weakened. The default is `MERGE_MODE=human`: the maintainer approves and merges. `MERGE_MODE=auto` (squash, delete branch) is allowed only where the maintainer has set it for the repository; it never uses an admin override, never bypasses branch protection and never targets `main` or `staging`. |
| ENG-FLOW-10 | **Documentation follows the merge.** The [user-documentation skill](../user-documentation/SKILL.md) is run on a new branch from the updated `development` and goes through ENG-FLOW-07 to ENG-FLOW-09 as its own pull request. |
| ENG-FLOW-11 | **Report and remember.** Comment on the ticket with the pull request links, test counts, gates passed, what was not verified and open questions. Where Ruflo is installed, store decisions, non-obvious causes and conventions the maintainer corrected with `memory_store`: never secrets, tokens or personal data, and never anything the repo already records (AGENTS.md section 7, rule 11). |

## Agent roles

Use a small team and give each agent one job (ENG-AGENT tool segregation). Names are the agent types available in Claude Code; other tools use their own equivalents.

| Step | Role | Typical agent |
|---|---|---|
| Plan | Specification and design | `specification`, `code-architect` |
| Build | Implementation | `coder` (C# and Angular), `database-reviewer` for entities and migrations |
| Tests | Test writing and whole-flow scenario | `tdd-london-swarm` or `dotnet-test:code-testing-implementer`, `e2e-runner` |
| Review | Independent review | `code-reviewer`, `csharp-reviewer`, `typescript-reviewer`, `security-reviewer` |
| Docs | User documentation | `doc-updater` |

## Stop and ask

Stop, say what is blocked and wait for the maintainer when:

| ID | Situation |
|---|---|
| ENG-FLOW-12 | The ticket is ambiguous about a rule, a permission, money or personal data, or touches a TODO in AGENTS.md. |
| ENG-FLOW-13 | A gate fails for a reason outside the ticket. Report it; do not skip or weaken the gate. |
| ENG-FLOW-14 | The change would need a compliance or engineering rule relaxed. Only the maintainer grants an exception. |
| ENG-FLOW-15 | Merge mode is `human`, or the review loop passed 3 rounds. |
| ENG-FLOW-16 | The step would be hard to reverse or visible outside the repository (a push to a shared branch, a merge, a release) and the maintainer has not already authorised it for this run. |

Never claim the product is certified. Never print or commit secrets or tokens.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [GitHub flow (branch, pull request, review, merge)](https://docs.github.com/get-started/using-github/github-flow) | GitHub docs as read on 2026-11-22 | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
