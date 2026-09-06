using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSetup : MonoBehaviour
{
    private static DragSetup instance;

    private Dictionary<string, SnapZone> snapZones = new Dictionary<string, SnapZone>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        GameObject setupObj = new GameObject("DragSetup");
        setupObj.AddComponent<DragSetup>();
        DontDestroyOnLoad(setupObj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        StartCoroutine(SetupDelayed());
    }

    private IEnumerator SetupDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        LoadSprites();
        SetupSnapZones();
        SetupDraggableBoxes();
    }

    private void LoadSprites()
    {
        Sprite whiteBowl = Resources.Load<Sprite>("Art/CookingArea/white bowl");
        Sprite plAdobo = Resources.Load<Sprite>("Art/CookingArea/pl adobo");

        if (whiteBowl != null)
            spriteCache["white bowl"] = whiteBowl;

        if (plAdobo != null)
            spriteCache["pl adobo"] = plAdobo;
    }

    private void SetupSnapZones()
    {
        GameObject zonesParent = new GameObject("SnapZones");

        // Kitchen tool snap zones (individual positions)
        CreateSnapZone(zonesParent, "ForkSnapZone", new Vector2(-2.078f, -2.238f), 0.8f);
        CreateSnapZone(zonesParent, "TurnerSnapZone", new Vector2(-0.08f, -2.26f), 0.8f);
        CreateSnapZone(zonesParent, "LadleSnapZone", new Vector2(-0.661f, -2.099f), 0.8f);
        CreateSnapZone(zonesParent, "SpatulaSnapZone", new Vector2(0.51f, -2.26f), 0.8f);
        CreateSnapZone(zonesParent, "TongsSnapZone", new Vector2(-1.354f, -2.332f), 0.8f);

        // Other snap zones
        CreateSnapZone(zonesParent, "StoveSnapZone", new Vector2(-3.03f, -1.55f), 2.0f);
        CreateSnapZone(zonesParent, "PotSnapZone", new Vector2(-2.85f, -1.3f), 1.5f);
        CreateSnapZone(zonesParent, "ServingSnapZone1", new Vector2(-0.09f, -3.29f), 1.0f);
        CreateSnapZone(zonesParent, "SisigSnapZone1", new Vector2(0.915f, -0.856f), 0.8f);
        CreateSnapZone(zonesParent, "SisigSnapZone2", new Vector2(0.915f, -1.529f), 0.8f);
        CreateSnapZone(zonesParent, "SisigSnapZone3", new Vector2(0.915f, -2.195f), 0.8f);

        // 27 brown tray snap zones
        float[,] trayPositions = new float[,]
        {
            {-0.103f, -0.843f}, {-0.094f, -1.473f}, {-0.087f, -2.136f},
            {1.931f, -0.871f}, {1.917f, -1.551f}, {1.924f, -2.209f},
            {2.919f, -0.870f}, {2.919f, -1.566f}, {2.915f, -2.215f},
            {3.916f, -0.853f}, {3.917f, -1.510f}, {3.919f, -2.183f},
            {4.985f, -0.850f}, {4.990f, -1.530f}, {4.990f, -2.185f},
            {5.874f, -0.853f}, {5.879f, -1.531f}, {5.872f, -2.213f},
            {6.740f, -0.851f}, {6.730f, -1.510f}, {6.723f, -2.197f},
            {7.606f, -0.852f}, {7.594f, -1.538f}, {7.611f, -2.210f},
            {8.425f, -0.860f}, {8.420f, -1.550f}, {8.406f, -2.210f}
        };

        for (int i = 0; i < 27; i++)
        {
            string zoneName = "Tray_" + (i / 3) + "_" + (i % 3);
            Vector2 pos = new Vector2(trayPositions[i, 0], trayPositions[i, 1]);
            CreateSnapZone(zonesParent, zoneName, pos, 0.5f);
        }
    }

    private void CreateSnapZone(GameObject parent, string name, Vector2 position, float radius)
    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(parent.transform);
        zoneObj.transform.position = new Vector3(position.x, position.y, 0f);

        CircleCollider2D circleCollider = zoneObj.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 0.5f;

        BoxCollider2D boxCollider = zoneObj.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(radius * 2f, radius * 2f);

        SnapZone snapZone = zoneObj.AddComponent<SnapZone>();
        snapZone.snapRadius = radius;
        snapZone.snapPosition = position;

        SpriteRenderer sr = zoneObj.AddComponent<SpriteRenderer>();
        sr.color = new Color(0f, 1f, 0f, 0.3f);
        sr.sortingOrder = -1;

        snapZones[name] = snapZone;
    }

    private void SetupDraggableBoxes()
    {
        // Get all tray zone names
        List<string> allTrayZones = new List<string>();
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                allTrayZones.Add("Tray_" + col + "_" + row);
            }
        }
        string[] trayZoneArray = allTrayZones.ToArray();

        List<ItemData> items = new List<ItemData>();

        // Kitchen tools - snap to individual zones + stove area + attach to pot
        items.Add(new ItemData("fork", new string[] { "ForkSnapZone", "StoveSnapZone", "PotSnapZone" }, 0.3f, 1.1f, false, "", true));
        items.Add(new ItemData("kt Slotted Turner", new string[] { "TurnerSnapZone", "StoveSnapZone", "PotSnapZone" }, 0.5f, 1.2f, false, "", true));
        items.Add(new ItemData("kt Soup Ladle", new string[] { "LadleSnapZone", "StoveSnapZone", "PotSnapZone" }, 0.6f, 1.4f, false, "", true));
        items.Add(new ItemData("kt Spatula", new string[] { "SpatulaSnapZone", "StoveSnapZone", "PotSnapZone" }, 0.6f, 1.1f, false, "", true));
        items.Add(new ItemData("kt tongs", new string[] { "TongsSnapZone", "StoveSnapZone", "PotSnapZone" }, 0.5f, 1.0f, false, "", true));

        // Cookware - snap to stove
        items.Add(new ItemData("pan", new string[] { "StoveSnapZone" }, 1.5f, 0.8f, false, ""));
        items.Add(new ItemData("pot with cover", new string[] { "PotSnapZone" }, 1.1f, 0.9f, false, ""));

        // Sisig plate - snap to sisig zones
        items.Add(new ItemData("sisig plate", new string[] { "SisigSnapZone1", "SisigSnapZone2", "SisigSnapZone3" }, 0.8f, 0.5f, false, ""));

        // Plates - swaps to pl adobo, snaps to all tray zones
        items.Add(new ItemData("plates", trayZoneArray, 0.6f, 0.5f, true, "pl adobo"));

        // All 8 bowl sinigang - swaps to white bowl, snaps to all tray zones
        items.Add(new ItemData("bowl sinigang", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (1)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (2)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (3)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (4)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (5)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (6)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (7)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));

        GameObject boxesParent = new GameObject("DragBoxes");

        foreach (ItemData item in items)
        {
            GameObject original = GameObject.Find(item.itemName);
            if (original == null)
            {
                continue;
            }

            Vector3 originalPos = original.transform.position;

            GameObject box = new GameObject("DragBox_" + item.itemName);
            box.transform.SetParent(boxesParent.transform);
            box.transform.position = originalPos;

            BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(item.boxWidth, item.boxHeight);

            WorldDrag drag = box.AddComponent<WorldDrag>();

            if (item.attachToParent)
            {
                drag.attachToParent = true;
            }

            if (item.swapSprite && !string.IsNullOrEmpty(item.dragSpriteName))
            {
                if (spriteCache.ContainsKey(item.dragSpriteName))
                {
                    drag.swapOnDrag = true;
                    drag.dragSprite = spriteCache[item.dragSpriteName];
                }
            }

            SnapZone[] zones = GetSnapZones(item.snapZoneNames);
            drag.SetSnapZones(zones);

            original.transform.SetParent(box.transform);
            original.transform.localPosition = Vector3.zero;
        }
    }

    private SnapZone[] GetSnapZones(string[] zoneNames)
    {
        List<SnapZone> zones = new List<SnapZone>();
        foreach (string name in zoneNames)
        {
            if (snapZones.ContainsKey(name))
            {
                zones.Add(snapZones[name]);
            }
        }
        return zones.ToArray();
    }

    private struct ItemData
    {
        public string itemName;
        public string[] snapZoneNames;
        public float boxWidth;
        public float boxHeight;
        public bool swapSprite;
        public string dragSpriteName;
        public bool attachToParent;

        public ItemData(string name, string[] zones, float width, float height, bool swap, string spriteName, bool attach = false)
        {
            itemName = name;
            snapZoneNames = zones;
            boxWidth = width;
            boxHeight = height;
            swapSprite = swap;
            dragSpriteName = spriteName;
            attachToParent = attach;
        }
    }
}
