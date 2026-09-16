using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Settings")]
    public int maxCustomersAtOnce = 2;
    public float spawnInterval = 5f;
    public float patienceTime = 60f;

    [Header("Customer Sprites")]
    public Sprite[] customerSprites;

    [Header("Dish Orders")]
    public string[] dishOrders = new string[] { "Adobo", "Sisig", "Sinigang" };

    [Header("Dish Sprites")]
    public Sprite adoboDishSprite;
    public Sprite sisigDishSprite;
    public Sprite sinigangDishSprite;

    [Header("Meter Sprites")]
    public Sprite[] adoboMeter;
    public Sprite[] sisigMeter;
    public Sprite[] sinigangMeter;

    [Header("Window Slots")]
    public WindowSlot[] windowSlots;

    [Header("Customer Prefab")]
    public GameObject customerPrefab;

    private List<Customer> activeCustomers = new List<Customer>();
    private float spawnTimer;
    private bool isRunning = false;
    private int nextCustomerIndex = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadDishSprites();
        LoadMeterSprites();
        StartCustomerSystem();
    }

    private void LoadDishSprites()
    {
        Sprite loadedAdobo = Resources.Load<Sprite>("Art/CookingArea/ticket order/ADOBO");
        Sprite loadedSisig = Resources.Load<Sprite>("Art/CookingArea/ticket order/SISIG");
        Sprite loadedSinigang = Resources.Load<Sprite>("Art/CookingArea/ticket order/SINIGANG");

        if (loadedAdobo != null) adoboDishSprite = loadedAdobo;
        if (loadedSisig != null) sisigDishSprite = loadedSisig;
        if (loadedSinigang != null) sinigangDishSprite = loadedSinigang;
    }

    private void LoadMeterSprites()
    {
        if (adoboMeter == null || adoboMeter.Length == 0)
            adoboMeter = LoadMeterSpritesFromFolder("Art/CookingArea/adobo meter", 6);

        if (sisigMeter == null || sisigMeter.Length == 0)
            sisigMeter = LoadMeterSpritesFromFolder("Art/CookingArea/sisig meter", 6);

        if (sinigangMeter == null || sinigangMeter.Length == 0)
            sinigangMeter = LoadMeterSpritesFromFolder("Art/CookingArea/sinigang meter", 6);
    }

    private Sprite[] LoadMeterSpritesFromFolder(string folderPath, int count)
    {
        List<Sprite> sprites = new List<Sprite>();

        for (int i = 1; i <= count; i++)
        {
            Sprite sprite = Resources.Load<Sprite>($"{folderPath}/mt {i}");
            if (sprite != null)
                sprites.Add(sprite);
        }

        return sprites.ToArray();
    }

    public void StartCustomerSystem()
    {
        isRunning = true;
        spawnTimer = spawnInterval;
        nextCustomerIndex = 0;

        SpawnInitialCustomers();
    }

    public void StopCustomerSystem()
    {
        isRunning = false;

        foreach (var customer in activeCustomers)
        {
            if (customer != null)
                customer.gameObject.SetActive(false);
        }
        activeCustomers.Clear();

        foreach (var slot in windowSlots)
        {
            slot.ClearSlot();
            slot.isOnCooldown = false;
            slot.cooldownTimer = 0f;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnCustomers();
        }
    }

    private void SpawnInitialCustomers()
    {
        int spawned = 0;
        foreach (var slot in windowSlots)
        {
            if (spawned >= maxCustomersAtOnce) break;

            if (slot.CanSpawn())
            {
                SpawnCustomerInSlot(slot);
                spawned++;
            }
        }
    }

    private void TrySpawnCustomers()
    {
        if (activeCustomers.Count >= maxCustomersAtOnce) return;

        foreach (var slot in windowSlots)
        {
            if (activeCustomers.Count >= maxCustomersAtOnce) break;

            if (slot.CanSpawn())
            {
                SpawnCustomerInSlot(slot);
            }
        }
    }

    private void SpawnCustomerInSlot(WindowSlot slot)
    {
        if (slot == null || !slot.CanSpawn()) return;
        if (customerPrefab == null)
        {
            Debug.LogError("[CustomerManager] Customer prefab is not assigned!");
            return;
        }

        int spriteIndex = nextCustomerIndex % customerSprites.Length;
        int randomDishIndex = Random.Range(0, dishOrders.Length);

        Sprite randomSprite = customerSprites[spriteIndex];
        string randomDish = dishOrders[randomDishIndex];
        Sprite dishSprite = GetDishSprite(randomDish);
        Sprite[] meterSprites = GetMeterSprites(randomDish);

        if (meterSprites == null || meterSprites.Length == 0) return;

        GameObject customerObj = Instantiate(customerPrefab);

        float targetHeight = 546f / 100f * 0.55f;
        float spriteWorldHeight = randomSprite.rect.height / 100f;
        float finalScale = targetHeight / spriteWorldHeight;
        customerObj.transform.localScale = new Vector3(finalScale, finalScale, 1f);

        Customer newCustomer = customerObj.GetComponent<Customer>();

        if (newCustomer == null)
        {
            Destroy(customerObj);
            return;
        }

        newCustomer.OnCustomerServed += HandleCustomerServed;
        newCustomer.OnCustomerLeft += HandleCustomerLeft;

        newCustomer.Setup(nextCustomerIndex, randomSprite, randomDish, dishSprite, slot.GetSpawnPosition(), meterSprites, patienceTime);

        if (newCustomer.patienceMeter != null)
        {
            Transform meterTransform = newCustomer.patienceMeter.transform;
            if (slot.slotIndex == 0)
                meterTransform.localPosition = new Vector3(2.1f, 3.21f, 0f);
            else
                meterTransform.localPosition = new Vector3(1.98f, 3.06f, 0f);
        }

        nextCustomerIndex++;

        slot.AssignCustomer(newCustomer);
        activeCustomers.Add(newCustomer);
    }

    private Sprite GetDishSprite(string dishName)
    {
        switch (dishName)
        {
            case "Adobo":
                return adoboDishSprite;
            case "Sisig":
                return sisigDishSprite;
            case "Sinigang":
                return sinigangDishSprite;
            default:
                return adoboDishSprite;
        }
    }

    private Sprite[] GetMeterSprites(string dishName)
    {
        switch (dishName)
        {
            case "Adobo":
                return adoboMeter;
            case "Sisig":
                return sisigMeter;
            case "Sinigang":
                return sinigangMeter;
            default:
                return adoboMeter;
        }
    }

    private void HandleCustomerServed(Customer customer)
    {
        RemoveCustomer(customer);
        spawnTimer = spawnInterval;
    }

    private void HandleCustomerLeft(Customer customer)
    {
        RemoveCustomer(customer);
        spawnTimer = spawnInterval;
    }

    private void RemoveCustomer(Customer customer)
    {
        activeCustomers.Remove(customer);

        foreach (var slot in windowSlots)
        {
            if (slot.currentCustomer == customer)
            {
                slot.ClearSlot();
                break;
            }
        }

        customer.OnCustomerServed -= HandleCustomerServed;
        customer.OnCustomerLeft -= HandleCustomerLeft;
    }

    public bool ServeDish(string dishName)
    {
        foreach (var slot in windowSlots)
        {
            if (slot.isOccupied && slot.HasOrder(dishName))
            {
                slot.currentCustomer.ServeCustomer();
                return true;
            }
        }
        return false;
    }

    public int GetActiveCustomerCount()
    {
        return activeCustomers.Count;
    }

    public int GetMaxCustomers()
    {
        return maxCustomersAtOnce;
    }
}
