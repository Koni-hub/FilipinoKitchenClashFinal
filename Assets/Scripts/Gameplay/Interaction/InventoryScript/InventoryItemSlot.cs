using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler
{
    // ITEM DATA
    public string itemName;
    public int quantity; 
    public Sprite itemSprite; 
    public bool isFull; 

    // INVENTORY DATA
    [SerializeField] private int maxNumberOfItems = 9; 
    private Transform spawnParent; 
    [SerializeField] private Sprite emptySlotSprite; 
    [SerializeField] private Image[] spawnImages; 
    private InventoryManager inventoryManager; 
    public BoxFunction[] boxFunctions; 
    private GameObject inventoryMenu; 
    GeneralSnapPoint[] snapPoints;
    
    // ITEM SLOT
    [SerializeField] TMP_Text quantityText; 
    [SerializeField] Image itemImage; 

    public GameObject selectedShader; 

    void Start()
    {
        inventoryMenu = GameObject.Find("InventoryMenu"); 
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); 
        spawnParent = GameObject.Find("SpawnedIngredients").transform; 
        snapPoints = FindObjectsOfType<GeneralSnapPoint>();
    }
    public void AddItem(string itemName, int quantity, Sprite itemSprite)
    {
        if (isFull)
            return;
        this.itemName = itemName;
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;
        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            this.quantity = maxNumberOfItems;
            quantityText.text = this.quantity.ToString(); 
            quantityText.gameObject.SetActive(true); 
            isFull = true; 
        }
        quantityText.text = this.quantity.ToString(); 
        quantityText.gameObject.SetActive(true); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true); 
        StartCoroutine(SecondsDelay(0.1f)); 
    }

    private IEnumerator SecondsDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        switch (itemName)
        {
            case "Garlic":
                spawnItem(spawnImages[0], boxFunctions[0], 0);
                break;
            case "Onion":
                spawnItem(spawnImages[1], boxFunctions[1], 0);
                break;
            case "Tomato":
                spawnItem(spawnImages[2], boxFunctions[2], 0);
                break;
            case "Laurel Leaves":
                spawnItem(spawnImages[3], boxFunctions[3], 1);
                break;
            default:
                Debug.Log("No item to remove.");
                break;
        }
    }

    private void EmptySlot()
    {
        quantityText.gameObject.SetActive(false);
        itemImage.sprite = emptySlotSprite;
    }

    private void spawnItem(Image spawnImage, BoxFunction boxFunction, int snapPointIndex)
    {
        if (inventoryManager.snapPoints[snapPointIndex].GetComponent<GeneralSnapPoint>().isOccupied)
        {
            Debug.Log("Place not available.");
            return;
        }

        // SPAWN THE ITEM IN THE INVENTORY
        Image spawnedImage = Instantiate(spawnImage, spawnParent);

        // SET THE SPAWN POSITION FROM THE INVENTORY MANAGER
        Vector2 spawnPosition = inventoryManager.snapPoints[snapPointIndex].GetComponent<RectTransform>().anchoredPosition;
        spawnedImage.GetComponent<RectTransform>().anchoredPosition = spawnPosition;

        // SET THE SNAP POINT AND UPDATE THE INVENTORY MANAGER'S PLACE ARRAY
        DragDrop dragDrop = spawnedImage.GetComponent<DragDrop>();
        inventoryManager.place[snapPointIndex] = true;
        inventoryManager.snapPoints[snapPointIndex].GetComponent<GeneralSnapPoint>().isOccupied = true;
        dragDrop.snapPoint = inventoryManager.snapPoints[snapPointIndex].GetComponent<GeneralSnapPoint>();

        // UPDATE THE INVENTORY MENU AND QUANTITY
        inventoryMenu.SetActive(false);
        inventoryManager.DeselectAllSlots();
        this.quantity -= 1;
        quantityText.text = this.quantity.ToString();
        boxFunction.OnIngredientUsed();
        if (this.quantity <= 0)
            EmptySlot();
    }
}
