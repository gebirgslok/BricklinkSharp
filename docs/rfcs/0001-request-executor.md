---
github_issue: 33
status: in-progress (phases 1-2 done)
created: 2026-11-13
---

# RFC 0001: Deepen the request pipeline into a single request executor

## Summary
Replace the per-endpoint request plumbing in `BricklinkClient` (URL building, hook call, one of 8 `HttpClient` wrappers, JSON parsing) with one concrete `BricklinkRequestExecutor` exposing a single entry point, `SendAsync<T>(BricklinkRequest, CancellationToken)`. The executor owns URL/query building, credential resolution, OAuth1 signing, the `IBricklinkRequestHandler` hook, sending, and meta/data parsing with error mapping. `HttpMessageHandler` becomes the test seam, so the riskiest code in the library (currently untested) gets boundary tests. The public `IBricklinkClient` does not change.

## Motivation
- All 40 endpoints repeat: build URL, `MeasureRequestAsync`, call an `HttpClient` extension wrapper, parse. Verb, resource type, expected status, array/object shape and empty-allowed rule are scattered, so they can drift apart.
- The seams hide real bugs:
  - A non-JSON body (e.g. an HTML 5xx or rate-limit page) throws a raw `JsonException`, violating the `BricklinkException` rule in `CODING.md`.
  - `MeasureRequestAsync` resolves credentials but passes the original (possibly null) value to `IBricklinkRequestHandler`.
  - `HttpResponseMessage` is never disposed.
  - `JsonExtensions.IsEmpty` throws `InvalidOperationException` on non-object `data`.
  - No `ConfigureAwait(false)` anywhere, contrary to `CODING.md`.
  - The OAuth signer mutates caller parameter lists and uses static clock/nonce; signing reads the `BricklinkClientConfiguration.Instance` singleton.
- The tests cover only `ToDomainString`, a trivial `OrderMessage` test and live image-URL HEAD requests. Nothing covers request building, parsing, errors or signing.

## Proposal
One internal class per file, per `CODING.md`.

```csharp
internal sealed class BricklinkRequestExecutor
{
    BricklinkRequestExecutor(HttpClient http, BricklinkRequestExecutorOptions options);
    Task<T> SendAsync<T>(BricklinkRequest request, CancellationToken cancellationToken);
}

internal sealed class BricklinkRequestExecutorOptions
{
    Uri BaseUri; IBricklinkRequestHandler? RequestHandler;
    Func<BricklinkCredentials?> DefaultCredentialsProvider; // factory wires () => BricklinkClientConfiguration.Instance
    Func<DateTimeOffset> Clock; Func<string> NonceGenerator;
}

// Immutable description of one call; T is not part of it.
internal sealed class BricklinkRequest
{
    BricklinkRequest(BricklinkApiResourceType resource, HttpVerb verb, string path);
    BricklinkRequest WithQueryParameter(string name, object? value); // null skipped, invariant culture, escaped
    BricklinkRequest WithBody(object? body);                  // null-skipping serialization, always
    BricklinkRequest WithCredentials(BricklinkCredentials? credentials);
    BricklinkRequest WithExpectedMetaCode(int metaCode);                  // default: GET 200, PUT 200, POST 201, DELETE 204
    BricklinkRequest WithShape(ResponseShape shape);          // override of the T-derived default
}
internal readonly struct NoData { }                           // SendAsync<NoData> = data must be absent/empty
```

Response shape defaults from `T`: `NoData` requires empty data; an array type allows an empty array; any other type throws `BricklinkNoDataReceivedException` on empty data. `WithShape` is the escape hatch (e.g. a non-empty array required). Default expected code for `NoData` PUT is 204, as today.

Execution order: resolve credentials (per-call, else `DefaultCredentialsProvider`, validated) → call hook with the **resolved** credentials → sign (fresh parameter list, injected clock/nonce) → send → dispose response → parse. Every failure of parsing or mapping surfaces as a `BricklinkException` subtype (transport exceptions such as `HttpRequestException` are not mapped). Domain validation (`ValidateThrowException`) stays in the endpoint methods.

Endpoint shape after migration:

```csharp
public Task<CatalogItem> GetItemAsync(ItemType type, string no, BricklinkCredentials? credentials = null, CancellationToken ct = default)
    => _executor.SendAsync<CatalogItem>(
        new BricklinkRequest(BricklinkApiResourceType.Item, HttpVerb.Get, $"items/{type.ToDomainString()}/{no}")
            .WithCredentials(credentials), ct);
```

`BricklinkClientConfiguration.Instance` is read in exactly one place: the factory wiring of `DefaultCredentials`. Tests use `new HttpClient(stubHandler)`, fixed clock/nonce and explicit credentials.

## Phases
Each phase leaves the repo building with tests green. New code is added before old code is removed.

