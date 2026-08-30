using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    GeneralSnapPoint[] snapPoints;
    public GeneralSnapPoint snapPoint;
    private InventoryManager inventoryManager;
    private WashFunction washFunction;
    private ChoppingBoardFunction choppingBoardFunction;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        snapPoints = FindObjectsOfType<GeneralSnapPoint>();
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); 
        washFunction = GameObject.Find("SinkSnapPoint").GetComponent<WashFunction>();
        choppingBoardFunction = GameObject.Find("ChoppingBoardSnapPoint").GetComponent<ChoppingBoardFunction>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        transform.localScale += new Vector3(0.2f, 0.2f, 1f); 

        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (point.GetComponent<RectTransform>().anchoredPosition == rectTransform.anchoredPosition)
            {
                point.isOccupied = false;
                inventoryManager.place[System.Array.IndexOf(inventoryManager.snapPoints, point)] = false; // Set the corresponding place value to false in the InventoryManager
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localScale += new Vector3(-0.2f, -0.2f, 1f); 

        // Check General Snap Points
        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (!point.isOccupied)
            {
                float distance = Vector2.Distance(
                    rectTransform.anchoredPosition,
                    point.GetComponent<RectTransform>().anchoredPosition
                );

                if (distance <= point.snapRadius)
                {

                    rectTransform.anchoredPosition = point.GetComponent<RectTransform>().anchoredPosition;
                    point.isOccupied = true;
                    snapPoint = point;
                    return;
                }
                else if (rectTransform.anchoredPosition == washFunction.GetComponent<RectTransform>().anchoredPosition ||
                     rectTransform.anchoredPosition == choppingBoardFunction.GetComponent<RectTransform>().anchoredPosition)
                {
                    snapPoint = point;
                    return;
                }
            }
        }

        if (snapPoint != null)
        {
            rectTransform.anchoredPosition = snapPoint.GetComponent<RectTransform>().anchoredPosition;
            snapPoint.isOccupied = true;
        }
    }
}