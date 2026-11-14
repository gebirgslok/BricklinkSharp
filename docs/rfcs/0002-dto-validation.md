---
github_issue: 35
status: in-progress
created: 2026-11-14
---

# RFC 0002: Consolidate DTO validation into `DtoValidation`

## Summary
Replace the four copies of `ValidateThrowException()` (`NewInventory`, `UpdateInventory`, `CouponBase`; `NewCoupon`/`UpdateCoupon` inherit) with one internal static class, `DtoValidation`, exposing `Validate(NewInventory)`, `Validate(UpdateInventory)` and `Validate(CouponBase)`. A private tier engine owns the tiered-price rules that are currently duplicated. Characterization tests are written first, so the refactor is provably behaviour-preserving; the few intentional behaviour changes are isolated in the last phase. The public `IBricklinkClient` and the public DTO properties do not change.

## Motivation
- The tier rules (all-six-or-none, ordering, unit-price bound) are copied into three classes with accidental differences:
  - "Set" means `> 0` on `NewInventory`/`CouponBase` (non-nullable) and `!= null` on `UpdateInventory`.
  - `UpdateInventory` compares `TierPrice1 >= UnitPrice` against a nullable `UnitPrice`; when it is null the comparison is false and the rule is skipped silently, untested and undocumented.
  - The stock-room rule is two checks in `NewInventory` (null, then A/B/C) and one in `UpdateInventory` (null folded into the A/B/C message, value `"null"`).
- Inventory tiers must descend in price and ascend in quantity; coupon tiers ascend in both. This is a real domain difference but is buried in duplicated code.
- Validation is `internal` and reachable only through a client call. The only related test is `Should_ReturnFaultedTask_When_ValidationFails`; no tier, stock-room or bulk rule is covered.
- Constraints: public surface unchanged; `BricklinkInvalidParameterException` shape and message format unchanged; errors still surface as a faulted `Task` (see RFC 0001 task 3.4); netstandard2.0 compatible.

## Proposal
One internal class per file, per `CODING.md`.

```csharp
internal static class DtoValidation
{
    // Throw BricklinkInvalidParameterException or return.
    internal static void Validate(NewInventory dto);
    internal static void Validate(UpdateInventory dto);
    internal static void Validate(CouponBase dto);   // NewCoupon and UpdateCoupon
}
```

A private tier engine (a nested or sibling internal type, not exposed) is configured per DTO with: the prices and quantities (or discount rates), the order direction, the optional upper bound (`UnitPrice`), and the "set" predicate. Each DTO's "set" meaning is mapped in one visible line before it reaches the engine, so the `> 0` versus `!= null` difference stays readable. The engine owns set-counting, the all-or-nothing rule, pairwise ordering checks, and building the invalid-parameter dictionary and rule message. A shared stock-room check and a shared bulk check sit beside it.

`ValidateThrowException()` is deleted from the DTOs; the five call sites in `BricklinkClient` become `DtoValidation.Validate(x)` and stay at the top of the async method, so failures remain a faulted `Task`. `InventoryBase` keeps only properties.

Preserved verbatim:
- Exception message text and key names, including the missing space in "must be set" + "when using tiered pricing" and the unbalanced parenthesis in the stock-room message.
- Check order: `NewInventory` is completeness, stock room, tiers, bulk (first error wins; tests pin it).
- Coupon tiers are validated only when `DiscountType == Percentage`.
- The inventory/coupon direction difference and the two "set" semantics.

Intentional behaviour changes (phase 3, listed in `CHANGELOG.md`):
- `UpdateInventory` with set tiers and a null `UnitPrice`: the skip of the `TierPrice1 < UnitPrice` rule becomes explicit and documented (a partial update cannot know the base price). No change in outcome.
- `UpdateInventory` null `StockRoomId` with `IsStockRoom == true` uses the same null message as `NewInventory`. This changes that message and its dictionary value.

## Phases
Each phase leaves the repo building with tests green. New code is added before old code is removed.

### Phase 1 — Characterization tests
Goal: current validation behaviour is pinned through the existing internal `ValidateThrowException()`; no production code changes except the tier-quantity fix below.
- [x] 1.1 `NewInventory` tests: completeness vs item type, stock room (null, invalid, valid), tier all-or-none, tier ordering (each offending key), bulk, and check-order precedence
- [x] 1.2 `UpdateInventory` tests: stock room (null folded into the A/B/C message), tier all-or-none, ordering, null `UnitPrice` skip, bulk
- [x] 1.3 `CouponBase` tests via `NewCoupon`/`UpdateCoupon`: tiers validated only for `Percentage`, all-or-none, ascending order (each offending key)
- [x] 1.4 Assert `InvalidParameters` keys, `TypeOfDataObject` and exact message text (including the existing typos)
- [x] 1.5 Run `dotnet build` and `dotnet test`
- [x] 1.6 Fix inverted tier-quantity comparison found by 1.1/1.2 (`NewInventory`, `UpdateInventory` rejected ascending quantities); changelog entry under Fixed

### Phase 2 — Introduce `DtoValidation` and migrate (pure refactor)
Goal: all validation lives in `DtoValidation`; all phase 1 tests pass unchanged apart from their entry point.
- [x] 2.1 Add `DtoValidation` with the private tier engine, stock-room and bulk checks
- [x] 2.2 Implement the three `Validate` overloads reproducing current behaviour exactly
- [x] 2.3 Switch the five `BricklinkClient` call sites and delete `ValidateThrowException()` from the DTOs
- [x] 2.4 Re-point the phase 1 tests at `DtoValidation.Validate`; keep `Should_ReturnFaultedTask_When_ValidationFails` green
- [x] 2.5 Run `dotnet build` and `dotnet test`

### Phase 3 — Intentional behaviour changes and changelog
Goal: the two deliberate changes are made, tested and documented.
- [ ] 3.1 Make the null `UnitPrice` skip in `UpdateInventory` explicit, with a test and a comment on the reason
- [ ] 3.2 Unify the null `StockRoomId` message in `UpdateInventory` with `NewInventory`'s; update the affected test
- [ ] 3.3 Add `CHANGELOG.md` entries under `## Unreleased` (Changed)
- [ ] 3.4 Run the demos' build and a final `dotnet test`; mention the issue can be closed

## Risks & alternatives
- A behaviour regression in a rule nobody tested before is the main risk; phase 1 exists to remove it.
- Rejected: a composable rule-set engine (`IDtoRule`, `RuleSet<T>`, `TierSpec<T>` with delegates) as speculative for four DTOs and about ten rules; and per-rule public types (`Tier`, `TierRules`, `Guard`), which let tests reach inside instead of testing at the boundary.
- Rejected for now: a public `Validate()`/`IsValid` on the DTOs. It adds a permanent public contract (return type, message stability) for little value; callers already get a rich `InvalidParameters` dictionary. It can be added later if requested.
- A new validated DTO needs a new overload; acceptable at this size.

## Out of scope
- Null-skipping on the wire: omitting unset (zero) tier fields from the `NewInventory`/`CouponBase` JSON instead of sending `0`. It is a wire change that cannot be verified without live API calls, and the non-nullable property types cannot change without breaking the public API. Candidate for a separate RFC.
- Enum/converter consistency (unknown-value handling).
- The image-URL helpers on `IBricklinkClient`.
- Merging `Direction` and `OrderDirection` (breaking).
- Any change to the public `IBricklinkClient` surface.
