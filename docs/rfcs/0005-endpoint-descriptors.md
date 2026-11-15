---
github_issue: 41
status: in-progress
created: 2026-11-14
---

# RFC 0005: Endpoint descriptors and a single send seam

## Summary

Move per-endpoint policy (path, query, body, response shape, expected meta code, validation) out of the ~50 hand-assembled `BricklinkClient` methods into pure `Endpoints` factories that return an `Endpoint<T>` description. `BricklinkClient` becomes one line per method and depends on a single seam, `IEndpointSender`, implemented by `BricklinkRequestExecutor`. Validation becomes part of building an endpoint, so it cannot be forgotten, and wire contracts become testable with plain asserts and no `HttpClient`.

## Motivation

Policy for one endpoint is spread over three places: the client method (builder chain), the executor (`GetDefaultExpectedMetaCode`, `ResolveShape<T>`) and a hand-written `DtoValidation.Validate(...)` call that exists in only four methods. Consequences found in the current code:

- `UpdateOrderAsync` and the feedback bodies are not validated; nothing forces a new endpoint to be.
- `GetFeedbacksAsync` lowercases `direction` itself instead of using `ToDomainString()`.
- `color_id` is handled three ways: `GetSupersetsAsync` omits it when `colorId <= 0`, `GetSubsetsAsync` and `GetPriceGuideAsync` always send it (see issue #3).
- `PostFeedbackAsync`, `ReplyToFeedbackAsync`, `UpdateOrderStatusAsync` and `UpdatePaymentStatusAsync` use anonymous-object bodies, so wire names are checked only by string comparison.
- `GetOrderItemsAsync` unwraps `OrderItem[][]` inline in the client.
- `BricklinkClientEndpointTests` needs the full client plus stub handler to check URL and body, and has no cases for `CreateInventory`, `CreateInventories`, `UpdateInventory`, `UpdateOrder`, `CreateCoupon` or `UpdateCoupon`.

Constraints: `IBricklinkClient` public signatures unchanged; validation runs before any I/O and surfaces as a faulted `Task`; credentials and `CancellationToken` flow through; tests need no network or credentials (`BricklinkTestHost`, `StubHttpMessageHandler`); netstandard2.0 floor; follow `CODING.md`.

## Proposal

Ports and adapters, chosen over three alternatives (minimal verb gateway, fully generic `Endpoint<TIn,TOut>` builder, common-caller gateway) because it is the only one where policy has one home per endpoint and the wire contract needs no `HttpClient` to test.

```csharp
// Pure data: what to send and how to read the reply.
internal sealed class Endpoint<T>
{
    ApiResourceType Resource; HttpVerb Verb; string Path;
    IReadOnlyList<KeyValuePair<string,string>> Query;   // nulls dropped, enums via ToDomainString
    object? Body;                                        // always a named wire DTO, never anonymous
    int ExpectedMetaCode; ResponseShape Shape;           // resolved at construction
    Func<object, T>? Project;                            // e.g. OrderItem[][] -> List<OrderItem[]>
    BricklinkException? Rejection;                       // validation failure as data
}

internal static class Endpoints   // one pure factory per API operation, split by API area
{
    static Endpoint<Superset[]> Supersets(ItemType t, string no, int colorId);
    static Endpoint<Inventory>  CreateInventory(NewInventory dto);   // validates
    static Endpoint<NoData>     UpdateOrderStatus(int id, OrderStatus s);
}

internal interface IEndpointSender
{
    Task<T> SendAsync<T>(Endpoint<T> endpoint, BricklinkCredentials? credentials, CancellationToken ct);
}
```

- `BricklinkRequestExecutor` implements `IEndpointSender`. A non-null `Rejection` returns `Task.FromException<T>` before credentials, request handler or `HttpClient` are touched.
- Validation lives inside each factory that takes a body. A table-driven test asserts every body-carrying endpoint either validates or is on an explicit exempt list.
- Typed wire records (`FieldChange`, `ReplyBody`, ...) with `[JsonPropertyName]` replace anonymous bodies. Body serialization uses a shared `WireJson` so tests can compare JSON.
- `BricklinkRequest` is kept as the executor's inner representation while migrating; retiring or folding it is decided in the last phase.
- `color_id` rule: decide once, shared by subsets, supersets and price guide. Default is omit when `<= 0`. This changes `GetSubsetsAsync` and `GetPriceGuideAsync` on the wire and needs a changelog entry. **Decided (Phase 1, by the user):** omit `color_id` when `<= 0`, for subsets, supersets and price guide.

## Phases

### Phase 1 — Seam and core types
Goal: `Endpoint<T>`, `IEndpointSender` and wire body records exist and are covered by tests; the client still uses the old path, so nothing user-visible changes.
- [x] Task 1.1 Add `Endpoint<T>` with verb helpers (`Get`/`Post`/`Put`/`Delete`), default meta code and shape resolution moved from the executor, and `Rejected(...)`
- [x] Task 1.2 Add shared `WireJson` serializer and typed wire body records (`FieldChange`, `ReplyBody`, ...)
- [x] Task 1.3 Implement `IEndpointSender` on `BricklinkRequestExecutor` (rejection short-circuit, credentials and token flow)
- [x] Task 1.4 Executor boundary tests: sends what the `Endpoint` describes, rejection makes zero `HttpClient` calls
- [x] Task 1.5 Confirm `color_id=0` server behaviour with the user and record the decision in this RFC (decision: omit when `<= 0`; server behaviour not verified against the live API)

### Phase 2 — Read endpoints
Goal: all GET endpoints are built by `Endpoints` factories and sent through `IEndpointSender`; the `color_id` and `direction` inconsistencies are fixed.
- [x] Task 2.1 `Endpoints` factories for catalog, price guide, supersets, subsets, colors, categories and mappings
- [x] Task 2.2 Factories for the remaining GET endpoints (orders, order items with `Project` (add `Project` to `Endpoint<T>` here; Phase 1 left it out as unused), feedback, inventories, coupons, members, notifications)
- [x] Task 2.3 Switch the matching `BricklinkClient` methods to one-line calls; unify `color_id`; use `ToDomainString()` for `direction`
- [x] Task 2.4 Move URL/query assertions from `BricklinkClientEndpointTests` to pure `EndpointsTests`; keep a few end-to-end stub tests; add missing cases (`GetOrders` and `GetCoupons` status flags, `GetSubsets` flags)
- [x] Task 2.5 Changelog entry for any wire change

### Phase 3 — Write endpoints and validation
Goal: all POST/PUT/DELETE endpoints are built by factories with validation inside; anonymous bodies are gone.
- [x] Task 3.1 Factories for inventory create/update/delete and `CreateInventories` (validates the whole batch before sending)
- [x] Task 3.2 Factories for coupons, orders (including `UpdateOrder`), feedback post/reply, order and payment status, drive-thru (expected code 204)
- [x] Task 3.3 Add validation for `UpdateOrder` and the feedback bodies
- [x] Task 3.4 Switch the matching client methods; add body-JSON tests for every body-carrying endpoint
- [x] Task 3.5 Table-driven test: every body-carrying endpoint validates or is on an explicit exempt list; keep `Should_ReturnFaultedTask_When_ValidationFails`
- [x] Task 3.6 Changelog entry

### Phase 4 — Cleanup and docs
Goal: no leftover scaffolding; docs reflect the new structure.
- [ ] Task 4.1 Remove now-unused builder paths and executor default logic; decide whether `BricklinkRequest` is folded into `Endpoint<T>` or stays internal to the executor
- [ ] Task 4.2 Update `CONTEXT.md` (Endpoint, Rejection), `CODING.md` if rules change, and add an ADR if the `IEndpointSender` seam warrants one
- [ ] Task 4.3 Final `dotnet format`, build, test; mention issue #3 can be closed

## Risks & alternatives

- **Large `Endpoints` surface (~50 factories).** Mitigated by splitting per API area.
- **`Project` as `Func<object,T>`** is slightly awkward; a typed variant would need a second type parameter for a single use.
- **Wire change** from unifying `color_id` may surprise callers; mitigated by the changelog entry and the Phase 1 decision.
- **Rejected alternatives:** minimal `Endpoint<T>` plus runner and verb-based `BricklinkApi` gateway (policy stays at call sites, so inconsistencies can return; validation by type switch can silently miss a new DTO); fully generic `Endpoint<TIn,TOut>` with rules and hooks (largest surface, awkward generics, escape hatch invites scattering).

## Out of scope

- OAuth signing and URL encoding round trip (separate candidate: decode/re-encode of query values).
- Image and thumbnail URL helpers on the client and their network-dependent tests.
- Converter and `WireMap` consolidation; `ChangedQuantityStringConverter` null handling.
- Making `BricklinkClientConfiguration.Instance` non-static.
