# GEMINI.md

All project instructions live in [AGENTS.md](AGENTS.md), imported below. Edit AGENTS.md (not this file) when decisions change.

@./AGENTS.md

**MUST FOLLOW:** the security and compliance rules in [docs/ai/secure-compliance/SKILL.md](docs/ai/secure-compliance/SKILL.md) (FedRAMP Moderate Rev 5 / NIST SP 800-53, GovRAMP, SOC 2, ISO 27001, OWASP Top 10, PCI DSS, EU law). They are imported below so they are always in context. If a request breaks a rule, stop and name the rule ID.

**MUST FOLLOW:** the engineering rules in [docs/ai/engineering/SKILL.md](docs/ai/engineering/SKILL.md) (SOLID/KISS/DRY/YAGNI, code quality and process, C# entities, EF Core, Angular with Material Design, AI agents). They are imported below too. Security and compliance rules win when the two disagree.

<!-- BEGIN GENERATED: ai skills (docs/ai/sync-ai-wrappers.py) -->
Gemini CLI imports every skill below into context at start, so all compliance and engineering rules are always loaded:

@./docs/ai/secure-compliance/SKILL.md
@./docs/ai/secure-compliance/authentication/SKILL.md
@./docs/ai/secure-compliance/authorization-tenant-isolation/SKILL.md
@./docs/ai/secure-compliance/change-management/SKILL.md
@./docs/ai/secure-compliance/configuration-environments/SKILL.md
@./docs/ai/secure-compliance/cryptography-secrets/SKILL.md
@./docs/ai/secure-compliance/dependencies-supply-chain/SKILL.md
@./docs/ai/secure-compliance/error-handling/SKILL.md
@./docs/ai/secure-compliance/input-validation/SKILL.md
@./docs/ai/secure-compliance/logging-audit/SKILL.md
@./docs/ai/secure-compliance/security-testing/SKILL.md
@./docs/ai/secure-compliance/standard-eu/SKILL.md
@./docs/ai/secure-compliance/standard-fedramp-moderate/SKILL.md
@./docs/ai/secure-compliance/standard-govramp/SKILL.md
@./docs/ai/secure-compliance/standard-iso27001/SKILL.md
@./docs/ai/secure-compliance/standard-owasp-top10/SKILL.md
@./docs/ai/secure-compliance/standard-pci-dss/SKILL.md
@./docs/ai/secure-compliance/standard-soc2/SKILL.md
@./docs/ai/secure-compliance/student-data-privacy/SKILL.md
@./docs/ai/secure-compliance/web-api-hardening/SKILL.md
@./docs/ai/engineering/SKILL.md
@./docs/ai/engineering/ai-agents/SKILL.md
@./docs/ai/engineering/angular-material-ui/SKILL.md
@./docs/ai/engineering/code-quality-process/SKILL.md
@./docs/ai/engineering/csharp-entities/SKILL.md
@./docs/ai/engineering/design-principles/SKILL.md
@./docs/ai/engineering/efcore/SKILL.md
@./docs/ai/engineering/identifiers-enums/SKILL.md
@./docs/ai/engineering/localization-i18n/SKILL.md
<!-- END GENERATED: ai skills -->

Key rules, in short (details in AGENTS.md):

- .NET 10 Clean Architecture solution hosted by Aspire, Angular frontend, PostgreSQL only. Not microservices.
- Multi-tenant: master tenant > school > branch. One shared database; every school-owned row has `SchoolId` (and `BranchId` if branch-scoped), enforced by EF Core global query filters. Master users bypass them.
- Access = modules the master enabled for the school ∩ role rights per module (View/Create/Edit/Delete) ∩ branches assigned to the user. Check this on the server, not only in Angular.
- Dependency rule: Domain <- Application <- Infrastructure/Web. Package versions only in `Directory.Packages.props`. Warnings are errors.
- Don't invent answers for items marked TODO in AGENTS.md.
