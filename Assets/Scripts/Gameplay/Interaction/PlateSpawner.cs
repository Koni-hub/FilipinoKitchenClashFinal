using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Sprite spawnSprite;
    public SnapZone[] spawnZones;

    private Camera mainCamera;
    private bool isDragging = false;
    private GameObject spawnedCopy;
    private SnapZone currentHighlighted;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (isDragging) return;

        isDragging = true;

        spawnedCopy = Instantiate(gameObject, transform.position, Quaternion.identity);
        spawnedCopy.name = "PlAdobo_Copy";

        SpriteRenderer sr = spawnedCopy.GetComponent<SpriteRenderer>();
        if (sr != null && spawnSprite != null)
        {
            sr.sprite = spawnSprite;
            sr.sortingOrder = 100;
        }

        PlateSpawner cloneSpawner = spawnedCopy.GetComponent<PlateSpawner>();
        if (cloneSpawner != null) Destroy(cloneSpawner);

        BoxCollider2D cloneCol = spawnedCopy.GetComponent<BoxCollider2D>();
        if (cloneCol != null) Destroy(cloneCol);
    }

    private void Update()
    {
        if (!isDragging || spawnedCopy == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        spawnedCopy.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

        UpdateHighlight();

        if (Input.GetMouseButtonUp(0))
        {
            FinishDrag();
        }
    }

    private void UpdateHighlight()
    {
        Vector2 copyPos = new Vector2(spawnedCopy.transform.position.x, spawnedCopy.transform.position.y);
        SnapZone nearest = null;
        float nearestDist = Mathf.Infinity;

        if (spawnZones != null)
        {
            foreach (SnapZone zone in spawnZones)
            {
                if (zone == null || zone.isOccupied) continue;
                float dist = Vector2.Distance(copyPos, zone.snapPosition);
                if (dist <= zone.snapRadius && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = zone;
                }
            }
        }

        if (currentHighlighted != null && currentHighlighted != nearest)
        {
            currentHighlighted.ShowHighlight(false);
        }

        if (nearest != null)
        {
            nearest.ShowHighlight(true);
            currentHighlighted = nearest;
        }
        else
        {
            currentHighlighted = null;
        }
    }

    private void FinishDrag()
    {
        isDragging = false;

        if (currentHighlighted != null)
        {
            currentHighlighted.ShowHighlight(false);
            currentHighlighted = null;
        }

        if (spawnedCopy == null) return;

        Vector2 copyPos = new Vector2(spawnedCopy.transform.position.x, spawnedCopy.transform.position.y);
        SnapZone nearestZone = null;
        float nearestDist = Mathf.Infinity;

        if (spawnZones != null)
        {
            foreach (SnapZone zone in spawnZones)
            {
                if (zone == null || zone.isOccupied) continue;
                float dist = Vector2.Distance(copyPos, zone.snapPosition);
                if (dist <= zone.snapRadius && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestZone = zone;
                }
            }
        }

        if (nearestZone != null)
        {
            spawnedCopy.transform.position = new Vector3(
                nearestZone.snapPosition.x,
                nearestZone.snapPosition.y,
                0f
            );
            nearestZone.isOccupied = true;
        }
        else
        {
            Destroy(spawnedCopy);
        }

        spawnedCopy = null;
    }
}
