using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChoppingBoardFunction : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Canvas canvas;
    public GameObject currentItem;
    public Image[] choppedObjects;
    public BarManager barManager;
    RectTransform draggedRect;

    public GameObject selectedShader;
    private Vector3 originalScale;
    public bool isOccupied = false;

    void Start()
    {
        canvas = GameObject.Find("NewCanvas").GetComponent<Canvas>();
        originalScale = selectedShader.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
            selectedShader.transform.localScale = originalScale * 1.3f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
            selectedShader.transform.localScale = originalScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            currentItem = eventData.pointerDrag;
            StartCoroutine(Delay(0.50f, eventData));
    
        }
    
    }

    private IEnumerator Delay(float delay, PointerEventData eventData)
    {
        switch (currentItem.tag)
            {
                case "WashedPork":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[0].sprite;
                    currentItem.tag = "PorkKawaliCut";
                    break;
                case "PorkKawaliCut":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[1].sprite;
                    currentItem.tag = "PorkKawaliMinced";
                    break;
                case "WashedLaurelLeaves":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[2].sprite;
                    currentItem.tag = "LaurelLeavesCut";
                    break;
                case "Garlic":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[3].sprite;
                    currentItem.tag = "GarlicMinced";
                    break;
                case "Onion":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[4].sprite;
                    currentItem.tag = "OnionFirstCut";
                    break;
                case "OnionFirstCut":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[5].sprite;
                    currentItem.tag = "OnionQuartered";
                    break;
                case "OnionQuartered":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[6].sprite;
                    currentItem.tag = "OnionWedges";
                    break;
                case "OnionWedges":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[7].sprite;
                    currentItem.tag = "OnionMinced";
                    break;
                case "Labanos":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[8].sprite;
                    break;
                case "PeeledGabi":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[9].sprite;
                    break;
                case "Sitaw":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[10].sprite;
                    break;
                case "Talong":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[11].sprite;
                    break;
                case "Okra":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[12].sprite;
                    break;
                case "Liver":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[13].sprite;
                    currentItem.tag = "LiverHalf";
                    break;
                case "LiverHalf":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[14].sprite;
                    currentItem.tag = "LiverInThree";
                    break;
                case "LiverInThree":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[15].sprite;
                    currentItem.tag = "LiverHalfSliced";
                    break;
                case "LiverHalfSliced":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[16].sprite;
                    currentItem.tag = "LiverHalfCubes";
                    break;
                case "LiverHalfCubes":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[17].sprite;
                    currentItem.tag = "LiverCubes";
                    break;
                case "Tomato":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[18].sprite;
                    currentItem.tag = "TomatoHalf";
                    break;
                case "TomatoHalf":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[19].sprite;
                    currentItem.tag = "TomatoWedges";
                    break;
            }
    }

    void ConvertScreenToLocalPoint(PointerEventData eventData)
    {
        RectTransform snapPointRect = GetComponent<RectTransform>();
        draggedRect = eventData.pointerDrag.GetComponent<RectTransform>();

        // Convert snap point position to Canvas local space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera, 
            snapPointRect.position
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        DragNDrop[] dragDrops = FindObjectsOfType<DragNDrop>();
        foreach(DragNDrop dragDrop in dragDrops)
        {
            dragDrop.chopBoardPosition = localPoint;
            dragDrop.chopBoardPositionTaken = true;
        }
        
        draggedRect.anchoredPosition = localPoint;
        isOccupied = true;
        selectedShader.SetActive(false);
    }
}
