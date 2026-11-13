---
name: work-with-rfc
description: >-
  Author and execute RFCs (design proposals) tracked both locally in the repo and as a GitHub
  issue. Use when the user wants to write an RFC, turn a design session into a phased proposal,
  or execute a phase of an existing RFC. Triggers on `/work-with-rfc #XY phase Z` (XY = GitHub
  issue number, Z = phase) and on phrases like "write an RFC", "work on phase N of the RFC",
  "advance the RFC" or "tick off the RFC tasks". Keeps the GitHub issue in sync by PATCHING its
  body, never by adding comments.
---

# Work with RFC

Manages an RFC's lifecycle: **authoring** it (local file + GitHub issue) and **executing** it one
phase at a time.

## Model

- **Local file** `docs/rfcs/NNNN-slug.md` is the source of truth. Its frontmatter holds `github_issue: XY`.
- **GitHub issue body** mirrors the file and carries the `- [ ]` / `- [x]` tasklist.

"Update the RFC" always means: edit the local file, then overwrite the issue body from it
(`references/github-mechanics.md`). **Never report progress as a comment.**

If the command names an issue and phase, execute. If there is no issue yet, author.

---

## Authoring

1. **Gather the substance** from the discussion at hand. If scope is fuzzy, run `/grill-me` first.
2. **Write `docs/rfcs/NNNN-slug.md`** from `references/rfc-template.md` (next free number). Each phase must:
   - **Leave the repo intact**: it builds and tests pass, with no half-done migrations. Order phases
     accordingly (add new before removing old).
   - **Fit in ~120k tokens** of work. Split it if it looks bigger.
   Break each phase into checkable tasks (`- [ ]`).
3. **Create the issue**, then write its number into the file's `github_issue` frontmatter and commit.
   Commands are in `references/github-mechanics.md`.

---

## Executing a phase

`/work-with-rfc #XY phase Z`: find the local RFC by its `github_issue` frontmatter (or rebuild it
from the issue body if missing).

**Before:** check out (or create from the default branch) the single RFC branch `feat/rfc-<XY>-<slug>`,
and re-read phase Z's tasks. Do exactly that scope. If the phase has outgrown the cap, propose splitting.

**Do the work** for phase Z's tasks.

**After, in order:**

1. **Review.** Dispatch the relevant agents from `.claude/agents/` against the phase's changes (in
   parallel). Triage findings by blast radius:
   - *Small*: fix directly.
   - *Large* (cross-cutting or design-affecting): ask the user, optionally via `/grill-me`.
   - *Very large* (re-architecture): propose a new RFC rather than bolting it on.
2. **Format and check.** Run `dotnet format`, `dotnet build`, and `dotnet test`. Fix what they
   surface. Do not continue while anything is red.
3. **Update the RFC.** Tick phase Z's tasks and the status in the local file, then PATCH the issue body.
4. **Commit** all phase changes, including the RFC file, as `feat(rfc-XY): complete phase Z — <summary>`.
   Push if the repo workflow expects it.

When the last phase is done, mention the issue can be closed. Don't auto-close mid-RFC.

## Reference files

- `references/rfc-template.md`: structure for the local RFC and issue body.
- `references/github-mechanics.md`: `gh` / REST commands to create the issue and PATCH its body.
