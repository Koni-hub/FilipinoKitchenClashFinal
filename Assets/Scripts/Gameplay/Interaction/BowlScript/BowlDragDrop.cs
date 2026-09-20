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
    public string[] bowlIngredients = new string[4];
    Vector2 genPointSize;
    Vector2 originalSize;

    // Snapping points for bowls
    BowlSnapPoints[] snapPoints;
    GeneralSnapPoint[] generalSnapPoints;
    public BowlSnapPoints BSP;
    public GeneralSnapPoint GSP;
    private ChoppingBoardFunction choppingBoardFunction;
    private SinkFunction sinkFunction;
    private SendBowlIngredients sendBowlIngredients;

    // Initialize References
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        sendBowlIngredients = GameObject.Find("DataHandler").GetComponent<SendBowlIngredients>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("NewCanvas").GetComponent<Canvas>();
        snapPoints = FindObjectsOfType<BowlSnapPoints>();
        generalSnapPoints = FindObjectsOfType<GeneralSnapPoint>();
        genPointSize = new Vector2(47f, 38f);
        originalSize = rectTransform.sizeDelta;
        GSP = null;
        sinkFunction = GameObject.Find("SinkSnapPoint").GetComponent<SinkFunction>();
        choppingBoardFunction = GameObject.Find("ChoppingBoard").GetComponent<ChoppingBoardFunction>();
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

            PutIngredientsOnBowl();
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
                    rectTransform.sizeDelta += genPointSize;
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

    void PutIngredientsOnBowl()
    {
        if (bowlName == "SilverBowl")
        {
            if (currentItem.tag == "LaurelLeavesCut")
            {
                transform.GetComponent<Image>().sprite = filledBowls[0].sprite;
                Destroy(currentItem);
                sendBowlIngredients.AddBowlIngredient("SilverBowlLaurelLeaves");
                SnapPointShaderOff();
                sinkFunction.selectedShader.SetActive(false);
                choppingBoardFunction.selectedShader.SetActive(false);
            }
            else if (currentItem.tag == "PorkKawaliMinced")
            {
                transform.GetComponent<Image>().sprite = filledBowls[1].sprite;
                Destroy(currentItem);
                sendBowlIngredients.AddBowlIngredient("SilverBowlPorkBelly");
                SnapPointShaderOff();
                sinkFunction.selectedShader.SetActive(false);
                choppingBoardFunction.selectedShader.SetActive(false);
            }
        }
        else if (bowlName == "BlueBowl")
        {
            if (currentItem.tag == "OnionMinced")
            {
                transform.GetComponent<Image>().sprite = filledBowls[0].sprite;
                Destroy(currentItem);
                sendBowlIngredients.AddBowlIngredient("BlueBowlOnionMinced");
                SnapPointShaderOff();
                sinkFunction.selectedShader.SetActive(false);
                choppingBoardFunction.selectedShader.SetActive(false);
            }
        }
        else if (bowlName == "WhiteBowl")
        {
            if (currentItem.tag == "GarlicMinced")
            {
                transform.GetComponent<Image>().sprite = filledBowls[0].sprite;
                Destroy(currentItem);
                sendBowlIngredients.AddBowlIngredient("WhiteBowlGarlic");
                SnapPointShaderOff();
                sinkFunction.selectedShader.SetActive(false);
                choppingBoardFunction.selectedShader.SetActive(false);
            }
        }
    }


}
