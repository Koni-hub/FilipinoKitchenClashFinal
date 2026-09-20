using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RefFunction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image spawnImage;
    private Transform spawnParent;

    void Start()
    {
        spawnParent = GameObject.Find("SpawnedIngredients").transform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GeneralSnapPoint[] snapPoints = FindObjectsOfType<GeneralSnapPoint>();

        if (snapPoints[0].isOccupied)
        {
            Debug.Log("Place not available.");
            return;
        }

        // SPAWN THE ITEM IN THE INVENTORY
        Image spawnedImage = Instantiate(spawnImage, spawnParent);

        // CONVERTS THE POSITION OF SNAP POINT TO THAT OF SPAWN INGREDIENT
        RectTransform snapPointRect = snapPoints[0].GetComponent<RectTransform>();
        RectTransform spawnParentRect = spawnParent.GetComponent<RectTransform>(); // this is where it comes from
        Camera cam = Camera.main;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, snapPointRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            spawnParentRect,
            screenPoint,
            cam,
            out Vector2 localPoint
        );

        DragNDrop dragDrop = spawnedImage.GetComponent<DragNDrop>();
        if (dragDrop != null)
        {
            dragDrop.spawnPositionCopy = localPoint;
            dragDrop.snapPointIndex = 0;
        }

        spawnedImage.GetComponent<RectTransform>().anchoredPosition = localPoint;
        snapPoints[0].isOccupied = true;
    }
}
