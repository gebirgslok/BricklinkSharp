# 0002: Endpoint descriptors behind a single send seam

Status: accepted (RFC 0005)

## Context
Per-endpoint policy (path, query, body, response shape, expected meta code, validation) was spread over `BricklinkClient` methods, the executor and hand-written `DtoValidation` calls. Validation was missing for some bodies, `color_id` and `direction` were handled inconsistently, and checking a URL or body needed the full client plus a stub `HttpClient`.

## Decision
Each API operation is a pure factory in `Endpoints` returning an `Endpoint<T>`. `BricklinkClient` only calls `IEndpointSender.SendAsync`, implemented by `BricklinkRequestExecutor`. Validation lives in the factory and surfaces as `Endpoint<T>.Rejection`, which the sender turns into a faulted task before any I/O. `BricklinkRequest` is removed; `Endpoint<T>` is the only description of a request, and the executor reads it directly.

## Consequences
- Policy for one endpoint has one home, and a new body-carrying endpoint is caught by the validation table test if it neither validates nor is exempt.
- Wire contracts are tested with plain asserts, without `HttpClient`.
- `Endpoint<T>.Project` (a projection from the wire type) exists for the one endpoint whose wire shape differs from its result.
- Rejected: a verb-based gateway (policy stays at call sites) and a fully generic `Endpoint<TIn,TOut>` builder (largest surface, awkward generics).
