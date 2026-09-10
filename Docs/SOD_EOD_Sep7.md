**📋 SOD - Filipino Kitchen Clash (Sep 7, 2026)**
**Today's Focus**
- Fix plate spawner system for pl adobo and sisig plate
- Add respawner logic (max 3 plates per type)
- Move pl adobo sprite to Resources folder for runtime loading
- Add hover highlight feedback on tray zones during drag
- Fix DragSetup scene loading (SceneManager.sceneLoaded callback)
- Commit and push to dev

---

**📋 EOD - Filipino Kitchen Clash (Sep 7, 2026)**
**Updates**
- Plate respawner system — pl adobo and sisig plate respawn after placing on tray
- Max 3 plates per type — stops spawning at 3
- Moved pl adobo sprite to Resources folder — fixed Resources.Load not finding sprite
- Hover highlight feedback — tray zones highlight green when dragged plate is nearby
- DragSetup scene loading fix — re-runs setup on SceneManager.sceneLoaded
- Removed PlateSpawner clone system — simplified to direct draggable approach
- HidePlates() — hides old plates object, replaced by pl adobo
- Fixed duplicate component cleanup — removes WorldDrag if added by DragSetupEditor
- ResetCounts() — resets plate counts on scene reload
- Fixed compiler error (nearestZone.zoneName → nearestZone.gameObject.name)

**Files Changed**
- `DragSetup.cs` — plate respawner setup, scene load callback, hide plates
- `PlateRespawner.cs` — new script for max 3 respawn logic
- `PlateSpawner.cs` — kept but no longer used (old clone system)
- `WorldDrag.cs` — OnSnapped callback for respawner
- `SnapZone.cs` — ShowHighlight made public
- `DragSetupEditor.cs` — removed plates from draggable list

**Pushed to:** `dev` branch (commit: 03640d6)

---

**Ongoing Initiatives**
- Test plate respawner in Unity — pl adobo and sisig plate max 3 working
- Implement remaining dish types (Adobo, Sinigang, Sisig, Balbacua, Bicol Express, Chicken Pochero, Dinuguan, Pinakbet)
- Customer order ticket system — random dish per customer
- Cooking mechanics per dish — drag ingredients to pot/pan
- Kitchen tools attach to pot — renders on top
- Sprite swap on drag — bowl sinigang → white bowl
- PR feat/plate-system → dev
