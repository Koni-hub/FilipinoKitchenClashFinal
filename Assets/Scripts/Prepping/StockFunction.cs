using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockFunction : MonoBehaviour
{
    // INVENTORY SLOT DATA 
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private Sprite sprite;
    public MainInventorySlot[] inventorySlot;
    int ingredientCount;

    // BASKET PROPERTIES
    public Image ingredientIcon;
    private NewBasket newBasket;
    Image basketIcon;
    [SerializeField] private Sprite defaultIcon;

    void Start()
    {
        newBasket = GameObject.Find("Basket").GetComponent<NewBasket>();
        ingredientCount = CatchGameManager.Instance.GetIngredientCount("Adobo", itemName);
    }

    public void OnStockPressed()
    {

        if (ingredientCount > 0)
        {
            foreach (Image icon in newBasket.icons)
            {
                if (icon.sprite == ingredientIcon.sprite)
                {
                    Debug.Log("Ingredient already in the basket.");
                    AddItemToInventory(itemName, quantity, sprite);
                    // ingredientCount--;
                    return;
                }
            }
            
            if (newBasket.iconPosition() != null)
            {
                basketIcon = newBasket.iconPosition();
                basketIcon.sprite = ingredientIcon.sprite;
                newBasket.isOccupied[Array.IndexOf(newBasket.icons, basketIcon)] = true;
            }
            else
            {
                Debug.Log("No available slot in the basket.");
            }   
            AddItemToInventory(itemName, quantity, sprite);
            ingredientCount--;
        }
    }

    void AddItemToInventory(string itemName, int quantity, Sprite sprite)
    {
        for (int i = 0; i < inventorySlot.Length; i++)
        {
            if (!inventorySlot[i].isFull && inventorySlot[i].itemName == itemName || inventorySlot[i].quantity == 0)
            {
                inventorySlot[i].AddItem(itemName, quantity, sprite);
                break;
            }
        }
    }

    public void OnIngredientUsed(string itemName)
    {
        foreach (MainInventorySlot slot in inventorySlot)
        {
            if (slot.itemName == itemName)
            {
                basketIcon.sprite = defaultIcon;
                newBasket.isOccupied[Array.IndexOf(newBasket.icons, basketIcon)] = false;
            }
        }
    }
}
