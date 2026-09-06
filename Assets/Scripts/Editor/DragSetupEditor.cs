using UnityEngine;
using UnityEditor;

public class DragSetupEditor : MonoBehaviour
{
    [MenuItem("Kitchen Clash/Setup Draggable Items")]
    static void SetupDraggableItems()
    {
        string[] draggableNames = new string[]
        {
            "fork",
            "kt Slotted Turner",
            "kt Spatula",
            "kt tongs",
            "kt Soup Ladle",
            "pan",
            "pot with cover",
            "trash cab",
            "sisig plate",
            "bowl sinigang",
            "plates"
        };

        int count = 0;
        foreach (string itemName in draggableNames)
        {
            GameObject obj = GameObject.Find(itemName);
            if (obj != null)
            {
                if (obj.GetComponent<BoxCollider2D>() == null)
                {
                    obj.AddComponent<BoxCollider2D>();
                }

                if (obj.GetComponent<WorldDrag>() == null)
                {
                    obj.AddComponent<WorldDrag>();
                }

                count++;
                Debug.Log("[DragSetup] Added components to: " + itemName);
            }
            else
            {
                Debug.LogWarning("[DragSetup] Object not found: " + itemName);
            }
        }

        Debug.Log("[DragSetup] Setup complete! " + count + " items configured.");
    }

    [MenuItem("Kitchen Clash/Setup Snap Zones")]
    static void SetupSnapZones()
    {
        string parentName = "SnapZones";
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
        {
            parent = new GameObject(parentName);
        }

        SnapZoneData[] zones = new SnapZoneData[]
        {
            new SnapZoneData("PanSnapZone", new Vector2(-2.0f, -1.5f), 1.5f),
            new SnapZoneData("StoveSnapZone", new Vector2(-2.90f, -1.46f), 1.5f),
            new SnapZoneData("TrashSnapZone", new Vector2(-8.01f, -3.81f), 1.5f),
            new SnapZoneData("ServingSnapZone1", new Vector2(-0.09f, -3.29f), 1.0f),
            new SnapZoneData("ServingSnapZone2", new Vector2(0.97f, -3.29f), 1.0f),
            new SnapZoneData("ServingSnapZone3", new Vector2(1.98f, -3.22f), 1.0f)
        };

        foreach (SnapZoneData zoneData in zones)
        {
            GameObject zoneObj = GameObject.Find(zoneData.name);
            if (zoneObj == null)
            {
                zoneObj = new GameObject(zoneData.name);
                zoneObj.transform.SetParent(parent.transform);
            }

            zoneObj.transform.position = new Vector3(zoneData.position.x, zoneData.position.y, 0f);

            CircleCollider2D collider = zoneObj.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = zoneObj.AddComponent<CircleCollider2D>();
            }
            collider.isTrigger = true;
            collider.radius = 0.5f;

            SnapZone snapZone = zoneObj.GetComponent<SnapZone>();
            if (snapZone == null)
            {
                snapZone = zoneObj.AddComponent<SnapZone>();
            }
            snapZone.snapRadius = zoneData.radius;

            SpriteRenderer sr = zoneObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = zoneObj.AddComponent<SpriteRenderer>();
            }
            sr.color = new Color(0f, 1f, 0f, 0.3f);
            sr.sortingOrder = -1;

            Debug.Log("[DragSetup] Created snap zone: " + zoneData.name + " at " + zoneData.position);
        }

        Debug.Log("[DragSetup] Snap zones setup complete!");
    }

    [MenuItem("Kitchen Clash/Assign Snap Zones to Items")]
    static void AssignSnapZones()
    {
        SnapZone panZone = FindSnapZone("PanSnapZone");
        SnapZone stoveZone = FindSnapZone("StoveSnapZone");
        SnapZone trashZone = FindSnapZone("TrashSnapZone");
        SnapZone serving1 = FindSnapZone("ServingSnapZone1");
        SnapZone serving2 = FindSnapZone("ServingSnapZone2");
        SnapZone serving3 = FindSnapZone("ServingSnapZone3");

        AssignZone("fork", new SnapZone[] { panZone });
        AssignZone("kt Slotted Turner", new SnapZone[] { panZone });
        AssignZone("kt Spatula", new SnapZone[] { panZone });
        AssignZone("kt tongs", new SnapZone[] { panZone });
        AssignZone("kt Soup Ladle", new SnapZone[] { panZone });
        AssignZone("pan", new SnapZone[] { stoveZone });
        AssignZone("pot with cover", new SnapZone[] { stoveZone });
        AssignZone("trash cab", new SnapZone[] { trashZone });
        AssignZone("sisig plate", new SnapZone[] { serving2 });
        AssignZone("bowl sinigang", new SnapZone[] { serving3 });
        AssignZone("plates", new SnapZone[] { serving1 });

        Debug.Log("[DragSetup] Snap zones assigned to all items!");
    }

    static SnapZone FindSnapZone(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
            return obj.GetComponent<SnapZone>();
        return null;
    }

    static void AssignZone(string itemName, SnapZone[] zones)
    {
        GameObject obj = GameObject.Find(itemName);
        if (obj != null)
        {
            WorldDrag drag = obj.GetComponent<WorldDrag>();
            if (drag != null)
            {
                drag.SetSnapZones(zones);
                Debug.Log("[DragSetup] Assigned zones to: " + itemName);
            }
        }
    }

    struct SnapZoneData
    {
        public string name;
        public Vector2 position;
        public float radius;

        public SnapZoneData(string name, Vector2 position, float radius)
        {
            this.name = name;
            this.position = position;
            this.radius = radius;
        }
    }
}
