# Coding standards

Authoritative rules for `BricklinkSharp.Client`. Existing code that predates a rule is not a reason to break it; fix it when you touch it.

## Compatibility

- Library targets netstandard2.0 and up, `LangVersion` 10. No newer BCL APIs without `#if` guards per target.
- Nullable reference types are enabled; annotate honestly.
- Public API is never broken silently. If a breaking change is unavoidable, flag it with a warning in the README changelog (see 1.3.1).

## Style

- File-scoped namespaces.
- Every `.cs` file starts with the `#region License` header like its neighbours.
- One public type per file, named after the type.

## API shape

- Every async method takes a trailing `CancellationToken`.
- Every method that needs authentication takes an optional `BricklinkCredentials? credentials = null`, which takes precedence over `BricklinkClientConfiguration.Instance`.
- Library code uses `ConfigureAwait(false)` in new and touched code. Retrofitting existing code is a separate task.
- Failures surface as exceptions deriving from `BricklinkException`. No new exception base types.

## JSON

- System.Text.Json only. Property names via `JsonPropertyName` matching the BrickLink API.
- Custom converters live in `Json/`. Converters for enums fail on unknown values rather than defaulting.
- Update models (`Update*`) leave unset properties null so they are not sent.

## Adding an endpoint

1. Member on `IBricklinkClient`, implementation in `BricklinkClient`.
2. Models and enums as needed.
3. Demo in `BricklinkSharp.Demos/`.
4. README section and changelog entry.
5. Test in `BricklinkSharp.Client.Tests/`.

## Tests

- NUnit. Name tests `Should_{Result}_When_{Condition}`.
- Prefer tests that need no network or credentials; keep live-API tests few (daily request limit).
