using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainInventory : MonoBehaviour
{
    [SerializeField] private MainInventorySlot[] inventorySlots;
    public bool[] place = new bool[2];
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

    public void DeselectAllSlots()
    {
        foreach (MainInventorySlot slot in inventorySlots)
        {
            slot.selectedShader.SetActive(false);
        }
    }
}
