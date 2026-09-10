**📋 SOD - Filipino Kitchen Clash (Sep 10, 2026)**
**Today's Focus**
- Add white bowl 2-8 as draggable items to brown tray snap zones
- Fix white bowl 1 snap issue (can't un-click after snapping)
- Fix pl adobo 1 alignment on brown tray 1
- Fix 1 sisig plate 3rd clone not spawning (broken respawner chain)

---

**📋 EOD - Filipino Kitchen Clash (Sep 10, 2026)**
**Updates**
- Fixed white bowl 1 snap issue — WorldDrag.SnapToPosition now keeps z=-0.1 after snap, preventing trigger collider from blocking OnMouseDown raycast
- Added white bowl 2 draggable — snaps to BrownTraySnapZone7/8/9 (brown tray 7/8/9), max 4, clone chain
- Added white bowl 3 draggable — snaps to BrownTraySnapZone10/11/12 (brown tray 10/11/12), max 4, clone chain
- Added white bowl 4 draggable — snaps to BrownTraySnapZone13/14/15 (brown tray 13/14/15), max 4, clone chain
- Added white bowl 5 draggable — snaps to BrownTraySnapZone16/17/18 (brown tray 16/17/18), max 4, clone chain
- Added white bowl 6 draggable — snaps to BrownTraySnapZone19/20/21 (brown tray 19/20/21), max 4, clone chain
- Added white bowl 7 draggable — snaps to BrownTraySnapZone22/23/24 (brown tray 22/23/24), max 4, clone chain
- Added white bowl 8 draggable — snaps to BrownTraySnapZone25/26/27 (brown tray 25/26/27), max 4, clone chain
- Fixed BrownTraySnapZone1 position — adjusted from (-0.0025, -1.023) to (-0.005, -0.925) to match brown tray 1 visual center
- Fixed 1 sisig plate respawner chain — added prefabRespawner on prefab (was missing, breaking clone chain), increased maxPlates from 3 to 4 (allows 3 clones)
- Added debug log to WorldDrag.OnMouseDown — outputs game object name, position, isSnapping, collider state

**Files Changed**
- `DragSetup.cs` — added BrownTraySnapZone7-27, white bowl 2-8 item data + respawner blocks, fixed BrownTraySnapZone1 position, fixed sisig plate respawner chain
- `WorldDrag.cs` — SnapToPosition forces z=-0.1 after snap, added debug log to OnMouseDown

**Pushed to:** `dev` branch (commit: 3ecf818)

---

**Ongoing Initiatives**
- Test all 8 white bowls drag-and-snap to corresponding brown trays in Unity
- Test sisig plate 3-clone chain (each clone should auto-spawn next when snapped)
- Implement remaining dish types (Adobo, Sinigang, Sisig, Balbacua, Bicol Express, Chicken Pochero, Dinuguan, Pinakbet)
- Customer order ticket system — random dish per customer
- Cooking mechanics per dish — drag ingredients to pot/pan
- Kitchen tools attach to pot — renders on top
- Sprite swap on drag — bowl sinigang → white bowl
- Remove WorldDrag debug log after confirming fix works
