# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added
- .NET 10 build target
- `BricklinkInvalidResponseException`, thrown when a response body is not valid JSON, lacks `meta` or does not match the expected shape

### Changed
- `UpdateInventory` with `IsStockRoom == true` and no `StockRoomId` now fails with the same message and parameter value (`"Null"`) as `NewInventory`, instead of the "must be either A, B or C (set to: null" message
- `BricklinkInvalidParameterException` and `BricklinkMissingCredentialsException` now derive from `BricklinkException` (`RequestUrl` and `HttpMethod` are `null` for them) instead of `Exception`
- **Behaviour change:** unknown enum wire values (`new_or_used`, `completeness`, `status`, `payment.status`, `rating_of_bs`, coupon `discount_type`/`applies_to.type`, event type, item type and others) no longer silently map to a default member (for example `Condition.New`); they raise `BricklinkInvalidResponseException`. The same applies to malformed decimal/double strings (previously `0`) and out-of-range feedback `rating` values
- Enum wire formats now come from one table per enum (`WireMap`), shared by request building and response parsing

### Fixed
- A non-JSON response body (e.g. an HTML error page) or a JSON converter error no longer leaks a raw `JsonException`; it surfaces as a `BricklinkException`
- `IBricklinkRequestHandler` now receives the resolved credentials (per-call, else `BricklinkClientConfiguration.Instance`) instead of `null`
- `HttpResponseMessage` is now disposed after every request
- Trailing separator in the `BricklinkInvalidParameterException` message listed `,` after the last parameter
- `NewInventory` and `UpdateInventory` validation rejected ascending tier quantities (`TierQuantity1 < TierQuantity2 < TierQuantity3`), the order its own error message demands, and accepted descending ones

### Removed
- **Breaking:** `IBricklinkClient.GetPartOutValueFromPageAsync` and everything that existed only for it: `PartOutResult`, `PartOutItemType`, `BricklinkPartOutParseErrorException`, `BricklinkPartOutRequestErrorException` and the exchange rate service (`BricklinkSharp.Client.CurrencyRates`, including `ExchangeRatesApiDotIoConfiguration`). BrickLink now protects the scraped page with a bot challenge, so the method no longer works. The public `BricklinkClient` constructor no longer takes an `IExchangeRatesService`

## 1.9.1 - 2026-06-19

