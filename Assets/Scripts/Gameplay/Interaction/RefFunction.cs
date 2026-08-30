using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RefFunction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image image;
    private Transform spawnParent;
    private InventoryManager inventoryManager;

    void Start()
    {
        spawnParent = GameObject.Find("InventoryCanvas").transform;
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); // Initialize the inventoryManager reference by finding the InventoryCanvas GameObject and getting the InventoryManager component from it.
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Image spawnedImage = Instantiate(image, spawnParent);
        if (inventoryManager.place[1] == true)
        {
            Debug.Log("No available spawn positions in the basket.");
            return;
        }

        // GET THE SPAWN POSITION FROM THE BOWL MANAGER
        Vector2 spawnPosition = inventoryManager.snapPoints[1].GetComponent<RectTransform>().anchoredPosition;

        // SET THE SPAWNED IMAGE POSITION TO THE SPAWN POSITION
        spawnedImage.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
        DragDrop dragDrop = spawnedImage.GetComponent<DragDrop>();
        GeneralSnapPoint[] snapPoints = FindObjectsOfType<GeneralSnapPoint>();
        foreach (GeneralSnapPoint point in snapPoints)
        {
            if (point.GetComponent<RectTransform>().anchoredPosition == spawnPosition)
            {
                inventoryManager.place[System.Array.IndexOf(inventoryManager.snapPoints, point)] = true;
                point.isOccupied = true;
                dragDrop.snapPoint = point;
            }
        }
    }
}
