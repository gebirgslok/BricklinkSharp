---
github_issue: 37
status: draft
created: 2026-11-14
---

# RFC 0003: One owner for enum and scalar wire-format mapping (`WireMap`)

## Summary
Each enum's wire format (for example `Condition` `N`/`U`) is written twice: as a `ToDomainString` switch in `EnumExtensions.cs` and as an inverse switch in one of the `Json/*Converter.cs` files. Introduce an internal `WireMap<TEnum>` that owns both directions from one explicit table, and make the converters and `ToDomainString` thin adapters over it. Unknown wire values then fail instead of silently defaulting, as `CODING.md` already requires. A shared test host plus realistic DTO response fixtures come first, so every step is provable. The public enums, DTO shapes, `IBricklinkClient` and the wire output do not change.

## Motivation
- Adding an enum member means editing the enum, the switch and the converter, with no compiler link between them.
- `CODING.md` says enum converters fail on unknown values. Most do not:
  - `ConditionStringConverter` falls back to `New`, `CompletenessStringConverter` to `Complete`, `ShippingAreaStringConverter` to `Domestic`, `CouponStatusStringConverter` to `Expired`.
  - `EventTypeStringConverter` and `NullableItemTypeStringConverter` return `0` when `Enum.TryParse` fails.
  - `RatingTypeIntConverter` casts any int.
  - `DecimalStringConverter` and `DoubleStringConverter` return `0` for a malformed number.
  - `ItemTypeStringConverter` and `OrderStatusStringConverter` already throw.
- The converters have no direct tests. `EnumExtensionsTests` covers enum to string only, and only `Color` is ever mapped from a response body, so the 62 `[JsonConverter]` usages are untested at the mapping level.
- Test setup (stub handler, executor options, envelope JSON) is duplicated across `BricklinkRequestExecutorTests` and `BricklinkClientEndpointTests`.
- Constraints: public surface and wire output unchanged; `Direction` and `OrderDirection` stay separate; netstandard2.0 compatible (generic `Enum.Parse`/`GetValues` are behind `HAVE_GENERIC_ENUM_PARSE`); one class per file per `CODING.md`.

## Proposal
```csharp
internal sealed class WireMap<TEnum> where TEnum : struct, Enum
{
    static WireMap<TEnum> Of(params (TEnum Value, string Wire)[] pairs); // throws on duplicate value or wire
    static WireMap<TEnum> ByNameUpper();   // casing-convention enums (OrderStatus, ...)
    static WireMap<TEnum> ByNameLower();
    string ToWire(TEnum value);                       // throws if unmapped
    bool TryFromWire(string? wire, out TEnum value);
    TEnum FromWire(string? wire);                     // throws on unknown or null
}

internal static class WireMaps { /* one static field per enum: the single owner */ }
internal class WireStringConverter<TEnum> : JsonConverter<TEnum> { ctor(WireMap<TEnum>); }
internal class NullableWireStringConverter<TEnum> : JsonConverter<TEnum?> { /* null and "" -> null */ }
internal static class Scalars { ParseDecimal; ParseDouble; ParseRatingType }   // strict, invariant culture
```

- **Explicit literals:** irregular tables use literal wire strings, not `nameof`, so renaming an enum member cannot change the wire format. `ByNameUpper`/`ByNameLower` keep regular enums free of hand-written pairs.
- **No attribute churn:** the 17 existing converter classes stay as one-line subclasses of `WireStringConverter<T>`, so the 62 `[JsonConverter]` attributes are untouched. No converter factory registered in options, so DTOs deserialised with other options behave the same.
- **Adapters:** `ToDomainString` stays as a one-line forwarder (`=> WireMaps.X.ToWire(e)`) so the 48 call sites do not move.
- **Failure path:** converters throw `JsonException` wrapping the cause; the parser already maps it to `BricklinkInvalidResponseException`. The exact exception type and the parser's catch list are settled in phase 2 task 2.5.
- **Guard against drift:** one round-trip test per map walks `Enum.GetValues`, so a member added without a table entry fails a test, not production.

