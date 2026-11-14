---
github_issue: 39
status: draft
created: 2026-11-14
---

# RFC 0004: Naming sweep for 2.0.0

## Summary
Rename public types, members, properties and parameters so the API matches the vocabulary in `CONTEXT.md` and follows one set of naming rules. This is a deliberate clean break for 2.0.0: no `[Obsolete]` forwarders. JSON wire names do not change.

## Motivation
Names drifted over time: `Get*ListAsync` next to `Get*sAsync`, five spellings of "item number" (`no`, `number`, `partNo`, `Number`, `AlternateNo`), nine date-property shapes, `*Base` classes that carry no meaning, two identical direction enums, and `Rating`/`RatingType` meaning the opposite of the glossary. Since 2.0.0 breaks anyway, the cost of fixing public names is lowest now and only grows.

Constraints:
- Wire names are owned by `WireMap` and `JsonPropertyName`; they must not change.
- Every phase builds and passes tests (`dotnet build`, `dotnet test`).
- Renames of colliding names happen in strict order (`Rating` → `FeedbackCounts` before `RatingType` → `Rating`).

## Proposal

### Policy
- Clean break, no `[Obsolete]` forwarders (ADR 0001).
- `CODING.md` allows deliberate breaking releases and gains a Naming section: `…On` for dates, plural `Get<Things>Async`, `itemNumber`, no `Type`/`Base` suffix without meaning, `*Fields`/`*Terms` for shared writable bases.
- `CHANGELOG.md` carries the old → new table; README gets a migration section.

### Types and enums
| Old | New |
|---|---|
| `OrderDirection` | `Direction` (merged, old one deleted) |
| `ItemBase` | `Item` |
| `CostBase` | removed (`Order.Cost`/`DisplayCost` use `Cost`, or a standalone `CostSummary`) |
| `OrderBase` | removed (`OrderDetails : Order`) |
| `FeedbackBase` | removed (internal post body) |
| `SuperSubSetEntryBase` | removed (`Item`, `Quantity` declared on `SupersetEntry` and `SubsetEntry`) |
| `SuperSubSetItem` | `ContainedItemSummary` |
| `CouponBase` | `CouponTerms` |
| `InventoryBase` | `InventoryFields` |
| `Rating` (class) | `FeedbackCounts` |
| `RatingType` | `Rating` |
| `RatingTargetRole` | `FeedbackTargetRole` |
| `Notification` | `PushNotification` |
| `EventType` | `PushNotificationEventType` |
| `InventoryStatusType` | `InventoryStatus` |
| `PriceGuideType` | `PriceGuideSource` |
| `Name` | `PersonName` |
| `IBricklinkRequestHandler` | `IRequestHandler` |
| `BricklinkApiResourceType` | `ApiResourceType` |

### Client members
- Plurals: `GetColorListAsync` → `GetColorsAsync`, `GetCategoryListAsync` → `GetCategoriesAsync`, `GetInventoryListAsync` → `GetInventoriesAsync`, `GetFeedbackListAsync` → `GetFeedbacksAsync`, `GetShippingMethodListAsync` → `GetShippingMethodsAsync`.
- Catalog: `GetItemAsync` → `GetCatalogItemAsync`, `GetItemImageAsync` → `GetCatalogImageAsync`.
- Item Mapping: `GetElementIdAsync` → `GetItemMappingsByItemAsync`, `GetItemNumberAsync` → `GetItemMappingsByElementIdAsync`.
- Feedback: `PostFeedbackAsync` stays (posting is the domain action); `ReplyFeedbackAsync` → `ReplyToFeedbackAsync`.
- Request handler: `OnRequestAsync` → `BeforeRequestAsync`, `ct` → `cancellationToken`.
- Parameters: `no`/`number`/`partNo` → `itemNumber`; `methodId` → `shippingMethodId`.

### Properties
- Ids: `Feedback.FeedbackId`, `Inventory.InventoryId`, `OrderBase.OrderId`, `ShippingMethod.MethodId` → `Id`; `UpdateShipping.MethodId` → `ShippingMethodId`.
- Dates: `Coupon.DateIssued` → `IssuedOn`, `Coupon.DateExpire` → `ExpiresOn`, `UpdateShipping.DateShipped` → `ShippedOn`, `Shipping.ShipmentDate` → `ShippedOn`, `Payment.DatePaid` → `PaidOn`, `OrderMessage.DateSent` → `SentOn`, `Order.OrderDate` → `OrderedOn`, `OrderDetails.StatusChangedDate` → `StatusChangedOn`, `Notification.Timestamp` → `OccurredOn`.
- Rating counts: `NumOfPraises` → `PraiseCount`, `NumOfNeutrals` → `NeutralCount`, `NumOfComplaints` → `ComplaintCount`.
- Cost: `Cost.Coupon` → `CouponDiscount`, `Cost.Shipping` → `ShippingCost`, `Cost.Insurance` → `InsuranceCost`; same for `UpdateCost.Shipping`/`Insurance`.
- Inventory: `IsRetain` → `RetainsWhenSoldOut`, `Bulk` → `BulkQuantity`, `MyCost` → `OwnCost`, `MyWeight` → `OwnWeight`, `SaleRate` → `SaleRatePercent`.
- Catalog: `DimX`/`DimY`/`DimZ` → `DimensionX`/`DimensionY`/`DimensionZ`, `AlternateNo` → `AlternateNumber`.
- Exceptions: `ExpectedCode` → `ExpectedStatusCode`, `ReceivedCode` → `ReceivedStatusCode`, `TypeOfDataObject` → `DataObjectType`.

