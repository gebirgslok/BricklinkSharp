---
name: pr-cleanup
description: Clean up a git repo after a pull request has been merged — check out the PR's target branch and pull it, delete the local and remote feature branch, close the issues the PR resolved (ticking off their task lists after asking), and suggest leftover housekeeping such as stale branches, worktrees, stashes and changed lockfiles. Use this skill whenever the user says a PR was merged, asks to "clean up", "tidy up", "wrap up" or "finish" a branch or PR, wants to delete a merged branch, says "PR is in, back to main", or runs /pr-cleanup, with or without a PR number or URL.
---

# PR cleanup after merge

The goal is to leave the repo exactly as it would be if the feature branch had never existed: on the up-to-date target branch, no leftover branches locally or remotely, the related issues closed, and nothing obvious forgotten.

Everything here relies on `git` and the GitHub CLI `gh`. Check `gh auth status` once at the start; if `gh` is missing or not authenticated, say so and offer to do only the local git part.

Work in this order: identify the PR → verify it is merged → present a plan and get one confirmation → execute → handle issues → suggest extra cleanup → report. Gather all facts first and ask for confirmation once, rather than interrupting the user at every step. The only extra questions should be the ones the plan cannot answer up front (ambiguous PR, issues with open tasks, optional extras).

## 1. Identify the PR

If `$ARGUMENTS` contains a PR number, `#123`, or a PR URL, use it and skip the inference.

Otherwise infer it, stopping at the first source that gives exactly one answer:

1. Current branch: `gh pr view --json number,title,state,headRefName,baseRefName,url` (works when the current branch is a PR head).
2. The current branch may already be the base (the user switched back to `main` themselves). Then look at the previous branch: `git rev-parse --abbrev-ref @{-1}` and run `gh pr list --head <that-branch> --state merged --json number,title,mergedAt,headRefName,baseRefName`.
3. Earlier conversation context: a PR number or branch name mentioned in this session.
4. The user's recently merged PRs: `gh pr list --author @me --state merged --limit 5 --json number,title,mergedAt,headRefName` and keep those whose head branch still exists locally (`git branch --list <name>`).

If more than one candidate remains, or the only candidate came from step 4, ask the user which PR they mean and show the candidates (number, title, branch, merged date). Never guess between candidates, because the next steps delete branches.

## 2. Gather facts and verify the merge

Fetch everything in one call:

```bash
gh pr view <PR> --json number,title,url,state,mergedAt,mergeCommit,headRefName,headRefOid,baseRefName,isCrossRepository,headRepositoryOwner,headRepository,closingIssuesReferences,body
```

If `state` is not `MERGED`, stop. Tell the user the PR is still open (or was closed without merging) and do nothing destructive. A PR closed without merging is a judgement call: offer the branch deletion only if the user asks for it explicitly.

Then collect:

- **Working tree state**: `git status --porcelain` and `git stash list`. Uncommitted changes on the feature branch would be lost or carried over on checkout, so they must be mentioned in the plan.
- **Unmerged local work**: compare the local branch tip with the PR head, `git rev-list --count <headRefOid>..<headRefName>`. A non-zero count means there are local commits that never reached the PR; flag them prominently and do not delete that branch without explicit approval.
- **Remote branch existence**: `git ls-remote --exit-code --heads origin <headRefName>`. GitHub's "auto-delete head branches" setting often removed it already; that is normal, not an error. For fork PRs (`isCrossRepository: true`), the branch lives on the fork's remote, so find the remote whose URL matches `headRepositoryOwner/headRepository`; if none does, skip remote deletion and say why.
- **Stacked PRs**: `gh pr list --base <headRefName> --state open --json number,title`. These PRs target the branch about to be deleted. Deleting it from the command line can close them, so they must be retargeted to `baseRefName` first.
- **Worktrees**: `git worktree list --porcelain`. Git refuses to delete a branch that is checked out in another worktree.
- **Related issues** (see section 4 for how to collect them).

Squash and rebase merges create new commits on the base, so `git branch -d` will refuse to delete the branch even though its work is merged. Once the PR is confirmed merged and there are no extra local commits, `git branch -D` is the correct, safe choice — explain this rather than being surprised by the error.

## 3. Present the plan, then execute

Show a short plan, for example:

```
PR #128 "Add retry to webhook sender" — merged into main 2h ago

  1. Checkout main and pull (fast-forward)
  2. Delete local branch feat/webhook-retry
  3. Delete remote branch origin/feat/webhook-retry
  4. Close #97 (no open tasks)
  5. #101 has 2 open tasks — will ask separately
  ⚠ Uncommitted changes on feat/webhook-retry: src/foo.ts (will stash first)
```

