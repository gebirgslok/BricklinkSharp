# BricklinkSharp

Be concise. Ask clarifying questions when uncertain.

Strongly-typed C# client for the BrickLink marketplace API (OAuth1, typed JSON, error handling). Published as NuGet package `BricklinkSharp`.

- `BricklinkSharp.Client/` — the library (netstandard2.0 + netcoreapp3.x, net5–net9; net10 has a property group but is not in `TargetFrameworks` — verify before relying on it)
- `BricklinkSharp.Client.Tests/` — NUnit tests
- `BricklinkSharp.Demos/` — console demos, one file per API area
- `Build/` — Azure Pipelines helper scripts (version/build number patching)

## Read before you edit

- Vocabulary → `CONTEXT.md` (use its terms in code, docs, commits)
- Code rules → `CODING.md` (authoritative)
- Decisions → `docs/adr/` (created when the first ADR is written)
- BrickLink API reference → https://www.bricklink.com/v3/api.page?page=started (JS-rendered; cannot be fetched, ask the user to paste relevant pages)

## Commands

```
dotnet build BricklinkSharp.sln
dotnet test BricklinkSharp.Client.Tests
```

Some tests hit the live BrickLink API and need credentials; the API is limited to 5000 requests/day.

## Gotchas

- Two identical enums exist: `Direction` (coupons) and `OrderDirection` (orders). Same concept; merging is a breaking change, so leave as is.
- `GetPartOutValueFromPageAsync` scrapes the public HTML page — no API, no credentials.
- Every user-visible change: update the README changelog and bump the version (see commit "update version to 1.9.1").

## Review agents (`.claude/agents/`)

- `code-reviewer` — critical review against `CODING.md`
- `naming-specialist` — naming quality only