### Unchanged
Wire names; `Direction` members; all `Bricklink*` names except the two prefixes dropped above; `Display*`, `Tier*1..3`, `Etc1`/`Etc2`, `AppearsAs`; `BuyerName`/`SellerName`; `HttpVerb`; the `BricklinkClientConfiguration : BricklinkCredentials` inheritance; `CouponBase`/`InventoryBase` stay as shared bases (renamed only).

### Mechanics
Roslyn-based rename where tooling allows, otherwise a scripted pass in strict order. After each group: `dotnet build` and `dotnet test`. The `WireMap` characterization tests prove wire names did not change.

## Phases

### Phase 1 — Policy and structural type changes
Goal: policy documented; base classes and `Direction` consolidated; everything else untouched.
- [ ] Task 1.1: Write `docs/adr/0001-clean-break-2-0-0.md` (clean break, no `[Obsolete]` forwarders)
- [ ] Task 1.2: Update `CODING.md`: deliberate-break exception and Naming section
- [ ] Task 1.3: Remove the `Direction`/`OrderDirection` gotcha from `CLAUDE.md`; merge `OrderDirection` into `Direction`
- [ ] Task 1.4: `ItemBase` → `Item`; `SuperSubSetItem` → `ContainedItemSummary`
- [ ] Task 1.5: Remove `OrderBase` (`OrderDetails : Order`), `FeedbackBase` (internal post body), `CostBase` and `SuperSubSetEntryBase`
- [ ] Task 1.6: `CouponBase` → `CouponTerms`, `InventoryBase` → `InventoryFields`
- [ ] Task 1.7: Review (`code-reviewer`, `naming-specialist`), format, build, test

### Phase 2 — Type and enum renames
Goal: all remaining public type names match the glossary.
- [ ] Task 2.1: `Rating` → `FeedbackCounts`, then `RatingType` → `Rating`, `RatingTargetRole` → `FeedbackTargetRole`
- [ ] Task 2.2: `Notification` → `PushNotification`, `EventType` → `PushNotificationEventType`
- [ ] Task 2.3: `InventoryStatusType` → `InventoryStatus`, `PriceGuideType` → `PriceGuideSource`, `Name` → `PersonName`
- [ ] Task 2.4: `IBricklinkRequestHandler` → `IRequestHandler`, `BricklinkApiResourceType` → `ApiResourceType`; `OnRequestAsync` → `BeforeRequestAsync`, `ct` → `cancellationToken`
- [ ] Task 2.5: Review, format, build, test

### Phase 3 — Property renames
Goal: all public properties follow the Naming rules; wire names unchanged.
- [ ] Task 3.1: Id properties → `Id`; `UpdateShipping.MethodId` → `ShippingMethodId`
- [ ] Task 3.2: Date properties → `…On`
- [ ] Task 3.3: Rating count and Cost properties
- [ ] Task 3.4: Inventory and catalog properties
- [ ] Task 3.5: Exception members
- [ ] Task 3.6: Verify `WireMap` characterization tests and JSON property names are unchanged
- [ ] Task 3.7: Review, format, build, test

### Phase 4 — Client methods, parameters, demos, tests
Goal: `IBricklinkClient` surface is final; demos and tests use the new names.
- [ ] Task 4.1: Plural `Get<Things>Async` renames, catalog and item-mapping methods, `ReplyToFeedbackAsync`
- [ ] Task 4.2: Parameters → `itemNumber`, `shippingMethodId`
- [ ] Task 4.3: Update `BricklinkSharp.Demos/` and rename affected tests
- [ ] Task 4.4: Review, format, build, test

### Phase 5 — Docs and migration guide
Goal: documentation is complete for release.
- [ ] Task 5.1: `CHANGELOG.md` `## Unreleased`: old → new table under Changed/Removed
- [ ] Task 5.2: README: update all examples and add a migration section
- [ ] Task 5.3: `CONTEXT.md` check against final names
- [ ] Task 5.4: Final `naming-specialist` pass over the whole public surface; build, test

## Risks & alternatives
- **Collision order:** `Rating` swaps meaning in one release. Mitigated by strict rename order and by a changelog note calling it out.
- **Wire regressions:** renaming a property could drop its `JsonPropertyName`. Mitigated by the characterization tests in Phase 3.
- **Silent behavior change:** `Rating` names a different type than before, so old code could compile. It won't in practice (`NumOfPraises` etc. are gone), but the changelog flags it.
- **Rejected: `[Obsolete]` forwarders.** Doubles the public surface in a breaking release; enums and records cannot be aliased cleanly.
- **Rejected: removing all base classes.** `CouponBase` (12 props, 3 users) and `InventoryBase` (18 props) would be duplicated, plus shared validation.

## Out of scope
- Replacing `Tier*1..3` with a collection.
- Reshaping `GetOrderItemsAsync` (`List<OrderItem[]>`).
- Decoupling `BricklinkClientConfiguration` from `BricklinkCredentials`.
- The release itself (version bump in `Directory.Build.props`, renaming `Unreleased`, tag).