### Phase 1 — Deterministic, non-mutating OAuth signer
Goal: signing is a pure function of credentials, request and injected clock/nonce; the existing request path still works.
- [x] 1.1 Add clock and nonce parameters to `OAuthRequest`/`OAuthUtilities`, defaulting to the current behaviour
- [x] 1.2 Stop mutating the caller's parameter list in `GetSignature` and `ConcatenateRequestElements`
- [x] 1.3 Add signature tests against fixed vectors (known base string, key, clock, nonce)
- [x] 1.4 Add `CHANGELOG.md` entry only if any user-visible behaviour changed (no user-visible change; no entry needed)

### Phase 2 — Add the executor (unused by the client)
Goal: `BricklinkRequestExecutor`, `BricklinkRequest`, `BricklinkRequestExecutorOptions`, `ResponseShape` and `NoData` exist and are fully tested with a stub `HttpMessageHandler`; `BricklinkClient` is untouched.
- [x] 2.1 Implement `BricklinkRequest` (query building with null-skip/escaping, body, credentials, expected code, shape)
- [x] 2.2 Implement the executor: credential resolution, hook with resolved credentials, signing, send, response disposal, `ConfigureAwait(false)`
- [x] 2.3 Implement parsing and error mapping: non-JSON body, missing `meta`, status/meta-code mismatch, shape mismatches, non-object `data` without throwing
- [x] 2.4 Tests: URL and query, Authorization header with fixed clock/nonce, null-credentials-to-hook, non-JSON body → `BricklinkException`, empty/array/object shapes, null-skipping body
- [x] 2.5 Read `ResponseMeta`, `BricklinkApiResourceType`, `HttpVerb` and the factory; reconcile any assumption in this RFC that does not hold
  - Reconciled: `BricklinkMissingCredentialsException` derives from `Exception` (moved into 4.2). Today `GetParseResponseAsync<T[]>` (Category, Color, ItemMapping, KnownColor, Subset, Superset) rejects empty arrays; the T-derived default now allows them, so tasks 3.1/3.2 must add `.WithShape(ResponseShape.ArrayNonEmpty)` there to preserve behaviour. Query values: enums throw (pass the domain string), dates format as ISO 8601. Old `Post` calls serialized nulls; the executor always skips them. Absent `data` for array shapes is `BricklinkNoDataReceivedException` (was `UnexpectedDataKind`). New public `BricklinkInvalidResponseException`; changelog in 4.3. Renamed in code: `WithQueryParameter`, `WithExpectedMetaCode`, `DefaultCredentialsProvider`, `ResponseShape.ArrayOrEmpty`.

### Phase 3 — Migrate endpoints, delete the old pipeline
Goal: all 40 endpoints go through `SendAsync`; the old wrappers are gone.
- [ ] 3.1 Give `BricklinkClient` an executor and migrate catalog and price-guide endpoints (including the `UriBuilder` and `?color_id=` styles)
- [ ] 3.2 Migrate inventory, order, coupon, feedback, notification and remaining endpoints, including `GetOrderItemsAsync` (nested array) and `BuildIncludeExcludeParameter` call sites
- [ ] 3.3 Add per-area stub-handler tests for request URL/verb/body and response mapping
- [ ] 3.4 Delete the 8 `HttpClient` wrappers, the 2 private executors, `MeasureRequestAsync` and `JsonExtensions.GetData`

### Phase 4 — Wiring and polish
Goal: the factory builds the executor; documentation and exception hierarchy match `CODING.md`.
- [ ] 4.1 Wire `BricklinkClientFactory` to build the executor (the only read of `BricklinkClientConfiguration.Instance`)
- [ ] 4.2 Make `BricklinkInvalidParameterException` and `BricklinkMissingCredentialsException` derive from `BricklinkException` and fix the trailing-separator logic
- [ ] 4.3 `CHANGELOG.md` entries under `## Unreleased` (Added: `BricklinkInvalidResponseException`; Fixed: non-JSON body, converter errors, hook credentials, disposal; Changed: exception base classes)
- [ ] 4.4 Run demos' build and a final `dotnet test`; mention the issue can be closed

## Risks & alternatives
- Inferring shape from `T` is compact but implicit; mitigated by `WithShape`.
- `BricklinkInvalidParameterException` base-class change is observable to callers catching `Exception` precisely; it is still catchable as `Exception`, so low risk, but it is noted in the changelog.
- Some request details (query encoding, header format) are consumed by the live API; a regression would only show against the real service. Stub tests lock in today's URLs before migration (task 3.3).
- Rejected: pipeline-of-stages design (seven new abstractions, one implementation each; speculative), ports-and-adapters with four interfaces (`IHttpTransport` duplicates `HttpMessageHandler`), and per-verb `GetAsync`/`PutAsync`/`DeleteAsync` overloads (seven methods differing only by default status code, shallower than one method plus a request builder). A stage pipeline can be added later behind `SendAsync` if retry or rate limiting is needed.

## Out of scope
- Enum/converter consistency (unknown-value handling) and DTO validation/null-skipping consolidation.
- The image-URL helpers on `IBricklinkClient`.
- Merging `Direction` and `OrderDirection` (breaking).
- Any change to the public `IBricklinkClient` surface.
