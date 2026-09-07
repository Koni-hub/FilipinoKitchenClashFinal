using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateRespawner : MonoBehaviour
{
    public GameObject platePrefab;
    public Vector3 spawnPosition;
    public int maxPlates = 3;

    private static Dictionary<string, int> totalSpawned = new Dictionary<string, int>();
    private string plateKey;

    private void OnEnable()
    {
        plateKey = platePrefab != null ? platePrefab.name : gameObject.name;

        if (!totalSpawned.ContainsKey(plateKey))
        {
            totalSpawned[plateKey] = 0;
        }

        if (!gameObject.name.Contains("Clone") && !gameObject.name.Contains("Prefab"))
        {
            totalSpawned[plateKey] = 1;
        }

        WorldDrag drag = GetComponent<WorldDrag>();
        if (drag != null)
        {
            drag.OnSnapped += OnPlaced;
        }
    }

    private void OnDisable()
    {
        WorldDrag drag = GetComponent<WorldDrag>();
        if (drag != null)
        {
            drag.OnSnapped -= OnPlaced;
        }
    }

    private void OnPlaced()
    {
        if (totalSpawned[plateKey] < maxPlates && platePrefab != null)
        {
            totalSpawned[plateKey]++;
            GameObject clone = Instantiate(platePrefab, spawnPosition, Quaternion.identity);
            clone.SetActive(true);
        }
    }

    public static void ResetCounts()
    {
        totalSpawned.Clear();
    }
}
