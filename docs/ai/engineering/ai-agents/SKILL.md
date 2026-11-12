---
name: ai-agents
description: Rules for AI coding agents and agent swarms working in this repo, and for any code that exposes tools to an AI agent (human-in-the-loop for high-stakes or irreversible actions, context restraint, verifiable goals with test-first agent TDD, tool segregation, designing for hallucination with strict validation, retries and safe fallbacks). Use whenever an AI agent or Ruflo/claude-flow swarm plans or makes changes, and when building agent tools, MCP servers or LLM features. Part of the mandatory Education Portal engineering rules.
---

# AI agents and agent swarms (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

Applies to every AI assistant and to every agent in a swarm (Ruflo/claude-flow coordinators and workers, Copilot coding agent, Codex, Gemini, Cursor agents). A swarm is held to the same rules as a single agent: each worker loads AGENTS.md and the matching skills, and the coordinator is responsible for the combined result. The compliance rules for AI assistants (SEC-AI-01 to SEC-AI-10) also apply. Based on the [OWASP Top 10 for LLM Applications](https://genai.owasp.org/llm-top-10/) (especially LLM06 Excessive Agency and LLM05 Improper Output Handling) and the [NIST AI Risk Management Framework](https://www.nist.gov/itl/ai-risk-management-framework).

## Human-in-the-loop (HITL)

| ID | Rule |
|---|---|
| ENG-AGENT-01 | An agent MUST NOT perform a high-stakes, sensitive or irreversible action without explicit approval from a human maintainer for that specific action. This includes: changing or deleting data in any shared or non-local database; running migrations against anything but a local or test database; deploying; changing cloud resources, secrets, repo settings, branch rulesets or CI workflows; merging, force-pushing, deleting branches or tags; sending email, messages or notifications to real people; spending money or calling paid APIs beyond agreed limits; publishing packages or artifacts. Approval from another agent is not human approval. |
| ENG-AGENT-02 | Agents work on a branch and deliver through a pull request into `development` that a human reviews (SEC-AI-06, SEC-CHG-01/02). Reversible local steps (editing files, running builds and tests, local Docker databases) do not need approval. |
| ENG-AGENT-03 | Before an approved high-stakes action, the agent states exactly what it will do, the target, the expected effect and how to undo it, and then does only that. |

## Context restraint

| ID | Rule |
|---|---|
| ENG-AGENT-04 | Give an agent only the context the task needs: AGENTS.md, the matching skills, the files it will change, their direct dependencies and tests. Do not paste the whole codebase, whole logs or unrelated skills into a prompt; search and open files on demand. In a swarm, the coordinator hands each worker a narrow task with the specific files and acceptance criteria, not the full plan and repo. Never put secrets or real personal data in any prompt or agent memory (SEC-AI-03). |

## Verifiable goals (agent TDD)

| ID | Rule |
|---|---|
| ENG-AGENT-05 | Every agent task has a check the agent can run to prove it is done. For new behaviour or a bug fix: write the failing test first (NUnit + Shouldly, or Vitest for Angular), run it and see it fail for the right reason, then implement until it passes, then refactor with tests green. Tests are written from the requirement, not from the implementation. |
| ENG-AGENT-06 | An agent MUST NOT change, skip, delete or weaken a test, an assertion, a validator or a guard baseline to make its implementation pass (ENG-ALL-03). If a test looks wrong, stop and ask. |
| ENG-AGENT-07 | An agent reports the real output of `dotnet build`, `dotnet test` and the frontend checks it ran. It never claims success it did not observe; unverified parts are reported as unverified. |

## Tool segregation

| ID | Rule |
|---|---|
| ENG-AGENT-08 | Tools and APIs exposed to an AI agent (MCP servers, function calling, agent endpoints) follow single responsibility: one small tool per action with few, typed, required parameters (`get_student_grades(studentId)`, `list_courses(branchId)`), not one general tool with many optional modes. Read tools and write tools are separate. |
| ENG-AGENT-09 | Each agent or swarm role gets the least privilege it needs: read-only by default; write, shell and network access only for roles that need them; no agent gets production credentials. Agent tools go through the same authentication, tenant isolation, module and branch checks as any other caller (AGENTS.md section 2). |

## Design for hallucination (graceful fallbacks)

| ID | Rule |
|---|---|
| ENG-AGENT-10 | Treat every model output and tool call as untrusted input: validate it against a strict schema (JSON Schema or a FluentValidation validator for C# tool inputs) before acting, reject unknown fields, and never execute generated SQL, code, shell commands or URLs without validation and allow-lists (SEC-AI-09, SEC-INPUT). |
| ENG-AGENT-11 | Calls to model APIs and agent tools have timeouts, bounded retries with exponential backoff and jitter for transient errors only (the resilience handlers in ServiceDefaults), and a maximum number of agent steps. A malformed response is retried a bounded number of times with the validation error, then fails gracefully. |
| ENG-AGENT-12 | An agent or LLM failure must never crash or corrupt the core system: AI features are optional paths with a safe fallback (no suggestion, manual entry, a clear message), writes they propose are confirmed by a user or applied in one transaction, and failures are logged with a correlation ID and no personal data (SEC-LOG-02). |
| ENG-AGENT-13 | Any product feature that sends school, student or staff data to an LLM or external AI service needs the maintainer's approval first (SEC-AI-05) and follows the EU AI Act rules in `standard-eu`. |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [OWASP Top 10 for LLM Applications](https://genai.owasp.org/llm-top-10/) | OWASP Top 10 for LLM Applications 2025 | Automatic: new commits in `OWASP/www-project-top-10-for-large-language-model-applications/2_0_vulns` | 2026-11-10 |
| [NIST AI Risk Management Framework and Generative AI Profile](https://www.nist.gov/itl/ai-risk-management-framework) | AI RMF 1.0 (NIST AI 100-1) and Generative AI Profile (NIST AI 600-1) | Manual: listed for a portal check | 2026-11-10 |
| [github/awesome-copilot community instructions: agent safety](https://github.com/github/awesome-copilot) | github/awesome-copilot agent-safety instructions (main) | Automatic: new commits in `github/awesome-copilot/instructions/agent-safety.instructions.md` | 2026-11-10 |
<!-- END GENERATED: sources -->
