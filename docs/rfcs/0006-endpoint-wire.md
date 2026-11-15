---
github_issue: 43
status: draft
created: 2026-11-15
---

# RFC 0006: Endpoint wire — one query, one signer, one response reader

## Summary

Merge two clusters on the path "`Endpoint<T>` → signed `HttpRequestMessage` → response → `T`": the HTTP execution core (`Endpoint`, `BricklinkRequestExecutor`, `BricklinkResponseParser`) and OAuth signing plus URL building (`OAuthRequest`, `OAuthUtilities`, `WebUtils`, `Endpoint.BuildUrl`). A new internal module `EndpointWire` exposes two pure functions, `ToRequest` and `ReadResponse`. A `QueryString` value type holds the query as raw name/value pairs and is the only thing that encodes, so the URL and the OAuth signature cannot diverge and nothing re-parses a URL. `Endpoint<T>` stops referencing the executor (`Map` replaces `Project`/`Retarget`/`GetProjected`), and the executor shrinks to credentials, the `IRequestHandler` hook and the `HttpClient` send. Internals only; the public API is unchanged.

## Motivation

RFC 0005 made `Endpoint<T>` the single description of an operation, but two seams on the path below it are still tangled.

**Query is encoded twice, by different rules.**
- `Endpoint.BuildUrl` encodes with `Uri.EscapeDataString`.
- `OAuthRequest` receives only the finished URL string. `OAuthUtilities.ConcatenateRequestElements` re-parses the query with `HttpUtility.ParseQueryString` (`WebUtils.ParseQueryString`) and re-encodes every value with a hand-written `UrlEncodeStrict`.
- By reading the code, `UrlEncodeStrict` has weaknesses: it encodes `char` by `char` (so a surrogate pair is not encoded as one UTF-8 sequence), and `PercentEncode` formats bytes as `%{b:X}`, which gives a single hex digit for most bytes below `0x10`. Both are outside what the existing tests cover. These claims come from reading, not from running the code; Phase 1 turns them into tests.
- Include/exclude filters such as `status=a,b,-c` are where URL and signature encodings must agree. A mismatch only shows up as an opaque 401 from the live API. No test pushes such a query through the whole path.
- `GetAuthorizationHeader(List<WebParameter>?)` has a `queryParameters` parameter the executor never passes. `BuildMessage` builds the header as a string and splits it on `' '` to make an `AuthenticationHeaderValue`. `OAuthRequest` signs but does not send, so its name misleads.

**`Endpoint<T>` is "pure data" but depends on the executor.**
- It holds `Func<BricklinkRequestExecutor, Endpoint<T>, BricklinkCredentials?, CancellationToken, Task<T>>? Project`, used only by `Endpoints.OrderItems`. That needs `Retarget<TOther>` and `BricklinkRequestExecutor.SendWithoutProjectionAsync`, and `SendAsync` branches on it.
- `Endpoint.Create` calls `BricklinkResponseParser.ResolveShape` and `DefaultExpectedMetaCode`, so response policy lives in `Endpoint`, the parser and `Endpoints.*`.
- `Endpoint.Rejected` builds an `Endpoint<T>` with `default` resource, verb and shape.
- The parser parses the body into a `JsonDocument` and then re-serializes `data` with `GetRawText()` to deserialize it, and does the same for `meta`.
- Understanding "how a request becomes a result" means reading four files.

**Tests follow the tangle.** `BricklinkRequestExecutorTests` (50 tests by grep, about 30 of them about response parsing and error mapping) needs a stub handler to test pure response policy. `EndpointUrlTests` and `OAuthRequestTests` test the URL and the signer separately, so the agreement between them is untested.

Constraints: `IBricklinkClient`, `IRequestHandler` and `IEndpointSender` unchanged; validation and `Rejection` behaviour from RFC 0005 unchanged (a rejected endpoint faults the task before credentials, hook or `HttpClient` are touched); injectable `Func<DateTimeOffset>` clock and `Func<string>` nonce kept; netstandard2.0 floor (`System.Text.Json` has no `JsonElement` deserialize overload there); follow `CODING.md` and the `CONTEXT.md` vocabulary; 2.0.0 clean break allows internal churn (ADR 0001).

## Proposal

Chosen from four designs (minimal two-function module, flexible with public signer, common-caller `Map`/`QueryString`, ports and adapters). The hybrid takes the two-function module from the minimal design and `QueryString` and `Map` from the common-caller design, and leaves out the extension points the other two add. Everything is `internal` unless noted.