Test side (`BricklinkSharp.Client.Tests/Support/`):
- `BricklinkTestHost.Create(handler)` builds the stub handler, executor and client with OAuth set up once, and exposes the captured request.
- `Fixtures.Json("Order")` loads embedded `Fixtures/*.json`; `Fixtures.Envelope(...)` wraps a body as `{"meta":{"code":200},"data":...}`.
- Fixtures for Order, Inventory, Coupon and Feedback. Mutating one enum field in a fixture gives the unknown-value failure case.

## Phases
Each phase leaves the repo building with tests green. New code is added before old code is removed.

### Phase 1 — Test host, fixtures and characterization
Goal: current mapping behaviour is pinned end to end; no production code changes.
- [ ] 1.1 Add `BricklinkTestHost`, `Fixtures` and the embedded-resource wiring in the test csproj
- [ ] 1.2 Move `BricklinkRequestExecutorTests` and `BricklinkClientEndpointTests` onto the test host, removing the duplicated setup
- [ ] 1.3 Add realistic fixtures for Order, Inventory, Coupon and Feedback, with deserialisation tests asserting every mapped enum and number field
- [ ] 1.4 Pin every current `ToDomainString` mapping and every current converter `Read` result for known values
- [ ] 1.5 Pin today's behaviour for unknown values (the silent defaults) in clearly named tests, to be flipped in phase 3
- [ ] 1.6 Run `dotnet build` and `dotnet test`

### Phase 2 — `WireMap` core and adapters (no behaviour change)
Goal: both directions of every enum come from one table; phase 1 tests pass unchanged.
- [ ] 2.1 Add `WireMap<TEnum>` with duplicate detection, `ByNameUpper`/`ByNameLower`, and netstandard2.0-safe enumeration
- [ ] 2.2 Add `WireMaps` with one map per enum, using literal wire strings for irregular enums
- [ ] 2.3 Add `WireStringConverter<T>` and `NullableWireStringConverter<T>`; reduce the existing converter classes to one-line subclasses that keep today's unknown-value behaviour
- [ ] 2.4 Make `ToDomainString` one-line forwarders to `WireMaps`
- [ ] 2.5 Decide the thrown exception type (`JsonException` vs `FormatException`) and check the parser's catch list
- [ ] 2.6 Add a round-trip test per map over `Enum.GetValues`
- [ ] 2.7 Run `dotnet build` and `dotnet test`

### Phase 3 — Strict unknown values and changelog
Goal: unknown wire values fail as `CODING.md` requires; the change is tested and documented.
- [ ] 3.1 Switch every enum converter to strict `FromWire`; flip the phase 1.5 tests to expect `BricklinkInvalidResponseException`
- [ ] 3.2 Make `DecimalStringConverter`, `DoubleStringConverter` and `RatingTypeIntConverter` strict via `Scalars`
- [ ] 3.3 Narrow the parser's catch list now that converters throw cleanly
- [ ] 3.4 Add `CHANGELOG.md` entries under `## Unreleased` (Changed)
- [ ] 3.5 Run the demos' build and a final `dotnet test`; mention the issue can be closed

## Risks & alternatives
- BrickLink may return undocumented values (for example a new order status); strict parsing then raises `BricklinkInvalidResponseException`. Intended per `CODING.md`, but visible to callers, hence the changelog entry and phase 3 being separate.
- A malformed or empty number that BrickLink genuinely sends as `""` would now throw; phase 1 fixtures should show whether that occurs before phase 3.
- Rejected: attributes on the public enums (touches the public surface, awkward for `Direction`/`OrderDirection`, needs reflection); a source generator (overkill for about 20 enums on netstandard2.0); a `JsonConverterFactory` registered in options (reflection, and changes behaviour for DTOs deserialised elsewhere).
- Rejected: a builder with policies, aliases and case modes, and a public extension seam. No current caller; `Default`/`Fallback` policies would reopen the silent-default risk.
- Tables are maintained by hand; the round-trip tests are the enforcement.

## Out of scope
- Merging `Direction` and `OrderDirection` (breaking).
- Path-segment escaping and the image-URL helpers on `IBricklinkClient`.
- OAuth percent-encoding.
- Inventory request-body price serialisation (`UnitPrice` string vs number).
- Any change to the public `IBricklinkClient` surface.
