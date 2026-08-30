using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxFunction : MonoBehaviour
{
    public Image ingredients; // THIS LINE OF CODE HANDLES THE SPRITE OF THE INGREDIENTS IN THE BASKET
    public Transform spawnParent; // THIS LINE OF CODE HANDLES THE PARENT TRANSFORM WHERE THE INGREDIENTS WILL BE SPAWNED IN THE BASKET
    public BasketFunction basketFunction; // THIS LINE OF CODE HANDLES THE FUNCTIONALITY OF THE BASKET, SUCH AS CHECKING FOR AVAILABLE SPACE AND REMOVING INGREDIENTS
    private InventoryManager inventoryManager; // THIS LINE OF CODE HANDLES THE FUNCTIONALITY OF THE INVENTORY, SUCH AS ADDING ITEMS TO THE INVENTORY WHEN THEY ARE PICKED UP AND REMOVING THEM WHEN THEY ARE USED
    public InventoryItemSlot[] inventorySlots; // THIS LINE OF CODE HANDLES THE ARRAY OF INVENTORY SLOTS, WHICH CAN BE USED TO CHECK FOR AVAILABLE SPACE IN THE INVENTORY OR TO UPDATE THE UI WHEN ITEMS ARE ADDED OR REMOVED

    [SerializeField] private string itemName; // THIS LINE OF CODE HANDLES THE NAME OF THE ITEM, WHICH CAN BE USED TO IDENTIFY THE ITEM IN THE INVENTORY OR TO CHECK FOR SPECIFIC ITEMS IN THE GAMEPLAY LOGIC
    [SerializeField] private int quantity; // THIS LINE OF CODE HANDLES THE QUANTITY OF THE ITEM, WHICH CAN BE USED TO TRACK HOW MANY OF A PARTICULAR ITEM THE PLAYER HAS IN THE INVENTORY OR TO CHECK IF THE PLAYER HAS ENOUGH OF AN ITEM TO PERFORM A CERTAIN ACTION
    [SerializeField] private Sprite sprite; // THIS LINE OF CODE HANDLES THE SPRITE OF THE ITEM, WHICH CAN BE USED TO DISPLAY THE ITEM IN THE INVENTORY UI OR TO REPRESENT THE ITEM IN THE GAME WORLD
    private int count = 0; // THIS LINE OF CODE HANDLES A COUNT VARIABLE, WHICH CAN BE USED TO TRACK HOW MANY TIMES AN ITEM HAS BEEN PICKED UP OR USED, AND TO CONTROL THE SPAWNING AND REMOVAL OF INGREDIENTS IN THE BASKET BASED ON THIS COUNT
    private List<GameObject> spawnedBasketImages = new List<GameObject>();
    private List<Vector2> spawnedPositions = new List<Vector2>();
    
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    public void OnBoxPressed()
    {
        if (ingredients != null) // IF THE INGREDIENTS IMAGE IS NOT NULL, THEN PROCEED TO SPAWN THE INGREDIENT IN THE BASKET AND ADD IT TO THE INVENTORY
        {
            Vector3 spawnPosition = basketFunction.spawnPosition(); // THIS LINE OF CODE CALLS THE spawnPosition() FUNCTION FROM THE BasketFunction SCRIPT TO GET AN AVAILABLE SPAWN POSITION IN THE BASKET. IF THERE ARE NO AVAILABLE POSITIONS, IT RETURNS Vector3.zero.            
            if(spawnPosition == Vector3.zero) // IF THE SPAWN POSITION IS Vector3.zero, IT MEANS THERE ARE NO AVAILABLE POSITIONS IN THE BASKET, SO WE LOG A MESSAGE AND ADD THE ITEM TO THE INVENTORY WITHOUT SPAWNING IT IN THE BASKET
            {
                Debug.Log("No available spawn positions in the basket.");
                inventoryManager.AddItem(itemName, quantity, sprite); // THIS LINE OF CODE ADDS THE ITEM TO THE INVENTORY USING THE AddItem() FUNCTION FROM THE InventoryManager SCRIPT, PASSING IN THE itemName, quantity, AND sprite AS PARAMETERS
                count++; // THIS LINE OF CODE INCREMENTS THE count VARIABLE, WHICH CAN BE USED TO TRACK HOW MANY TIMES AN ITEM HAS BEEN PICKED UP OR USED, AND TO CONTROL THE SPAWNING AND REMOVAL OF INGREDIENTS IN THE BASKET BASED ON THIS COUNT
                return;
            }
            Image spawnedImage = Instantiate(ingredients, spawnParent); // THIS LINE OF CODE INSTANTIATES A NEW IMAGE USING THE ingredients AS THE PREFAB AND spawnParent AS THE PARENT TRANSFORM, WHICH MEANS THE NEW IMAGE WILL BE SPAWNED AS A CHILD OF THE spawnParent TRANSFORM IN THE HIERARCHY
            spawnedImage.GetComponent<RectTransform>().anchoredPosition = spawnPosition; // THIS LINE OF CODE SETS THE ANCHORED POSITION OF THE SPAWNED IMAGE TO THE spawnPosition OBTAINED FROM THE BasketFunction SCRIPT, WHICH PLACES THE IMAGE IN THE CORRECT LOCATION IN THE BASKET
            spawnedBasketImages.Add(spawnedImage.gameObject); // add to list instead
            spawnedPositions.Add(spawnPosition); // track its position too
            inventoryManager.AddItem(itemName, quantity, sprite); // THIS LINE OF CODE ADDS THE ITEM TO THE INVENTORY USING THE AddItem() FUNCTION FROM THE InventoryManager SCRIPT, PASSING IN THE itemName, quantity, AND sprite AS PARAMETERS
            count++; // THIS LINE OF CODE INCREMENTS THE count VARIABLE, WHICH CAN BE USED TO TRACK HOW MANY TIMES AN ITEM HAS BEEN PICKED UP OR USED, AND TO CONTROL THE SPAWNING AND REMOVAL OF INGREDIENTS IN THE BASKET BASED ON THIS COUNT
        }
    }

    public void OnIngredientUsed()
    {
        count--;
        Debug.Log(count);
        if (count <= 0)
        {
            // Destroy all spawned images of this ingredient
            foreach (GameObject image in spawnedBasketImages)
            {
                if (image != null)
                    Destroy(image);
            }
            // Free all their positions
            foreach (Vector2 position in spawnedPositions)
            {
                basketFunction.RemoveIngredient(position);
            }
            spawnedBasketImages.Clear();
            spawnedPositions.Clear();
            count = 0;
        }
        else if (count > 0 && count <= 2)
        {
            // Only remove the most recently added one
            int last = spawnedBasketImages.Count - 1;
            if (last >= 0 && spawnedBasketImages[last] != null)
            {
                Destroy(spawnedBasketImages[last]);
                basketFunction.RemoveIngredient(spawnedPositions[last]);
                spawnedBasketImages.RemoveAt(last);
                spawnedPositions.RemoveAt(last);
            }
        }
    }

}
