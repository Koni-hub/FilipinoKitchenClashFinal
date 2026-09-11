SOD - Filipino Kitchen Clash (Sep 10, 2026)
Today's Focus
Fix white bowl 1 un-clickable after snapping
Add white bowl 2 draggable to brown tray 7, 8, 9
Add white bowl 3 draggable to brown tray 10, 11, 12
Add white bowl 4 draggable to brown tray 13, 14, 15
Add white bowl 5 draggable to brown tray 16, 17, 18
Add white bowl 6 draggable to brown tray 19, 20, 21
Add white bowl 7 draggable to brown tray 22, 23, 24
Add white bowl 8 draggable to brown tray 25, 26, 27
Fix pl adobo 1 not centered on brown tray 1
Fix 1 sisig plate clone chain
Commit and push to dev

---

EOD - Filipino Kitchen Clash (Sep 10, 2026)
Updates
White bowl un-click fix — SnapToPosition keeps DragBox at z=-0.1 after snap
BrownTraySnapZone1 position adjusted to match brown tray 1 center
White bowl 2-8 draggable — each snaps to its brown tray column, max 4, clone chain
1 sisig plate respawner chain fixed — added missing prefabRespawner on prefab
1 sisig plate maxPlates increased from 3 to 4 (allows 3 clones)
Added Debug.Log to WorldDrag.OnMouseDown for troubleshooting

Ongoing Initiatives
Test all 8 white bowl draggables in Unity
Test sisig plate clone chain — 3 clones should spawn automatically
Implement remaining dish types (Adobo, Sinigang, Sisig, Balbacua, Bicol Express, Chicken Pochero, Dinuguan, Pinakbet)
Customer order ticket system — random dish per customer
Cooking mechanics per dish — drag ingredients to pot/pan
Kitchen tools attach to pot — renders on top
Sprite swap on drag — bowl sinigang → white bowl
Remove Debug.Log from WorldDrag.OnMouseDown after testing
