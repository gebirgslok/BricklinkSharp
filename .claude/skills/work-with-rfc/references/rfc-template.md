# RFC template

Use this as the structure for both the local file and the GitHub issue body. The local file adds
YAML frontmatter; the issue body can include or omit the frontmatter (omitting is cleaner). Keep them
otherwise identical so a single edit of the local file can be pushed to the issue body verbatim.

## Local file frontmatter

```yaml
---
github_issue: XY      # required — filled in right after the issue is created; the mapping
                       # work-with-rfc depends on to resolve local file ↔ issue
status: draft         # draft | in-progress | completed | superseded
created: YYYY-MM-DD
---
```

Title lives only in the H1. The branch is always derivable as `feat/rfc-<XY>-<slug>`.

## Body structure

```markdown
# RFC NNNN: <Title>

## Summary
One paragraph: what this changes and why. Written so a reviewer gets the point in 20 seconds.

## Motivation
The problem, the cost of not solving it, and the constraints that shape the solution.

## Proposal
The design. Enough detail to implement, not a novel. Diagrams/snippets where they earn their place.

## Phases
Each phase leaves the system in an **intact state** (builds, tests pass, no half-done migrations) and
is scoped to fit within a **~120k-token** execution session. Order phases so intactness always holds
(e.g. introduce-new before remove-old).

### Phase 1 — <name>
Goal: <what intact state this phase delivers>
- [ ] Task 1.1
- [ ] Task 1.2
- [ ] Task 1.3

### Phase 2 — <name>
Goal: <...>
- [ ] Task 2.1
- [ ] Task 2.2

## Risks & alternatives
What could go wrong, and what was considered and rejected.

## Out of scope
Explicitly what this RFC does not do (candidates for a future RFC).
```

## Notes on the tasklist

- GitHub renders `- [ ]` / `- [x]` under an issue as a progress tracker. Keep each task a single,
  checkable line. Completing a task = flipping `[ ]` to `[x]` **in the body**, then PATCHing the body.
- Keep phase headings stable once the issue exists — reviewers and links may reference them.
