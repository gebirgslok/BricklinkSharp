# BricklinkSharp

A typed client for the BrickLink marketplace API, used by sellers and tools to read the catalog and manage their store, orders, feedback and coupons.

## Language

### Catalog

**Item**:
Anything identified by an Item Type plus an item number, such as `3003` (part) or `6090-1` (set).
_Avoid_: Product, piece

**Item Type**:
The kind of an Item: Minifig, Part, Set, Book, Gear, Catalog, Instruction, Unsorted Lot or Original Box.
_Avoid_: Category (a different concept)

**Catalog Item**:
The full catalog entry of an Item, including name, weight and dimensions.
_Avoid_: Product details

**Category**:
A BrickLink catalog grouping that Catalog Items belong to.
_Avoid_: Item Type

**Color**:
A BrickLink color, identified by a color ID, in which a Part comes.
_Avoid_: Colour

**Known Color**:
A Color in which a given Item exists, with its quantity.
_Avoid_: Available color

**Superset**:
A set or minifigure that contains a given Item.
_Avoid_: Parent, appears-in

**Subsets**:
The parts list of a set, minifigure or other Item that contains parts.
_Avoid_: Inventory, parts list

**Price Guide**:
Sold or current-stock price statistics for an Item in a Color and Condition.
_Avoid_: Price history

**Price Guide Source**:
Whether a Price Guide reports sold items or current stock.
_Avoid_: Guide type

**Item Mapping**:
The link between an item number plus Color and a BrickLink Element ID.
_Avoid_: Element lookup

### Store

**Inventory**:
A seller's store listing of an Item on BrickLink.
_Avoid_: Stock, lot, listing

**Condition**:
Whether an Inventory or Order Item is new or used.
_Avoid_: State

**Order**:
A purchase between a buyer and a seller, tracked by status and payment status.
_Avoid_: Purchase, transaction

**Direction**:
Which side the user is on. Out means the user is the seller, In means the user is the buyer. Applies to Orders, Coupons and Feedback.
_Avoid_: Role, side

**Feedback**:
A rating and comment a member leaves on an Order. Leaving one is called posting it.
_Avoid_: Review

**Rating**:
The verdict of a single Feedback: Praise, Neutral or Complaint.
_Avoid_: Score, vote

**Feedback Target Role**:
Whether a Feedback rates the seller or the buyer of its Order.
_Avoid_: Recipient

**Coupon**:
A discount a seller offers to a specific buyer.
_Avoid_: Voucher, promo code

**Member Rating**:
A member's aggregated Feedback ratings, as counts of Praise, Neutral and Complaint.
_Avoid_: Reputation

**Shipping Method**:
A seller's configured way to ship Orders to a shipping area.
_Avoid_: Carrier

**Push Notification**:
An event BrickLink queues for the user, such as a new order or message.
_Avoid_: Webhook

### Access

**Credentials**:
The OAuth1 consumer key and secret plus token value and secret that authorize API calls, set globally or passed per call.
_Avoid_: API key, login
