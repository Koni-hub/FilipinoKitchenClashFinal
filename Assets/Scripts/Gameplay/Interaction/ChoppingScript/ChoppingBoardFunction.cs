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

        if (IngredientLookup.Instance != null && choppedObjects != null)
        {
            IngredientLookup.Instance.RegisterChopSprites("WashedPork", new Sprite[] { choppedObjects[0].sprite, choppedObjects[1].sprite });
            IngredientLookup.Instance.RegisterChopSprites("WashedLaurelLeaves", new Sprite[] { choppedObjects[2].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Garlic", new Sprite[] { choppedObjects[3].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Onion", new Sprite[] { choppedObjects[4].sprite, choppedObjects[5].sprite, choppedObjects[6].sprite, choppedObjects[7].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Tomato", new Sprite[] { choppedObjects[18].sprite, choppedObjects[19].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Liver", new Sprite[] { choppedObjects[13].sprite, choppedObjects[14].sprite, choppedObjects[15].sprite, choppedObjects[16].sprite, choppedObjects[17].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Labanos", new Sprite[] { choppedObjects[8].sprite });
            IngredientLookup.Instance.RegisterChopSprites("PeeledGabi", new Sprite[] { choppedObjects[9].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Sitaw", new Sprite[] { choppedObjects[10].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Talong", new Sprite[] { choppedObjects[11].sprite });
            IngredientLookup.Instance.RegisterChopSprites("Okra", new Sprite[] { choppedObjects[12].sprite });
        }
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
        string originalTag = currentItem.tag;
        string newTag = originalTag;
        int spriteIndex = -1;

        switch (currentItem.tag)
            {
                case "WashedPork":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[0].sprite;
                    currentItem.tag = "PorkKawaliCut";
                    newTag = "PorkKawaliCut";
                    spriteIndex = 0;
                    break;
                case "PorkKawaliCut":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[1].sprite;
                    currentItem.tag = "PorkKawaliMinced";
                    newTag = "PorkKawaliMinced";
                    spriteIndex = 1;
                    break;
                case "WashedLaurelLeaves":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[2].sprite;
                    currentItem.tag = "LaurelLeavesCut";
                    newTag = "LaurelLeavesCut";
                    spriteIndex = 0;
                    break;
                case "Garlic":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[3].sprite;
                    currentItem.tag = "GarlicMinced";
                    newTag = "GarlicMinced";
                    spriteIndex = 0;
                    break;
                case "Onion":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[4].sprite;
                    currentItem.tag = "OnionFirstCut";
                    newTag = "OnionFirstCut";
                    spriteIndex = 0;
                    break;
                case "OnionFirstCut":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[5].sprite;
                    currentItem.tag = "OnionQuartered";
                    newTag = "OnionQuartered";
                    spriteIndex = 1;
                    break;
                case "OnionQuartered":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[6].sprite;
                    currentItem.tag = "OnionWedges";
                    newTag = "OnionWedges";
                    spriteIndex = 2;
                    break;
                case "OnionWedges":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[7].sprite;
                    currentItem.tag = "OnionMinced";
                    newTag = "OnionMinced";
                    spriteIndex = 3;
                    break;
                case "Labanos":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[8].sprite;
                    currentItem.tag = "LabanosChopped";
                    newTag = "LabanosChopped";
                    spriteIndex = 0;
                    break;
                case "PeeledGabi":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[9].sprite;
                    currentItem.tag = "GabiChopped";
                    newTag = "GabiChopped";
                    spriteIndex = 0;
                    break;
                case "Sitaw":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[10].sprite;
                    currentItem.tag = "SitawCut";
                    newTag = "SitawCut";
                    spriteIndex = 0;
                    break;
                case "Talong":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[11].sprite;
                    currentItem.tag = "TalongCut";
                    newTag = "TalongCut";
                    spriteIndex = 0;
                    break;
                case "Okra":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[12].sprite;
                    currentItem.tag = "OkraCut";
                    newTag = "OkraCut";
                    spriteIndex = 0;
                    break;
                case "Liver":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[13].sprite;
                    currentItem.tag = "LiverHalf";
                    newTag = "LiverHalf";
                    spriteIndex = 0;
                    break;
                case "LiverHalf":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[14].sprite;
                    currentItem.tag = "LiverInThree";
                    newTag = "LiverInThree";
                    spriteIndex = 1;
                    break;
                case "LiverInThree":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[15].sprite;
                    currentItem.tag = "LiverHalfSliced";
                    newTag = "LiverHalfSliced";
                    spriteIndex = 2;
                    break;
                case "LiverHalfSliced":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[16].sprite;
                    currentItem.tag = "LiverHalfCubes";
                    newTag = "LiverHalfCubes";
                    spriteIndex = 3;
                    break;
                case "LiverHalfCubes":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[17].sprite;
                    currentItem.tag = "LiverCubes";
                    newTag = "LiverCubes";
                    spriteIndex = 4;
                    break;
                case "Tomato":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[18].sprite;
                    currentItem.tag = "TomatoHalf";
                    newTag = "TomatoHalf";
                    spriteIndex = 0;
                    break;
                case "TomatoHalf":
                    ConvertScreenToLocalPoint(eventData);
                    barManager.UpdateLoadingBar();
                    yield return new WaitForSeconds(delay);
                    currentItem.GetComponent<Image>().sprite = choppedObjects[19].sprite;
                    currentItem.tag = "TomatoWedges";
                    newTag = "TomatoWedges";
                    spriteIndex = 1;
                    break;
            }

        if (originalTag != newTag && PreppingSync.Instance != null)
        {
            DragNDrop dragDrop = currentItem.GetComponent<DragNDrop>();
            if (dragDrop != null && !string.IsNullOrEmpty(dragDrop.syncId))
            {
                PreppingSync.Instance.SendChopComplete(dragDrop.syncId, originalTag, newTag, spriteIndex);
            }
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
