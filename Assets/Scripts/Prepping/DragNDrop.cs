using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class DragNDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    GeneralSnapPoint[] snapPoints;
    public GeneralSnapPoint snapPoint;
    private MainInventorySlot[] inventorySlot;
    private MainInventory inventoryManager;
    private SinkFunction sinkFunction;
    private ChoppingBoardFunction choppingBoardFunction;
    public Vector2 spawnPositionCopy;
    public int snapPointIndex;
    public Vector2 sinkPosition;
    public Vector2 chopBoardPosition;
    public bool sinkPositionTaken = false;
    public bool chopBoardPositionTaken = false;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        snapPoints = FindObjectsOfType<GeneralSnapPoint>();
        inventorySlot = FindObjectsOfType<MainInventorySlot>();
        sinkFunction = GameObject.Find("SinkSnapPoint").GetComponent<SinkFunction>();
        choppingBoardFunction = GameObject.Find("ChoppingBoard").GetComponent<ChoppingBoardFunction>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.localScale += new Vector3(0.2f, 0.2f, 1f); 

        if (rectTransform.anchoredPosition != sinkPosition && rectTransform.anchoredPosition != chopBoardPosition)
            snapPoints[snapPointIndex].GetComponent<GeneralSnapPoint>().isOccupied = false;
        Debug.Log("Snap point is " + snapPoints[snapPointIndex].GetComponent<GeneralSnapPoint>().isOccupied);

        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (!point.isOccupied)
                point.selectedShader.SetActive(true);
        }

        if (!sinkFunction.isOccupied)
        {
            if (transform.tag == "LaurelLeaves")
                sinkFunction.selectedShader.SetActive(true);            
        }

        if (spawnPositionCopy == sinkPosition)
        {
            sinkFunction.isOccupied = false;
            sinkFunction.selectedShader.SetActive(true);       
        }

        if (spawnPositionCopy == chopBoardPosition)
        {
            choppingBoardFunction.isOccupied = false;
            choppingBoardFunction.selectedShader.SetActive(true);
        }
        
        if(!choppingBoardFunction.isOccupied)
        {
            if (transform.tag != "PorkKawaliMinced" && transform.tag != "LaurelLeavesCut" && 
                transform.tag != "GarlicMinced" && transform.tag != "OnionMinced" &&  
                transform.tag != "TomatoWedges" &&  
                transform.tag != "LaurelLeaves" || spawnPositionCopy == chopBoardPosition)
            {
                choppingBoardFunction.selectedShader.SetActive(true);
            }

            if (transform.tag == "WashedLaurelLeaves")
                choppingBoardFunction.selectedShader.SetActive(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (!point.isOccupied)
            {
                float distance = Vector2.Distance(
                    rectTransform.position,
                    point.GetComponent<RectTransform>().position
                );

                if (distance <= point.snapRadius)
                {
                    point.OnHoverEnter();
                }
                else
                {
                    point.OnHoverExit();
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localScale += new Vector3(-0.2f, -0.2f, 1f); 

        RectTransform spawnParent = transform.parent.GetComponent<RectTransform>();
        // Check General Snap Points
        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (!point.isOccupied)
            {
                float distance = Vector2.Distance(
                    rectTransform.position,
                    point.GetComponent<RectTransform>().position
                );
                Debug.Log("Distance to snap point: " + distance);

                if (distance <= point.snapRadius)
                {
                    point.OnHoverExit();
                    SnapPointShaderOff();
                    sinkFunction.selectedShader.SetActive(false);
                    choppingBoardFunction.selectedShader.SetActive(false);
                    point.ConvertScreenToLocalPoint(rectTransform, spawnParent);
                    point.isOccupied = true;
                    snapPointIndex = System.Array.IndexOf(snapPoints, point);
                    spawnPositionCopy = rectTransform.anchoredPosition;
                    return;
                }
            }
        }

        if (rectTransform.anchoredPosition == sinkPosition || rectTransform.anchoredPosition == chopBoardPosition)
        {
            spawnPositionCopy = rectTransform.anchoredPosition;
            sinkFunction.selectedShader.SetActive(false);
            SnapPointShaderOff();
            return;
        }
        
        // No snap point found — snap back to last position
        rectTransform.anchoredPosition = spawnPositionCopy;
        if (spawnPositionCopy == sinkPosition)
            sinkFunction.isOccupied = true;
        if (spawnPositionCopy == chopBoardPosition)
            choppingBoardFunction.isOccupied = true;
        SnapPointShaderOff();
        sinkFunction.selectedShader.SetActive(false);
        choppingBoardFunction.selectedShader.SetActive(false);
        snapPoints[snapPointIndex].isOccupied = true;
    }

    void SnapPointShaderOff()
    {
        foreach (GeneralSnapPoint point in snapPoints)
        {
            point.selectedShader.SetActive(false);
        }
    }
}
