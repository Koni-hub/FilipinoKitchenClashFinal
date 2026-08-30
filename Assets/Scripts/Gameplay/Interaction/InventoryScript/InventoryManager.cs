using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventoryItemSlot[] inventorySlots;
    public bool[] place = new bool[4];
    public GeneralSnapPoint[] snapPoints;

    void Awake()
    {
        snapPoints = FindObjectsOfType<GeneralSnapPoint>();
    }

    public Vector2 spawnPosition()
    {
        for (int i = 0; i < place.Length; i++)
        {
            if (place[i] == false)
            {
                place[i] = true;
                return snapPoints[i].GetComponent<RectTransform>().anchoredPosition;
            }
        }
        return Vector3.zero;
    }

    public void RemovePlace(Vector2 position)
    {
        for (int i = 0; i < place.Length; i++)
        {
            if (snapPoints[i].GetComponent<RectTransform>().anchoredPosition == position)
            {
                snapPoints[i].GetComponent<GeneralSnapPoint>().isOccupied = false;
                place[i] = false;
                return;
            }
        }
    }
    public void AddItem(string itemName, int quantity, Sprite itemSprite)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].isFull && inventorySlots[i].itemName == itemName || inventorySlots[i].quantity == 0)
            {
                inventorySlots[i].AddItem(itemName, quantity, itemSprite);
                return;
            }
        }
    }

    public void DeselectAllSlots()
    {
        foreach (InventoryItemSlot slot in inventorySlots)
        {
            slot.selectedShader.SetActive(false);
        }
    }
}
