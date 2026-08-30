using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;

public class SinkFunction : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Canvas canvas;
    public Image[] waterAnimation;
    public Image[] washedObjects;
    public GameObject currentItem;
    public GameObject sinkObject;
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
        Debug.Log("Object has entered");
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
            switch (currentItem.tag)
            {
                case "LaurelLeaves":
                    ConvertScreenToLocalPoint(eventData);
                    StartCoroutine(PlayWaterAnimation());
                    currentItem.GetComponent<Image>().sprite = washedObjects[0].sprite;
                    currentItem.tag = "WashedLaurelLeaves";
                    break;
                case "Pork":
                    ConvertScreenToLocalPoint(eventData);
                    StartCoroutine(PlayWaterAnimation());
                    currentItem.GetComponent<Image>().sprite = washedObjects[1].sprite;
                    currentItem.tag = "WashedPork";
                    break;
            }
        }
    }

    void ConvertScreenToLocalPoint(PointerEventData eventData)
    {
        RectTransform snapPointRect = GetComponent<RectTransform>();
        RectTransform draggedRect = eventData.pointerDrag.GetComponent<RectTransform>();

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
            dragDrop.sinkPosition = localPoint;
        }
        draggedRect.anchoredPosition = localPoint;
        isOccupied = true;
        selectedShader.SetActive(false);
    }

    private IEnumerator PlayWaterAnimation()
    {
        for (int i = 0; i < 6; i++)
        {
            sinkObject.GetComponent<Image>().sprite = waterAnimation[i].sprite;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