### Fixed
- [GetParseResponseArrayAllowEmpty passes credentials as body to ExecuteRequestAsync](https://github.com/gebirgslok/BricklinkSharp/issues/28)

## 1.9.0 - 2025-03-17

### Added
- `IsCounterpart` to `SubsetEntry` (see [SubsetEntry is missing IsCounterpart parameter](https://github.com/gebirgslok/BricklinkSharp/issues/27))

### Removed
- Deprecated `GetObjectData(SerializationInfo, StreamingContext)` methods from custom exceptions

### Security
- Updated System.Text.Json NuGet dependency to v8.0.5 for .NET Standard targets, see [GHSA-8g4q-xg66-9fp4](https://github.com/advisories/GHSA-8g4q-xg66-9fp4)

## 1.8.0 - 2024-07-17

### Security
- Updated System.Text.Json NuGet dependency to v8.0.4 for .NET Standard targets, see [GHSA-hh2w-p6rv-4g7w](https://github.com/advisories/GHSA-hh2w-p6rv-4g7w)

## 1.7.0 - 2024-07-03

### Added
- Methods to retrieve thumbnail images from catalog items (see [Retrieve thumbnail image url](https://github.com/gebirgslok/BricklinkSharp/issues/25)), thanks to [stephanstapel](https://github.com/stephanstapel)

## 1.6.2 - 2023-12-15

### Fixed
- [GetPartOutValueFromPageAsync for >$1k sets returns just thousands as single digit](https://github.com/gebirgslok/BricklinkSharp/issues/22), thanks to [norberto5](https://github.com/norberto5)

## 1.6.1 - 2023-12-05

### Added
- `order.store_name` to `GetOrderAsync` response, thanks to [stephanstapel](https://github.com/stephanstapel)

## 1.6.0 - 2023-08-23

### Added
- Option to inject a custom `IBricklinkRequestHandler` to handle the BrickLink API rate limiting (5000 requests per day) in your own way, thanks to [stephanstapel](https://github.com/stephanstapel)

## 1.5.2 - 2023-07-13

### Added
- `order.date_status_changed` to `GetOrderAsync` response, thanks to [norberto5](https://github.com/norberto5)

## 1.5.1 - 2023-04-08

### Added
- `shipping.address.phone_number` to `GetOrderAsync` response

## 1.5.0 - 2023-03-21

### Added
- Optional parameter `BricklinkCredentials? credentials = null` on every `IBricklinkClient` method that requires authentication. It takes precedence over the global credentials (`BricklinkClientConfiguration.Instance`) and is useful for server scenarios (e.g. ASP.NET Core) where you need to make a request in the name of the logged-in user

### Changed
- `ItemTypeStringConverter` now fails on unknown strings instead of returning `ItemType.Minifig` as default

### Fixed
- [Exception updating inventory items](https://github.com/gebirgslok/BricklinkSharp/issues/13), thanks to [stephanstapel](https://github.com/stephanstapel)

## 1.4.1 - 2023-01-29

### Fixed
- [GetPriceGuideAsync does not support the 'vat' parameter](https://github.com/gebirgslok/BricklinkSharp/issues/11), thanks to [cleargreen](https://github.com/cleargreen)

## 1.4.0 - 2023-01-10

### Added
- .NET 7 build target

### Fixed
- [UpdatePaymentStatusAsync expects wrong HTTP status code](https://github.com/gebirgslok/BricklinkSharp/issues/7), thanks to [Fandermill](https://github.com/Fandermill)

## 1.3.1 - 2022-02-12

### Changed
- **Breaking:** renamed enum `RatingTargeRole` to `RatingTargetRole` (typo fix) and the property `RatingTargetRole` in class `Feedback` accordingly
- Refactored all .cs files to use file-scoped namespaces

### Fixed
- [Error with EnumExtensions](https://github.com/gebirgslok/BricklinkSharp/issues/5) along with many performance improvements, thanks to [xramcire](https://github.com/xramcire)

## 1.3.0 - 2022-02-09

### Added
- `CancellationToken` support for all async methods

### Fixed
- `GetElementIdAsync` threw an exception if no element ID is available. It now returns an empty array

## 1.2.1 - 2022-01-15

### Fixed
- Typo in `JsonPropertyName` of the `UpdateInventory.StockRoomId` property that made it impossible to update the stock room ID, thanks to [JelleCeulemans](https://github.com/JelleCeulemans) ([bricklink store](https://store.bricklink.com/JelleCeulemans))

## 1.2.0 - 2021-12-24

### Added
- .NET 6 build target

### Changed
- Upgraded Demos and Tests to .NET 6
- Minor performance improvements

### Removed
- Unused code

### Fixed
- All obsolete warnings for .NET 6

## 1.1.1 - 2021-09-19

### Fixed
- [Get Supersets does not use color id, just gets all supersets](https://github.com/gebirgslok/BricklinkSharp/issues/3), thanks to [Yoonwoo](https://github.com/Yoonwoo)

## 1.1.0 - 2021-04-05

### Added
- Support for an API key for the [exchange rate service](https://exchangeratesapi.io/) when using `GetPartOutValueFromPageAsync` with the exchange rate service (see demo project and README)

## 1.0.0 - 2021-03-22

### Added
- Coverage of all public API endpoints
- `GetPartOutValueFromPageAsync` (demo project, docs, exchange rate service)
- `IBricklinkClient.GetBookImage`, `GetGearImage`, `GetCatalogImage`, `GetInstructionImage` and `GetOriginalBoxImage`, which build and return the image URL for a specific item number

### Changed
- New assembly versioning:
  - Assembly Version: 1.0.0.0
  - Assembly File Version: 1.0.0.\<build ID\>
  - Assembly Informational File Version: 1.0.0-\<build time\>+\<build date\>

## 0.7.0 - 2021-02-03

### Added
- `IBricklinkClient.GetMinifigImage` and `GetSetImage`, which build and return the image URL for a specific minifigure or set number

## 0.6.1 - 2021-01-04

### Fixed
- `OrderMessage.Subject` can be `null`, thanks to [aalex675](https://github.com/aalex675)

## 0.6.0 - 2020-12-19

### Added
- `IBricklinkClient.GetPartImageForColor`, which builds and returns the image URL for a specific part number / color ID
- `IBricklinkClient.EnsureImageUrlScheme`, which adds (ensures) a URI scheme on image URLs returned by the BrickLink API

## 0.5.0 - 2020-10-24

### Added
- Coupons
- Nullable annotations

### Changed
- **Breaking:** renamed `UpdatedInventory` container to `UpdateInventory`

## 0.4.0 - 2020-09-10

### Added
- Order

### Changed
- `IBricklinkClient` implements `IDisposable` and manages one instance of `HttpClient`

## 0.3.0 - 2020-08-13

### Added
- Feedback
- Member / Get member rating
- Push Notification
- Setting / Shipping methods

### Changed
- **Breaking:** renamed `IBricklinkClient.GetItemNumber` to `GetItemNumberAsync`
- **Breaking:** renamed `IBricklinkClient.GetElementId` to `GetElementIdAsync`

## 0.2.0 - 2020-08-07

### Added
- User Inventory
- Item Mapping

## 0.1.0 - 2020-08-02

### Added
- OAuth1 handling
- Catalog Item
- Color
- Category
