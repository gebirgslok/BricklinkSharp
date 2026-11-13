# BricklinkSharp

Be concise. Ask clarifying questions when uncertain.

Strongly-typed C# client for the BrickLink marketplace API (OAuth1, typed JSON, error handling). Published as NuGet package `BricklinkSharp`.

- `BricklinkSharp.Client/` — the library (netstandard2.0 + netcoreapp3.x, net5–net10)
- `BricklinkSharp.Client.Tests/` — NUnit tests
- `BricklinkSharp.Demos/` — console demos, one file per API area
- `Build/` — CI helper scripts (version patching, release checks); workflows in `.github/workflows/`

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
- BrickLink protects its public HTML pages with a bot challenge (AWS WAF), so scraping them is not an option; use the API only.
- Every user-visible change: add an entry under `## Unreleased` in `CHANGELOG.md` (Keep a Changelog sections: Added/Changed/Fixed/Removed/Security).
- Release: rename `Unreleased` to the version and date in `CHANGELOG.md`, bump `PackageVersion` (and `FileVersion`/`InformationalVersion`) in `Directory.Build.props`, merge, then push tag `vX.Y.Z`. `.github/workflows/release.yml` verifies tag = `PackageVersion` plus a changelog section, tests, pushes to NuGet (trusted publishing) and creates the GitHub Release. A manual run of the workflow is a dry run.

## Review agents (`.claude/agents/`)

- `code-reviewer` — critical review against `CODING.md`
- `naming-specialist` — naming quality only
