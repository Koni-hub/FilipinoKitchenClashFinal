using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;

public class MainInventorySlot : MonoBehaviour, IPointerClickHandler
{
    // ITEM DATA
    public string itemName;
    public int quantity; 
    public bool isFull; 

    // INVENTORY DATA
    [SerializeField] private int maxNumberOfItems = 9; 
    private Transform spawnParent; 
    [SerializeField] private Sprite emptySlotSprite; 
    [SerializeField] private Image[] spawnImages; 
    [SerializeField] private StockFunction[] stockFunctions;
    public MainInventory inventoryManager;
    private GameObject mainInventory;

    // ITEM SLOT
    [SerializeField] TMP_Text quantityText; 
    [SerializeField] Image itemImage; 

    public GameObject selectedShader;

    void Start()
    {
        mainInventory = GameObject.Find("MainInventory"); 
        inventoryManager = GameObject.Find("MainInventory").GetComponent<MainInventory>();
        spawnParent = GameObject.Find("SpawnedIngredients").transform; 
    }

    public void AddItem(string itemName, int quantity, Sprite itemSprite)
    {
        if (isFull) return;
        this.itemName = itemName;
        itemImage.sprite = itemSprite;
        this.quantity += quantity;
        if (this.quantity > maxNumberOfItems)
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
        selectedShader.SetActive(true);
        StartCoroutine(SpawnItem(0.1f));
    }

    private IEnumerator SpawnItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        switch (itemName)
        {
            case "Garlic":
                InstantiateItem(spawnImages[0], stockFunctions[0], 1);
                break;
            case "Onion":
                InstantiateItem(spawnImages[1], stockFunctions[1], 1);
                break;
            case "Tomato":
                InstantiateItem(spawnImages[2], stockFunctions[2], 1);
                break;
            case "LaurelLeaves":
                InstantiateItem(spawnImages[3], stockFunctions[3], 0);
                break;
            default:
                Debug.Log("No item to remove.");
                break;
        }
    }

    void InstantiateItem(Image spawnImage, StockFunction stockFunction, int snapPointIndex)
    {
        GeneralSnapPoint[] snapPoints = FindObjectsOfType<GeneralSnapPoint>();

        if (snapPoints[snapPointIndex].isOccupied)
        {
            Debug.Log("Place not available.");
            return;
        }

        // SPAWN THE ITEM IN THE INVENTORY
        Image spawnedImage = Instantiate(spawnImage, spawnParent);

        // CONVERTS THE POSITION OF SNAP POINT TO THAT OF SPAWN INGREDIENT
        RectTransform snapPointRect = inventoryManager.snapPoints[snapPointIndex].GetComponent<RectTransform>();
        RectTransform spawnParentRect = spawnParent.GetComponent<RectTransform>(); // this is where it comes from
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, snapPointRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            spawnParentRect,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        DragNDrop dragDrop = spawnedImage.GetComponent<DragNDrop>();
        if (dragDrop != null)
        {
            dragDrop.spawnPositionCopy = localPoint;
            dragDrop.snapPointIndex = snapPointIndex;
        }

        spawnedImage.GetComponent<RectTransform>().anchoredPosition = localPoint;
        snapPoints[snapPointIndex].isOccupied = true;
        Debug.Log(snapPoints[snapPointIndex].isOccupied);

        // UPDATE THE INVENTORY MENU AND QUANTITY
        mainInventory.SetActive(false);
        inventoryManager.DeselectAllSlots();
        this.quantity -= 1;
        quantityText.text = this.quantity.ToString();
        if (this.quantity <= 0)
        {
            stockFunction.OnIngredientUsed(itemName);
            EmptySlot();
        }
    }

    private void EmptySlot()
    {
        quantityText.gameObject.SetActive(false);
        itemImage.sprite = emptySlotSprite;
    }
}