```csharp
// One query representation, used for both the URL and the signature.
internal readonly struct QueryString
{
    IReadOnlyList<KeyValuePair<string, string>> Pairs;   // raw, unencoded
    QueryString With(string name, string value);
    string Encode();                                      // "a=1&b=x%2Cy", strict RFC 3986
}
internal static class Rfc3986 { static string Encode(string value); }  // unreserved only, UTF-8, upper-case hex; the only encoder

// Endpoint<T>: still pure data, no executor reference.
internal sealed class Endpoint<T>
{
    QueryString Query; ...                                // as before otherwise
    Endpoint<T> WithQuery(string name, object? value);    // unchanged
    Endpoint<T> WithQuery(string name, IEnumerable<string>? values);  // comma list for include/exclude; null/empty dropped
    Endpoint<TResult> Map<TResult>(Func<T, TResult> map); // replaces Project, Retarget, GetProjected
}

// The one module for request -> signed message and response -> T. Both functions are pure.
internal static class EndpointWire
{
    internal sealed record Signing(BricklinkCredentials Credentials, Func<DateTimeOffset> Clock, Func<string> Nonce);

    static HttpRequestMessage ToRequest<T>(Endpoint<T> endpoint, Uri baseUri, Signing signing);
    static T ReadResponse<T>(Endpoint<T> endpoint, Uri url, HttpMethod method, int httpStatusCode, string body);
}

internal static class OAuth1Signer   // replaces OAuthRequest
{
    static AuthenticationHeaderValue Sign(HttpMethod method, Uri baseResource, QueryString query,
        BricklinkCredentials credentials, DateTimeOffset timestamp, string nonce);
}
```

- **`ToRequest`** builds the URL from `endpoint.Query.Encode()`, signs with `OAuth1Signer` using the same `QueryString` pairs (sorted and encoded by `Rfc3986`), and sets the typed `Authorization` header and the JSON body (`WireJson`). No string is parsed back into parameters and the header is never split.
- **`ReadResponse`** owns all response policy: shape resolution and expected meta code move out of `BricklinkResponseParser` and `Endpoint.Create` into this module; meta check, shape checks, the six exception types and the single JSON parse sit behind it. `Endpoint<T>` carries `Shape` and `ExpectedMetaCode` as data, plus its pure mapper. `ReadResponse` takes `(Uri url, HttpMethod method)` rather than the request message, so it does not depend on the message type.
- **`Map`** composes a pure `Func<T, TResult>` over the wire type. `OrderItems` becomes `Endpoint<OrderItem[][]>.Get(...).Map(batches => batches.ToList())`. The endpoint never sees the executor.
- **Executor** `SendAsync`: rejection short-circuit (`Task.FromException`, as today), resolve credentials, `IRequestHandler.BeforeRequestAsync`, `ToRequest`, `HttpClient.SendAsync`, read body, `ReadResponse`. `BricklinkRequestExecutorOptions` keeps `Clock` and `NonceGenerator`.
- **JSON parsed once.** `meta` and `data` are read from the `JsonElement`. On targets where `System.Text.Json` lacks the element overload, `GetRawText()` stays for the final deserialize only.
- **Signature compatibility.** For plain ASCII values the new encoder must produce the same bytes as today, so existing known-vector tests keep passing. Differences are expected only for characters the old encoder mishandled (see Phase 1).

## Phases

Each phase leaves the repo building with tests green. New code is added before old code is removed.

### Phase 1 — Encoding and signing
Goal: `Rfc3986`, `QueryString` and `OAuth1Signer` exist and are tested, including a whole-path regression vector; the executor still uses the old path.
- [ ] Task 1.1 Add `Rfc3986.Encode` and `QueryString` (`With`, `Pairs`, `Encode`) with tests for unreserved characters, `,` `-` `+` space `~` `&` `=` `%`, non-ASCII and a surrogate pair
- [ ] Task 1.2 Add `OAuth1Signer.Sign` returning `AuthenticationHeaderValue`, built on `QueryString`; keep the fixed-clock/nonce header and base-string vectors from `OAuthRequestTests` passing against it
- [ ] Task 1.3 Characterization test comparing old `UrlEncodeStrict` and `Rfc3986.Encode` on ASCII vectors (equal) and on the edge cases (document where they differ and why the new result is correct)
- [ ] Task 1.4 Record the signature-vector tests for `a,b,-c`, `+`, space, `~` and non-ASCII values

