using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BowlDragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{

    // Bowl Data
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private BowlManager bowlManager;
    public GameObject currentItem;
    public Canvas canvas;
    public Image[] filledBowls;
    public string bowlName;
    Vector2 genPointSize;
    Vector2 originalSize;

    // Snapping points for bowls
    BowlSnapPoints[] snapPoints;
    GeneralSnapPoint[] generalSnapPoints;
    public BowlSnapPoints BSP;
    public GeneralSnapPoint GSP;

    // Initialize References
    private void Start()
    {
        // bowlManager = GameObject.Find("Canvas").GetComponent<BowlManager>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("NewCanvas").GetComponent<Canvas>();
        snapPoints = FindObjectsOfType<BowlSnapPoints>();
        generalSnapPoints = FindObjectsOfType<GeneralSnapPoint>();
        genPointSize = new Vector2(117.8653f, 88.9671f);
        originalSize = rectTransform.sizeDelta;
        GSP = null;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Makes the bowl semi-transparent
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;

        // Enlarge the bowl image while dragging
        transform.localScale += new Vector3(0.2f, 0.2f, 1f);

        rectTransform.sizeDelta = originalSize;

        if (BSP != null)
        {
            BSP.isOccupied = false;
        }
        else if (GSP != null)
        {
            GSP.isOccupied = false;
        }

        foreach (BowlSnapPoints point in snapPoints)
        {
            if (!point.isOccupied)
                point.selectedShader.SetActive(true);
        }
        foreach (GeneralSnapPoint point in generalSnapPoints)
        {
            if (!point.isOccupied)
                point.selectedShader.SetActive(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        foreach (GeneralSnapPoint point in generalSnapPoints)
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

        foreach (BowlSnapPoints point in snapPoints)
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

        BowlSnapPointDrop();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            currentItem = eventData.pointerDrag;

            // switch(bowlName)
            // {
            //     case "WhiteBowl":
            //         switch(currentItem.tag)
            //         {
            //             case "GarlicMinced":
            //             transform.GetComponent<Image>().sprite = filledBowls[2].sprite;
            //             Destroy(currentItem);
            //             break;
            //         }
            //         break;
            // }
            
            // switch(currentItem.tag)
            // {
            //     case "LaurelLeavesCut":
            //         transform.GetComponent<Image>().sprite = filledBowls[0].sprite;
            //         Destroy(currentItem);
            //         break;
            //     case "PorkKawaliMinced":
            //         transform.GetComponent<Image>().sprite = filledBowls[1].sprite;
            //         transform.localScale += new Vector3(0.0f, 0.15f, 1f);
            //         Destroy(currentItem);
            //         break;
            //     case "GarlicMinced":
            //         transform.GetComponent<Image>().sprite = filledBowls[2].sprite;
            //         Destroy(currentItem);
            //         break;
            //     case "OnionWedges":
            //         transform.GetComponent<Image>().sprite = filledBowls[3].sprite;
            //         Destroy(currentItem);
            //         break;
            //     case "OnionMinced":
            //         transform.GetComponent<Image>().sprite = filledBowls[4].sprite;
            //         Destroy(currentItem);
            //         break;
            //     case "TomatoWedges":
            //         transform.GetComponent<Image>().sprite = filledBowls[5].sprite;
            //         Destroy(currentItem);
            //         break;
                
            // }
        }
    }

    void BowlSnapPointDrop()
    {
        RectTransform spawnParent = transform.parent.GetComponent<RectTransform>();
        // Check Bowl Snap Points first
        foreach (BowlSnapPoints point in snapPoints)
        {
            if (!point.isOccupied)
            {
                float distance = Vector2.Distance(
                    rectTransform.position,
                    point.GetComponent<RectTransform>().position
                );

                if (distance <= point.snapRadius)
                {
                    point.OnHoverExit();
                    SnapPointShaderOff();
                    if (GSP != null)
                    {
                        GSP.isOccupied = false;
                        GSP = null;
                    }
                    BSP = point;
                    point.ConvertScreenToLocalPoint(rectTransform, spawnParent);
                    point.isOccupied = true;
                    return; // found a valid bowl snap point, stop checking
                }
            }
        }

        // Check General Snap Points
        foreach (GeneralSnapPoint point in generalSnapPoints)
        {
            if (!point.isOccupied)
            {
                float distance = Vector2.Distance(
                    rectTransform.position,
                    point.GetComponent<RectTransform>().position
                );

                if (distance <= point.snapRadius)
                {
                    rectTransform.sizeDelta = genPointSize;
                    point.OnHoverExit();
                    SnapPointShaderOff();
                    if (BSP != null)
                    {
                        BSP.isOccupied = false;
                        BSP = null;  
                    }
                    GSP = point;
                    point.ConvertScreenToLocalPoint(rectTransform, spawnParent);
                    point.isOccupied = true;
                    Debug.Log(GSP);
                    return;
                }
            }
        }

        // Neither snap point found — snap back to last position
        if (BSP != null)
        {
            BSP.ConvertScreenToLocalPoint(rectTransform, spawnParent);
            BSP.isOccupied = true;
            SnapPointShaderOff();
        }
        else if (GSP != null)
        {
            GSP.ConvertScreenToLocalPoint(rectTransform, spawnParent);
            GSP.isOccupied = true;
            rectTransform.sizeDelta = genPointSize;
            SnapPointShaderOff();
        }
    }

    void SnapPointShaderOff()
    {
        foreach (GeneralSnapPoint point in generalSnapPoints)
        {
            point.selectedShader.SetActive(false);
        }

        foreach (BowlSnapPoints point in snapPoints)
        {
            point.selectedShader.SetActive(false);
        }
    }
}
