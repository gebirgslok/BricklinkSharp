#!/usr/bin/env python3
"""List or tick Markdown task-list checkboxes in a GitHub issue body.

Usage:
  issue_tasks.py list <issue> [--repo OWNER/REPO]
  issue_tasks.py tick <issue> (--all | --index N [N ...]) [--repo OWNER/REPO] [--dry-run]
  issue_tasks.py list --stdin          # parse a body from stdin (offline / testing)

`list` prints JSON: number, title, state, open_tasks [{index, text, issue_ref}], done_count.
`index` is the position among ALL checkboxes in the body (0-based) and is what
`tick --index` expects. Checkboxes inside fenced code blocks are ignored.
"""
import argparse
import json
import re
import subprocess
import sys

BOX = re.compile(r"^(\s*(?:[-*+]|\d+[.)])\s+)\[([ xX])\](\s+.*)?$")
FENCE = re.compile(r"^\s*(```|~~~)")
ISSUE_REF = re.compile(r"(?<![\w&])(?:[\w.-]+/[\w.-]+)?#(\d+)\b|github\.com/[^/\s]+/[^/\s]+/issues/(\d+)")


def scan(body):
    """Yield (line_no, box_index, checked, text) for every real checkbox."""
    in_fence = False
    idx = 0
    for ln, line in enumerate(body.split("\n")):
        if FENCE.match(line):
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        m = BOX.match(line)
        if m:
            yield ln, idx, m.group(2) != " ", (m.group(3) or "").strip()
            idx += 1


def summarize(body):
    open_tasks, done = [], 0
    for _, idx, checked, text in scan(body):
        if checked:
            done += 1
        else:
            ref = ISSUE_REF.search(text)
            open_tasks.append({
                "index": idx,
                "text": text,
                "issue_ref": int(ref.group(1) or ref.group(2)) if ref else None,
            })
    return {"open_tasks": open_tasks, "done_count": done}


def tick(body, indices):
    lines = body.split("\n")
    hit = set()
    for ln, idx, checked, _ in scan(body):
        if indices is None or idx in indices:
            if not checked:
                m = BOX.match(lines[ln])
                lines[ln] = f"{m.group(1)}[x]{m.group(3) or ''}"
            hit.add(idx)
    if indices is not None and (missing := set(indices) - hit):
        sys.exit(f"No checkbox with index {sorted(missing)}")
    return "\n".join(lines)


def gh(*args, stdin=None):
    r = subprocess.run(["gh", *args], input=stdin, capture_output=True, text=True)
    if r.returncode != 0:
        sys.exit(r.stderr.strip() or f"gh {' '.join(args)} failed")
    return r.stdout


def fetch(issue, repo):
    args = ["issue", "view", str(issue), "--json", "number,title,state,body"]
    if repo:
        args += ["--repo", repo]
    return json.loads(gh(*args))


def main():
    p = argparse.ArgumentParser()
    sub = p.add_subparsers(dest="cmd", required=True)
    pl = sub.add_parser("list")
    pl.add_argument("issue", nargs="?")
    pl.add_argument("--repo")
    pl.add_argument("--stdin", action="store_true")
    pt = sub.add_parser("tick")
    pt.add_argument("issue")
    pt.add_argument("--repo")
    g = pt.add_mutually_exclusive_group(required=True)
    g.add_argument("--all", action="store_true")
    g.add_argument("--index", type=int, nargs="+")
    pt.add_argument("--dry-run", action="store_true")
    a = p.parse_args()

    if a.cmd == "list":
        if a.stdin:
            print(json.dumps(summarize(sys.stdin.read()), indent=2))
            return
        if not a.issue:
            p.error("issue number required unless --stdin")
        data = fetch(a.issue, a.repo)
        out = {k: data[k] for k in ("number", "title", "state")}
        out.update(summarize(data.get("body") or ""))
        print(json.dumps(out, indent=2))
        return

    data = fetch(a.issue, a.repo)
    new_body = tick(data.get("body") or "", None if a.all else set(a.index))
    if a.dry_run:
        print(new_body)
        return
    # gh api, not `gh issue edit`: the latter fails on repos linked to Projects (classic).
    repo = a.repo or "{owner}/{repo}"
    gh("api", "-X", "PATCH", f"repos/{repo}/issues/{a.issue}", "-F", "body=@-", stdin=new_body)
    print(json.dumps(summarize(new_body), indent=2))


if __name__ == "__main__":
    main()
