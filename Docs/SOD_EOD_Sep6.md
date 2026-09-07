**📋 SOD - Filipino Kitchen Clash (Sep 6, 2026)**
**Today's Focus**
- Customer system with patience meters (60s per customer, 30s cooldown)
- Drag-and-drop kitchen items (13 draggable items)
- Snap zones for all items (34 total)
- Sprite swap on drag (bowl sinigang → white bowl, plates → pl adobo)
- Kitchen tools attach to pot (renders on top)
- Kitchen tools can snap to stove area
- Invisible BoxCollider2D on all drop zones
- Commit and push to dev

---

**📋 EOD - Filipino Kitchen Clash (Sep 6, 2026)**
**Updates**
- Customer system — random dish (Adobo/Sinigang/Sisig), random customer sprite (1-4), 60s patience, 30s cooldown per slot
- Patience meters — adobo (6 stages), sinigang (5 stages), sisig (6 stages)
- Drag-and-drop system — 13 draggable items with WorldDrag.cs
- 34 snap zones — 5 kitchen tool zones, 2 stove zones, 1 serving zone, 3 sisig zones, 27 brown tray zones
- Sprite swap — bowl sinigang → white bowl, plates → pl adobo
- Kitchen tools attach to pot — renders on top (sortingOrder = pot + 1)
- Kitchen tools can snap to stove area (StoveSnapZone + PotSnapZone)
- Invisible BoxCollider2D on all drop zones for better detection
- Fixed compiler error (nearestZone.zoneName → nearestZone.gameObject.name)

**Files Changed**
- `Customer.cs`, `CustomerManager.cs`, `WindowSlot.cs`
- `WorldDrag.cs`, `SnapZone.cs`, `DragSetup.cs`

**Pushed to:** `dev` branch (commit: 37ea80a)

---

**Ongoing Initiatives**
- Catch game UI/gameplay polish
- Customer order ticket system
- More dish types (Balbacua, Bicol Express, Chicken Pochero, etc.)
- Cooking mechanics per dish
- PR feat/customer-system → dev
