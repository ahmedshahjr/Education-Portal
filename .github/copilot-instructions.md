# Copilot instructions

All project instructions live in [AGENTS.md](../AGENTS.md) at the repository root. Read it before making changes, and edit it (not this file) when decisions change.

**MUST FOLLOW:** the security and compliance rules in [docs/ai/secure-compliance/SKILL.md](../docs/ai/secure-compliance/SKILL.md) (FedRAMP Moderate Rev 5 / NIST SP 800-53, GovRAMP, SOC 2, ISO 27001, OWASP Top 10, PCI DSS, EU law). Read the overview, then each topic and standard skill it lists that matches your change, before changing anything. Each skill is also attached as an instructions file in `.github/instructions/`. If a request breaks a rule, stop and name the rule ID.

**MUST FOLLOW:** the engineering rules in [docs/ai/engineering/SKILL.md](../docs/ai/engineering/SKILL.md) (SOLID/KISS/DRY/YAGNI, code quality and process, C# entities, EF Core, Angular with Material Design, AI agents). Each topic is attached as an `engineering-*` instructions file in `.github/instructions/`. Security and compliance rules win when the two disagree.

Key rules, in short (details in AGENTS.md):

- .NET 10 Clean Architecture solution hosted by Aspire, Angular frontend, PostgreSQL only. Not microservices.
- Multi-tenant: master tenant > school > branch. One shared database; every school-owned row has `SchoolId` (and `BranchId` if branch-scoped), enforced by EF Core global query filters. Master users bypass them.
- Access = modules the master enabled for the school ∩ role rights per module (View/Create/Edit/Delete) ∩ branches assigned to the user. Check this on the server, not only in Angular.
- Dependency rule: Domain <- Application <- Infrastructure/Web. Package versions only in `Directory.Packages.props`. Warnings are errors.
- Don't invent answers for items marked TODO in AGENTS.md.
