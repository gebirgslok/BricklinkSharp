# 0001: Clean break for 2.0.0, no `[Obsolete]` forwarders

Status: accepted (RFC 0004)

## Context
Public names drifted from the vocabulary in `CONTEXT.md` (`Rating` vs `RatingType`, `*Base` classes, two identical direction enums, mixed plural and date-property shapes). 2.0.0 is a breaking release anyway.

## Decision
Rename public types, members, properties and parameters directly. Do not keep `[Obsolete]` forwarders for the old names. JSON wire names do not change.

## Consequences
- Consumers must migrate by hand; the old → new table in `CHANGELOG.md` and a README migration section are the support path.
- The public surface does not double in size for one release.
- Enums and records cannot be aliased cleanly, so forwarders could not be complete anyway.
- Rejected: forwarders for every renamed member.
