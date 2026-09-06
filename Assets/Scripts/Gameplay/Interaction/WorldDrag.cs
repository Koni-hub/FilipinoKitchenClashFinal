using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDrag : MonoBehaviour
{
    [Header("Drag Settings")]
    public float snapSpeed = 10f;

    [Header("Snap Zones")]
    public SnapZone[] snapZones;

    [Header("Drag Sprite Swap")]
    public bool swapOnDrag = false;
    public Sprite dragSprite;

    [Header("Attach Settings")]
    public bool attachToParent = false;

    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 offset;
    private Camera mainCamera;
    private SnapZone currentSnapZone;
    private Vector3 originalPosition;
    private SpriteRenderer childSpriteRenderer;
    private Sprite originalSprite;
    private int originalSortingOrder;
    private bool isAttached = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        originalPosition = transform.position;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            childSpriteRenderer = sr;
            originalSprite = sr.sprite;
        }
    }

    private void OnMouseDown()
    {
        if (isSnapping) return;

        if (isAttached)
        {
            DetachFromParent();
        }

        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();

        if (childSpriteRenderer != null)
        {
            originalSortingOrder = childSpriteRenderer.sortingOrder;
            childSpriteRenderer.sortingOrder = 100;

            if (swapOnDrag && dragSprite != null)
            {
                childSpriteRenderer.sprite = dragSprite;
            }
        }

        if (currentSnapZone != null)
        {
            currentSnapZone.isOccupied = false;
            currentSnapZone = null;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = GetMouseWorldPosition();
        transform.position = mousePos + offset;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.sortingOrder = originalSortingOrder;

            if (swapOnDrag && originalSprite != null)
            {
                childSpriteRenderer.sprite = originalSprite;
            }
        }

        SnapToNearestZone();
    }

    private void SnapToNearestZone()
    {
        if (snapZones == null || snapZones.Length == 0)
        {
            ReturnToOriginal();
            return;
        }

        SnapZone nearestZone = null;
        float nearestDistance = Mathf.Infinity;

        foreach (SnapZone zone in snapZones)
        {
            if (zone == null || zone.isOccupied) continue;

            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(zone.snapPosition.x, zone.snapPosition.y)
            );

            if (distance <= zone.snapRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestZone = zone;
            }
        }

        if (nearestZone != null)
        {
            currentSnapZone = nearestZone;
            currentSnapZone.isOccupied = true;

            if (attachToParent && nearestZone.gameObject.name == "PotSnapZone")
            {
                StartCoroutine(AttachToPot(currentSnapZone.snapPosition));
            }
            else
            {
                StartCoroutine(SnapToPosition(currentSnapZone.snapPosition));
            }
        }
        else
        {
            ReturnToOriginal();
        }
    }

    private IEnumerator AttachToPot(Vector3 targetPosition)
    {
        isSnapping = true;

        while (Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(targetPosition.x, targetPosition.y)
        ) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                snapSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;

        GameObject pot = GameObject.Find("pot with cover");
        if (pot != null)
        {
            SpriteRenderer potSR = pot.GetComponentInChildren<SpriteRenderer>();
            if (potSR != null && childSpriteRenderer != null)
            {
                childSpriteRenderer.sortingOrder = potSR.sortingOrder + 1;
            }

            transform.SetParent(pot.transform);
            isAttached = true;
        }

        isSnapping = false;
    }

    private void DetachFromParent()
    {
        transform.SetParent(null);
        isAttached = false;

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.sortingOrder = originalSortingOrder;
        }
    }

    private IEnumerator SnapToPosition(Vector3 targetPosition)
    {
        isSnapping = true;

        while (Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(targetPosition.x, targetPosition.y)
        ) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                snapSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        isSnapping = false;
    }

    private void ReturnToOriginal()
    {
        StartCoroutine(SnapToPosition(originalPosition));
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void SetSnapZones(SnapZone[] zones)
    {
        snapZones = zones;
    }

    public void ResetToOriginal()
    {
        if (isAttached)
        {
            DetachFromParent();
        }

        if (currentSnapZone != null)
        {
            currentSnapZone.isOccupied = false;
            currentSnapZone = null;
        }

        StopAllCoroutines();
        isDragging = false;
        isSnapping = false;
        transform.position = originalPosition;

        if (childSpriteRenderer != null && originalSprite != null)
        {
            childSpriteRenderer.sprite = originalSprite;
        }
    }
}