### Phase 2 — `EndpointWire.ToRequest` and `Map`
Goal: `Endpoint<T>` holds a `QueryString`, has `WithQuery` (comma list) and `Map`, and the executor builds requests with `ToRequest`; `Project` and the executor reference are gone.
- [ ] Task 2.1 Switch `Endpoint<T>.Query` to `QueryString`; add the comma-list `WithQuery` overload and use it where `Endpoints.*` build include/exclude filters (`IncludeExclude`)
- [ ] Task 2.2 Add `EndpointWire.ToRequest` (URL, signing, JSON body) and use it from `SendAsync`
- [ ] Task 2.3 Add `Endpoint<T>.Map<TResult>`; convert `Endpoints.OrderItems`; remove `Project`, `Retarget`, `GetProjected` and `SendWithoutProjectionAsync`
- [ ] Task 2.4 Move `EndpointUrlTests` and the `OAuthRequestTests` header cases to `ToRequest` tests (URL, query and `Authorization` header from a fixed clock and nonce); add the test that the URL query equals the signed query for `a,b,-c`
- [ ] Task 2.5 Update `EndpointsTests` (`Project` assertion becomes an `OrderItems` mapping test)

### Phase 3 — `EndpointWire.ReadResponse`
Goal: all response policy lives in `EndpointWire`; `BricklinkResponseParser` is gone; the body is parsed once.
- [ ] Task 3.1 Move shape resolution, default expected meta code and the parse/meta/shape/error mapping into `EndpointWire.ReadResponse(endpoint, url, method, status, body)`; parse once from the `JsonElement`
- [ ] Task 3.2 `Endpoint.Create` and `Rejected` use the moved policy functions; `Rejected` no longer needs `default` fields to be meaningful (keep behaviour, document or tighten)
- [ ] Task 3.3 Move the roughly 30 parsing and error-mapping cases from `BricklinkRequestExecutorTests` to `ReadResponse` tests; keep executor tests for credentials, hook order, rejection short-circuit, cancellation and body content type; keep `EndpointSenderTests`
- [ ] Task 3.4 Confirm the fixture deserialization and wire-format characterization tests still pass unchanged

### Phase 4 — Cleanup and docs
Goal: no leftover scaffolding; docs reflect the new structure.
- [ ] Task 4.1 Delete `OAuthRequest`, `OAuthUtilities.UrlEncodeStrict`/`PercentEncode`/`ConcatenateRequestElements` (folded into the signer), `WebUtils.ParseQueryString`, `WebParameter` and `Endpoint.BuildUrl`; rename any remaining type that no longer matches its job
- [ ] Task 4.2 Update `CONTEXT.md` (Query, Signer vocabulary if introduced), `CODING.md` if rules change, and add an ADR if the `EndpointWire` seam warrants one
- [ ] Task 4.3 `CHANGELOG.md` entry under Unreleased: Fixed if any edge-character signature changes; otherwise internal-only note
- [ ] Task 4.4 Confirm once against the live API (needs credentials) that a filtered request (`a,b,-c`) and a request with a space or non-ASCII value are accepted; record the result in this RFC
- [ ] Task 4.5 Final `dotnet format`, build, test

## Risks & alternatives

- **Signature changes for edge characters.** The new encoder is stricter and may differ from `UrlEncodeStrict` on control characters, surrogate pairs and other inputs it mishandled. ASCII filters must be byte-identical. Mitigated by the Phase 1 characterization test and the live check in Task 4.4; only BrickLink can confirm acceptance.
- **`Endpoint<T>` still holds a delegate.** `Map` stores a pure `Func`. It is deterministic and executor-free, but endpoint equality is not structural. Accepted: it is set in one place.
- **`Map` changes the generic type**, so the `OrderItems` factory briefly builds an `Endpoint<OrderItem[][]>`. Mild; confined to one factory.
- **Two static functions are not an abstraction.** Easy to test, but there is no seam to substitute. If a seam is needed later, wrap them in an interface then.
- **netstandard2.0** lacks the `JsonElement` deserialize overload, so one `GetRawText()` may remain for the final deserialize.
- **Rejected alternatives:** a public `IRequestSigner`/`SigningContext` (freezes new public API for a single supported scheme, OAuth1/HMAC-SHA1); a full ports-and-adapters pipeline with `IClock`, `INonceSource`, `ICredentialSource` and an `HttpResult` struct (the existing `Func<>` delegates and `HttpClient` already are the ports); a `ResponseReader<T>`/`ResponsePolicy` type hierarchy (more types than the two functions need).

## Out of scope

- A public signing extension point (`IRequestSigner`) and pluggable pipeline stages, serializers or response readers.
- `IClock`, `INonceSource`, `ICredentialSource` and an `HttpResult` struct.
- Retry, rate limiting and timeout handling; wrapping `HttpRequestException` and `TaskCanceledException` in a `BricklinkException` (separate candidate; confirm against `CODING.md` first).
- Making `BricklinkClientConfiguration.Instance` non-static and a public composition seam.
- Converter and `WireMap` consolidation; image and thumbnail URL helpers and their live tests.