Leave out the steps that do not apply (e.g., remote branch already deleted). After the user confirms, execute in this order, because each step depends on the previous one:

1. If there are uncommitted changes, ask whether to stash (`git stash push -u -m "pr-cleanup: <branch>"`), commit somewhere, or abort. Do not discard them.
2. `git checkout <baseRefName>` then `git pull --ff-only`. If the fast-forward fails, the local base has diverged; stop and show the user rather than merging or resetting.
3. Retarget stacked PRs: `gh api -X PATCH repos/{owner}/{repo}/pulls/<n> -f base=<baseRefName>` for each (not `gh pr edit`, which fails on repos linked to Projects classic).
4. Remove a worktree that holds the branch only if it has no changes (`git worktree remove <path>`); otherwise ask.
5. Delete the local branch (`git branch -d`, falling back to `-D` per section 2).
6. Delete the remote branch if it still exists: `git push <remote> --delete <headRefName>`.
7. `git fetch --prune` to drop the stale remote-tracking ref.

If a command fails, stop, show the error, and continue only with steps that do not depend on it.

## 4. Close the related issues

Collect candidates from two sources and keep them separate, because they mean different things:

- **Linked issues**: `closingIssuesReferences` from the PR (issues linked with "Fixes #n", "Closes #n", or via the sidebar). GitHub only auto-closes these when the PR merges into the default branch, so if `baseRefName` is not the default branch they are still open and need closing here. Often they are already closed; just report that.
- **Mentioned issues**: plain `#n` references in the PR body or commit messages that are not linked. These may be related only loosely, so ask before closing them, as a group.

For each open candidate, run the bundled helper:

```bash
python3 <skill-dir>/scripts/issue_tasks.py list <issue> [--repo owner/repo]
```

It returns the issue's open checkboxes (ignoring ones inside code blocks), each with an `index` and an `issue_ref` when the task points at another issue.

- **No open tasks** → close it: `gh issue close <n> --reason completed --comment "Resolved by #<PR>"`.
- **Open tasks** → show them to the user, quoted exactly, and ask whether to:
  - tick all and close,
  - tick only some (the user names which) and leave the issue open if any remain,
  - leave the issue untouched.

  Tick with `python3 <skill-dir>/scripts/issue_tasks.py tick <issue> --all` or `--index 1 3`. Use `--dry-run` first if the user wants to see the new body.
- **Tasks that reference another issue** (`issue_ref` is set): check that issue's state with `gh issue view <ref> --json state`. If it is closed, the task is effectively done; say so and suggest ticking it. If it is open, the parent issue is genuinely not finished, so recommend leaving it open.

Ticking a box is a claim that the work is done, which is why the user decides, never the skill on its own. Ask about all issues with open tasks in a single message where possible.

## 5. Suggest forgotten cleanup

Check for these and suggest only the ones that actually apply, each as a one-liner the user can accept or skip. Do not run any of them without a yes.

- **Other gone branches**: local branches whose upstream was deleted, from `git branch -vv | grep ': gone]'`. Typical leftovers of earlier PRs; offer to delete them, listing names.
- **Other merged PRs with local branches**: local branches whose name matches a merged PR (`gh pr list --state merged --author @me --limit 30 --json headRefName`). These do not show as "gone" when the remote branch was never pushed or still exists.
- **Stashes from the branch**: `git stash list` entries mentioning the branch name. They are probably obsolete now; offer to show or drop them.
- **Stale worktrees**: `git worktree prune --dry-run` output, and worktrees on deleted branches.
- **Dependencies changed**: compare lockfiles between the old base tip and the new one (`git diff --name-only ORIG_HEAD HEAD` after the pull) for `package-lock.json`, `pnpm-lock.yaml`, `yarn.lock`, `poetry.lock`, `uv.lock`, `Cargo.lock`, `go.sum`, `Gemfile.lock`, etc. If any changed, suggest the matching install command.
- **Migrations changed**: new files under typical migration directories (`migrations/`, `db/migrate/`, `alembic/versions/`, `prisma/migrations/`) in the same diff; suggest running them locally.
- **Untracked leftovers**: `git status --porcelain --ignored` showing build artifacts or generated files that belonged to the old branch.
- **Project board / milestone**: if the issues are on a GitHub Project or milestone that does not auto-update, mention it (`gh issue view <n> --json projectItems,milestone`).

## 6. Report

Finish with a compact summary of what was done, what was skipped and why, and anything left for the user (e.g., "#101 left open: 1 task remaining — 'update docs'"). Keep it to a few lines.
