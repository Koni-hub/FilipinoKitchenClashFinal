using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralSnapPoint : MonoBehaviour
{
    public float snapRadius = 50f;
    public bool isOccupied = false;
    public GameObject selectedShader;
    Canvas canvas;
    private Vector3 originalScale;

    void Start()
    {
        canvas = GameObject.Find("NewCanvas").GetComponent<Canvas>();
        originalScale = selectedShader.transform.localScale;
    }

    public void OnHoverEnter()
    {
        selectedShader.transform.localScale = originalScale * 1.3f;
    }

    public void OnHoverExit()
    {
        selectedShader.transform.localScale = originalScale;
    }

    public void ConvertScreenToLocalPoint(RectTransform item, RectTransform spawnParent)
    {
        RectTransform snapPointRect = GetComponent<RectTransform>();

        // Convert snap point position to Canvas local space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera, 
            snapPointRect.position
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            spawnParent,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        item.anchoredPosition = localPoint;
    }
}
