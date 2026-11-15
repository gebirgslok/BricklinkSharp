# 0003: One wire module between Endpoint and HTTP

Status: accepted (RFC 0006)

## Context
Turning an `Endpoint<T>` into a signed request and a response into `T` was spread over `Endpoint`, `BricklinkRequestExecutor`, `BricklinkResponseParser`, `OAuthRequest` and `OAuthUtilities`. The query was encoded once for the URL and again, by different rules, for the OAuth signature, after re-parsing the finished URL. `Endpoint<T>` also referenced the executor through `Project`.

## Decision
`EndpointWire` has two pure functions: `ToRequest` (URL, signature, JSON body) and `ReadResponse` (all response policy). A `QueryString` holds raw name/value pairs and `Rfc3986` is the only encoder, so the URL and the signature use the same pairs and the same rules. `OAuth1Signer` signs a `QueryString`; nothing parses a URL back. `Endpoint<T>` is pure data and gains `Map` in place of `Project`. The executor only resolves credentials, calls the `IRequestHandler` hook and sends.

## Consequences
- Request and response policy are tested without `HttpClient`, including that the URL query equals the signed query.
- Signatures for plain ASCII values are unchanged. Values containing `%`, control characters below `0x10` or surrogate pairs now encode correctly where the old encoder did not.
- There is no seam to substitute the signer or reader. Add an interface only if a second signing scheme or reader is needed.
- Rejected: a public `IRequestSigner`, a ports-and-adapters pipeline and a `ResponseReader<T>` hierarchy.
