# GitHub mechanics

The golden rule: **to update the RFC on GitHub, overwrite the issue body. Never append a comment.**
Ticking tasks, updating status, editing phases — all happen by editing the body and PATCHing it.

## Creating the issue (authoring)

The local file is canonical. Build the issue body from it (frontmatter optional), then create:

```bash
# Strip frontmatter if you don't want it in the issue, then create from a body file.
gh issue create --title "RFC NNNN: <Title>" --body-file /tmp/rfc-body.md --label rfc
# Capture the new issue number:
ISSUE=$(gh issue list --label rfc --limit 1 --json number -q '.[0].number')
```

Then write `github_issue: $ISSUE` into the local file's frontmatter and commit the local RFC.
(If you need the number deterministically, `gh issue create` prints the issue URL on stdout —
parse the trailing number from it instead of listing.)

## Updating the issue (the only correct way)

Always PATCH the body — pull current body to a file only if you want to diff; otherwise regenerate
the body from the canonical local file and push it:

```bash
# Option A — push the canonical local file's body straight to the issue:
gh issue edit XY --body-file /tmp/rfc-body.md

# Option B — fetch, edit, push (e.g. to flip checkboxes in place):
gh issue view XY --json body -q .body > /tmp/rfc-body.md
# ...edit /tmp/rfc-body.md: change "- [ ] Task 2.1" to "- [x] Task 2.1", update status...
gh issue edit XY --body-file /tmp/rfc-body.md
```

`gh issue edit --body-file` replaces (PATCHes) the entire body. That is exactly what we want.

**Do NOT do this to report progress:**
```bash
gh issue comment XY --body "Phase 2 done ✅"   # ❌ never — scatters state, hides the tasklist truth
```

## REST API fallback (no `gh`)

If the `gh` CLI isn't available, use the REST API with a token (`$GITHUB_TOKEN`). The body update is
a PATCH on the issue — same semantics:

```bash
# Update (PATCH the body):
curl -sS -X PATCH \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/OWNER/REPO/issues/XY \
  -d "$(jq -Rs '{body: .}' < /tmp/rfc-body.md)"

# Create:
curl -sS -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/OWNER/REPO/issues \
  -d "$(jq -Rs --arg t 'RFC NNNN: Title' '{title:$t, body:., labels:["rfc"]}' < /tmp/rfc-body.md)"
```

`jq -Rs` safely JSON-encodes the markdown (handles quotes/newlines). Resolve OWNER/REPO from the
git remote if not known.

## Tickboxes

GitHub task progress is purely the `- [ ]`/`- [x]` lines in the body. There is no separate "tick" API
for free-form tasklists — editing the body is the mechanism. This is why the patch-the-body rule and
the tickbox requirement are the same action.
