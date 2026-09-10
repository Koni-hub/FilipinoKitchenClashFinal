using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(SetupDelayed());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(SetupDelayed());
    }

    private IEnumerator SetupDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        snapZones.Clear();
        spriteCache.Clear();
        PlateRespawner.ResetCounts();

        LoadSprites();
        SetupSnapZones();
        HidePlates();
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
        GameObject existingZones = GameObject.Find("SnapZones");
        if (existingZones != null)
        {
            Destroy(existingZones);
        }

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

        // Brown tray snap zones (left side)
        CreateSnapZone(zonesParent, "BrownTraySnapZone1", new Vector2(-0.005f, -0.925f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone2", new Vector2(0.006f, -1.61f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone3", new Vector2(0.014f, -2.255f), 0.6f);

        // Brown tray snap zones (right side)
        CreateSnapZone(zonesParent, "BrownTraySnapZone4", new Vector2(2.042f, -0.95f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone5", new Vector2(2.057f, -1.61f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone6", new Vector2(2.057f, -2.254f), 0.6f);

        // Brown tray snap zones (right side 2)
        CreateSnapZone(zonesParent, "BrownTraySnapZone7", new Vector2(3.048f, -0.934f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone8", new Vector2(3.063f, -1.586f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone9", new Vector2(3.039f, -2.239f), 0.6f);

        // Brown tray snap zones (right side 3)
        CreateSnapZone(zonesParent, "BrownTraySnapZone10", new Vector2(4.018f, -0.898f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone11", new Vector2(4.024f, -1.571f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone12", new Vector2(4.035f, -2.244f), 0.6f);

        // Brown tray snap zones (right side 4)
        CreateSnapZone(zonesParent, "BrownTraySnapZone13", new Vector2(5.084f, -0.88f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone14", new Vector2(5.095f, -1.543f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone15", new Vector2(5.107f, -2.244f), 0.6f);

        // Brown tray snap zones (right side 5)
        CreateSnapZone(zonesParent, "BrownTraySnapZone16", new Vector2(5.936f, -0.876f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone17", new Vector2(5.936f, -1.533f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone18", new Vector2(5.936f, -2.229f), 0.6f);

        // Brown tray snap zones (right side 6)
        CreateSnapZone(zonesParent, "BrownTraySnapZone19", new Vector2(6.845f, -0.874f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone20", new Vector2(6.861f, -1.556f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone21", new Vector2(6.853f, -2.222f), 0.6f);

        // Brown tray snap zones (right side 7)
        CreateSnapZone(zonesParent, "BrownTraySnapZone22", new Vector2(7.722f, -0.868f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone23", new Vector2(7.744f, -1.533f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone24", new Vector2(7.738f, -2.21f), 0.6f);

        // Brown tray snap zones (right side 8)
        CreateSnapZone(zonesParent, "BrownTraySnapZone25", new Vector2(8.498f, -0.866f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone26", new Vector2(8.527f, -1.532f), 0.6f);
        CreateSnapZone(zonesParent, "BrownTraySnapZone27", new Vector2(8.527f, -2.198f), 0.6f);
    }

    private void CreateSnapZone(GameObject parent, string name, Vector2 position, float radius)    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(parent.transform);
        zoneObj.transform.position = new Vector3(position.x, position.y, 0f);

        CircleCollider2D circleCollider = zoneObj.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 0.5f;

        BoxCollider2D boxCollider = zoneObj.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(radius * 2f, radius * 2f);

        SnapZone snapZone = zoneObj.AddComponent<SnapZone>();
        snapZone.snapRadius = radius;
        snapZone.snapPosition = position;

        SpriteRenderer sr = zoneObj.AddComponent<SpriteRenderer>();
        sr.color = new Color(0f, 1f, 0f, 0.3f);
        sr.sortingOrder = -1;

        snapZones[name] = snapZone;
    }

    private void HidePlates()
    {
        GameObject platesObj = GameObject.Find("plates");
        if (platesObj != null)
        {
            platesObj.SetActive(false);
        }
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

        // Sisig plate - snap to sisig zones (max 3)
        items.Add(new ItemData("1 sisig plate", new string[] { "SisigSnapZone1", "SisigSnapZone2", "SisigSnapZone3" }, 0.8f, 0.5f, false, ""));

        // Pl adobo - snaps to all tray zones
        items.Add(new ItemData("pl adobo", trayZoneArray, 0.6f, 0.5f, false, ""));

        // Pl adobo 1 - snaps to brown tray 1, 2, 3 (max 4)
        string[] brownTrayZones = new string[] { "BrownTraySnapZone1", "BrownTraySnapZone2", "BrownTraySnapZone3" };
        items.Add(new ItemData("pl adobo 1", brownTrayZones, 0.6f, 0.5f, false, ""));

        // White bowl 1 - snaps to brown tray 4, 5, 6 (max 4)
        string[] brownTrayZones2 = new string[] { "BrownTraySnapZone4", "BrownTraySnapZone5", "BrownTraySnapZone6" };
        items.Add(new ItemData("white bowl 1", brownTrayZones2, 0.6f, 0.5f, false, ""));

        // White bowl 2 - snaps to brown tray 7, 8, 9 (max 4)
        string[] brownTrayZones3 = new string[] { "BrownTraySnapZone7", "BrownTraySnapZone8", "BrownTraySnapZone9" };
        items.Add(new ItemData("white bowl 2", brownTrayZones3, 0.6f, 0.5f, false, ""));

        // White bowl 3 - snaps to brown tray 10, 11, 12 (max 4)
        string[] brownTrayZones4 = new string[] { "BrownTraySnapZone10", "BrownTraySnapZone11", "BrownTraySnapZone12" };
        items.Add(new ItemData("white bowl 3", brownTrayZones4, 0.6f, 0.5f, false, ""));

        // White bowl 4 - snaps to brown tray 13, 14, 15 (max 4)
        string[] brownTrayZones5 = new string[] { "BrownTraySnapZone13", "BrownTraySnapZone14", "BrownTraySnapZone15" };
        items.Add(new ItemData("white bowl 4", brownTrayZones5, 0.6f, 0.5f, false, ""));

        // White bowl 5 - snaps to brown tray 16, 17, 18 (max 4)
        string[] brownTrayZones6 = new string[] { "BrownTraySnapZone16", "BrownTraySnapZone17", "BrownTraySnapZone18" };
        items.Add(new ItemData("white bowl 5", brownTrayZones6, 0.6f, 0.5f, false, ""));

        // White bowl 6 - snaps to brown tray 19, 20, 21 (max 4)
        string[] brownTrayZones7 = new string[] { "BrownTraySnapZone19", "BrownTraySnapZone20", "BrownTraySnapZone21" };
        items.Add(new ItemData("white bowl 6", brownTrayZones7, 0.6f, 0.5f, false, ""));

        // White bowl 7 - snaps to brown tray 22, 23, 24 (max 4)
        string[] brownTrayZones8 = new string[] { "BrownTraySnapZone22", "BrownTraySnapZone23", "BrownTraySnapZone24" };
        items.Add(new ItemData("white bowl 7", brownTrayZones8, 0.6f, 0.5f, false, ""));

        // White bowl 8 - snaps to brown tray 25, 26, 27 (max 4)
        string[] brownTrayZones9 = new string[] { "BrownTraySnapZone25", "BrownTraySnapZone26", "BrownTraySnapZone27" };
        items.Add(new ItemData("white bowl 8", brownTrayZones9, 0.6f, 0.5f, false, ""));

        // All 8 bowl sinigang - swaps to white bowl, snaps to all tray zones
        items.Add(new ItemData("bowl sinigang", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (1)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (2)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (3)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (4)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (5)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (6)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));
        items.Add(new ItemData("bowl sinigang (7)", trayZoneArray, 0.5f, 0.8f, true, "white bowl"));

        GameObject existingBoxes = GameObject.Find("DragBoxes");
        if (existingBoxes != null)
        {
            Destroy(existingBoxes);
        }

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
            box.transform.position = new Vector3(originalPos.x, originalPos.y, -0.1f);

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

            if (item.itemName == "pl adobo")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "PlAdobo_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = originalPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 3;
            }

            if (item.itemName == "1 sisig plate")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "SisigPlate_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "pl adobo 1")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "PlAdobo1_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 1")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl1_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 2")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl2_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 3")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl3_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 4")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl4_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 5")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl5_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 6")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl6_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 7")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl7_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }

            if (item.itemName == "white bowl 8")
            {
                box.SetActive(false);
                GameObject prefab = Instantiate(box);
                prefab.name = "WhiteBowl8_Prefab";
                prefab.transform.SetParent(boxesParent.transform);
                box.SetActive(true);

                Vector3 spawnPos = new Vector3(originalPos.x, originalPos.y, -0.1f);

                PlateRespawner respawner = box.AddComponent<PlateRespawner>();
                respawner.spawnPosition = spawnPos;
                respawner.platePrefab = prefab;
                respawner.maxPlates = 4;

                PlateRespawner prefabRespawner = prefab.AddComponent<PlateRespawner>();
                prefabRespawner.spawnPosition = spawnPos;
                prefabRespawner.platePrefab = prefab;
                prefabRespawner.maxPlates = 4;
            }
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
