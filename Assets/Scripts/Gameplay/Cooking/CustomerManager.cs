using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Settings")]
    public int maxCustomers = 2;
    public float spawnInterval = 30f;
    public float patienceTime = 60f;

    [Header("Customer Sprites")]
    public Sprite[] customerSprites;

    [Header("Dish Orders")]
    public string[] dishOrders = new string[] { "Adobo", "Sinigang", "Sisig" };

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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCustomerSystem();
    }

    public void StartCustomerSystem()
    {
        isRunning = true;
        spawnTimer = spawnInterval;

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
            TrySpawnCustomer();
        }
    }

    private void SpawnInitialCustomers()
    {
        int spawned = 0;
        foreach (var slot in windowSlots)
        {
            if (spawned >= maxCustomers) break;

            SpawnCustomerInSlot(slot);
            spawned++;
        }
    }

    private void TrySpawnCustomer()
    {
        foreach (var slot in windowSlots)
        {
            if (slot.CanSpawn())
            {
                SpawnCustomerInSlot(slot);
            }
        }
    }

    private void SpawnCustomerInSlot(WindowSlot slot)
    {
        if (slot == null || !slot.CanSpawn()) return;

        int randomSpriteIndex = Random.Range(0, customerSprites.Length);
        int randomDishIndex = Random.Range(0, dishOrders.Length);

        Sprite randomSprite = customerSprites[randomSpriteIndex];
        string randomDish = dishOrders[randomDishIndex];
        Sprite[] meterSprites = GetMeterSprites(randomDish);

        if (meterSprites == null || meterSprites.Length == 0) return;

        GameObject customerObj = Instantiate(customerPrefab);
        Customer newCustomer = customerObj.GetComponent<Customer>();

        if (newCustomer == null)
        {
            Destroy(customerObj);
            return;
        }

        newCustomer.OnCustomerServed += HandleCustomerServed;
        newCustomer.OnCustomerLeft += HandleCustomerLeft;

        newCustomer.Setup(0, randomSprite, randomDish, slot.GetSpawnPosition(), meterSprites, patienceTime);
        slot.AssignCustomer(newCustomer);
        activeCustomers.Add(newCustomer);
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
}
