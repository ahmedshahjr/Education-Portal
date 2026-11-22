---
name: user-documentation
description: How to write the end-user guide for a merged feature (where pages live, the page template with who, prerequisites, numbered steps, rights, common errors, a worked example and a flow recap, using the real screen labels, fake data only, languages and right-to-left notes, and a separate documentation pull request after the code merge). Use after a feature merges, or when a ticket asks for help pages on how to create users, roles, permissions, campuses or any other flow. Part of the mandatory Education Portal engineering rules.
---

# User documentation (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. This skill is step ENG-FLOW-10 of the [ticket-to-merge skill](../ticket-to-merge/SKILL.md).

The readers are school staff and the master tenant's administrators, not developers. The docs describe what the running application does, so they are written from the code and the screens, never from memory.

## Where and how

| ID | Rule |
|---|---|
| ENG-DOCS-01 | **Own pull request, after the merge.** Start a branch `docs/<ticket-id>-user-guide` from the updated `origin/development`, so the docs describe merged behaviour. It goes through the normal gates and review (ENG-FLOW-07 to ENG-FLOW-09). A docs-only change still fills the Review checklist. |
| ENG-DOCS-02 | **Location and format.** Plain Markdown in `docs/user-guide/`, one page per task (`create-a-user.md`, `create-a-role.md`, `assign-a-campus.md`...), plus `docs/user-guide/README.md` as the index in the order a new reader should follow. Create the folder when it is missing. No tool-specific format: every AI tool and every human must read the same files (AGENTS.md section 7, rule 7). |
| ENG-DOCS-03 | **Facts come from the code.** Take screen names, button text, field labels and menu paths from the Angular templates and the `en` translation file (`src/Web/ClientApp/src/locale/en/messages.json`), and rights, errors and limits from the use cases, validators and `AGENTS.md`. If the doc and the code disagree, the code wins and the doc is fixed. Run the flow once (the acceptance scenario or the dev app on `demo.localhost`) and write what actually happened. |
| ENG-DOCS-04 | **Fake data only.** Examples use invented schools, people and emails (`Example School`, `ada.example@example.test`). Never real names, student data, tokens, passwords or the Development seed password. Do not describe how to bypass a security rule. |

## Page template

Every task page has these headings in this order. Keep sentences short and in the order the user acts.

1. **What this does** and **who can do it** (the role or right needed, in plain words).
2. **Before you start**: the module that must be on for the school (and who switches it, the master tenant), the role rights needed (View, Create, Edit, Delete, Approve, Export), and the campus the user must be assigned to.
3. **Steps**: numbered, one action each, with the exact label (`Users and roles` > `Add user`), what the user types or picks, and what they see when it worked.
4. **What each right allows** when the page is about roles or permissions.
5. **If something goes wrong**: each error in plain words with the cause and the fix, for example `MODULE_NOT_ENABLED` (the school does not have this module; ask the master tenant), `PASSWORD_CHANGE_REQUIRED` (choose your own password before anything else), and a 403 for a campus the user does not belong to.
6. **Worked example** with fake data, start to finish.
7. **Flow recap**: a short box that restates the order to remember, for example *school or campus exists, then roles with their rights, then users, then campus assignment, then the user signs in and checks*. Every page ends with a link to the previous and the next page of the flow.

## Languages and layout

| ID | Rule |
|---|---|
| ENG-DOCS-05 | The guide is written in English first. When the screen has Arabic and Urdu labels, give them next to the English label in a table once on the first page that uses them. Say that Arabic and Urdu screens are laid out right to left, so left and right swap. Do not machine-translate whole pages without a note that a native review is still open (AGENTS.md section 9). |
| ENG-DOCS-06 | Screenshots are optional. When used they show fake data only, no personal data, and have a text alternative. Prefer exact text steps, which stay correct longer and can be searched. |

## Keeping it current

| ID | Rule |
|---|---|
| ENG-DOCS-07 | A change that alters a screen, a right, a limit or an error updates the matching user-guide page in the same PR from then on (definition of done, ENG-PROC-02). The ticket-to-merge pipeline only creates the page the first time. |
| ENG-DOCS-08 | The index (`docs/user-guide/README.md`) lists every page with a one-line purpose, and the README of the repository links to it once when the guide first exists (the README stays the product overview, AGENTS.md section 7, rule 8). |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Diataxis documentation framework (tutorials, how-to guides, reference, explanation)](https://diataxis.fr/) | Diataxis as read on 2026-11-22 | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
