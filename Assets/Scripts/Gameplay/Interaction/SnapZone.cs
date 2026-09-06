using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapZone : MonoBehaviour
{
    [Header("Snap Settings")]
    public Vector2 snapPosition;
    public float snapRadius = 1.0f;
    public bool isOccupied = false;

    [Header("Visual Feedback")]
    public SpriteRenderer highlightRenderer;
    public float highlightScale = 1.2f;
    public Color highlightColor = Color.green;
    public Color normalColor = Color.white;

    private Vector3 originalScale;
    private bool isHovering = false;

    private void Awake()
    {
        snapPosition = transform.position;

        if (highlightRenderer == null)
            highlightRenderer = GetComponent<SpriteRenderer>();

        if (highlightRenderer != null)
        {
            originalScale = highlightRenderer.transform.localScale;
            highlightRenderer.color = normalColor;
        }
    }

    private void OnMouseEnter()
    {
        if (isOccupied) return;

        isHovering = true;
        ShowHighlight(true);
    }

    private void OnMouseExit()
    {
        isHovering = false;
        ShowHighlight(false);
    }

    private void ShowHighlight(bool show)
    {
        if (highlightRenderer == null) return;

        if (show)
        {
            highlightRenderer.color = highlightColor;
            highlightRenderer.transform.localScale = originalScale * highlightScale;
        }
        else
        {
            highlightRenderer.color = normalColor;
            highlightRenderer.transform.localScale = originalScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(snapPosition, snapRadius);
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;

        if (!occupied && isHovering)
        {
            ShowHighlight(true);
        }
    }

    public bool IsAvailable()
    {
        return !isOccupied;
    }

    public float GetDistance(Vector2 position)
    {
        return Vector2.Distance(position, snapPosition);
    }
}
