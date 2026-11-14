# CLAUDE.md

All project instructions live in [AGENTS.md](AGENTS.md), imported below. Edit AGENTS.md, not this file.

@AGENTS.md

**MUST FOLLOW:** the security and compliance rules in [docs/ai/secure-compliance/SKILL.md](docs/ai/secure-compliance/SKILL.md). Claude Code also loads the overview as the `secure-compliance` skill and each topic and standard as a `compliance-*` skill from `.claude/skills/` (generated pointers; the same skills reach Gemini, Copilot, Cursor and Codex, see AGENTS.md section 7).

**MUST FOLLOW:** the engineering rules in [docs/ai/engineering/SKILL.md](docs/ai/engineering/SKILL.md) (SOLID/KISS/DRY/YAGNI, code quality and process, C# entities, EF Core, Angular with Material Design, AI agents and swarms). Claude Code loads the overview as the `engineering-standards` skill and each topic as an `engineering-*` skill from `.claude/skills/`. Ruflo/claude-flow agents read this file too and are bound by the same rules.
