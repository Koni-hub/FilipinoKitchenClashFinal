using System;
using UnityEngine;

public class CookingSync : MonoBehaviour
{
    public static CookingSync Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameMessageReceived += HandleGameMessage;
    }

    private void OnDisable()
    {
        GameManager.OnGameMessageReceived -= HandleGameMessage;
    }

    private void HandleGameMessage(string msg)
    {
        if (msg.StartsWith(RoomProtocol.COOK_CUSTOMER_SPAWN + "|"))
            HandleCustomerSpawn(msg);
        else if (msg.StartsWith(RoomProtocol.COOK_TAKE_ORDER + "|"))
            HandleTakeOrder(msg);
        else if (msg.StartsWith(RoomProtocol.COOK_ADD_TO_POT + "|"))
            HandleAddToPot(msg);
        else if (msg.StartsWith(RoomProtocol.COOK_SERVE_DISH + "|"))
            HandleServeDish(msg);
        else if (msg.StartsWith(RoomProtocol.SCORE_UPDATE + "|"))
            HandleScoreUpdate(msg);
    }

    public void SendCustomerSpawn(int customerId, string dishName, int slotIndex)
    {
        string msg = $"{RoomProtocol.COOK_CUSTOMER_SPAWN}|{customerId}|{dishName}|{slotIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[CookingSync] Sent customer spawn: {msg}");
    }

    public void SendTakeOrder(int customerId)
    {
        string msg = $"{RoomProtocol.COOK_TAKE_ORDER}|{customerId}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[CookingSync] Sent take order: {msg}");
    }

    public void SendAddToPot(string ingredientId, int potIndex)
    {
        string msg = $"{RoomProtocol.COOK_ADD_TO_POT}|{ingredientId}|{potIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[CookingSync] Sent add to pot: {msg}");
    }

    public void SendServeDish(string dishName, int customerId)
    {
        string msg = $"{RoomProtocol.COOK_SERVE_DISH}|{dishName}|{customerId}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[CookingSync] Sent serve dish: {msg}");
    }

    public void SendScoreUpdate(int hostScore, int clientScore)
    {
        string msg = $"{RoomProtocol.SCORE_UPDATE}|{hostScore}|{clientScore}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[CookingSync] Sent score update: {msg}");
    }

    private void HandleCustomerSpawn(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 4) return;

        int customerId = int.Parse(parts[1]);
        string dishName = parts[2];
        int slotIndex = int.Parse(parts[3]);

        Debug.Log($"[CookingSync] Remote customer spawn: id={customerId} dish={dishName} slot={slotIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (CustomerManager.Instance != null)
            {
                WindowSlot[] slots = CustomerManager.Instance.windowSlots;
                if (slots != null && slotIndex < slots.Length)
                {
                    WindowSlot slot = slots[slotIndex];
                    if (slot.CanSpawn())
                    {
                        CustomerManager.Instance.SpawnCustomerInSlot(slot, customerId);
                    }
                }
            }
        });
    }

    private void HandleTakeOrder(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 2) return;

        int customerId = int.Parse(parts[1]);

        Debug.Log($"[CookingSync] Remote take order: customer {customerId}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (CustomerManager.Instance != null)
            {
                foreach (var slot in CustomerManager.Instance.windowSlots)
                {
                    if (slot.isOccupied && slot.currentCustomer != null)
                    {
                        if (slot.currentCustomer.customerID == customerId)
                        {
                            slot.currentCustomer.OnOrderTaken();
                            break;
                        }
                    }
                }
            }
        });
    }

    private void HandleAddToPot(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 3) return;

        string ingredientId = parts[1];
        int potIndex = int.Parse(parts[2]);

        Debug.Log($"[CookingSync] Remote add to pot: {ingredientId} in pot {potIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            GameObject ingredient = PreppingSync.Instance?.GetSyncedObject(ingredientId);
            if (ingredient != null)
            {
                GameObject pot = GameObject.Find("pot with cover");
                if (pot != null)
                {
                    ingredient.transform.SetParent(pot.transform);
                    Vector3 potPos = pot.transform.position;
                    ingredient.transform.position = new Vector3(potPos.x, potPos.y, potPos.z - 0.1f);
                }
            }
        });
    }

    private void HandleServeDish(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 3) return;

        string dishName = parts[1];
        int customerId = int.Parse(parts[2]);

        Debug.Log($"[CookingSync] Remote serve dish: {dishName} to customer {customerId}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (CustomerManager.Instance != null)
            {
                CustomerManager.Instance.ServeDish(dishName);
            }
        });
    }

    private void HandleScoreUpdate(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 3) return;

        int hostScore = int.Parse(parts[1]);
        int clientScore = int.Parse(parts[2]);

        Debug.Log($"[CookingSync] Score update: Host={hostScore}, Client={clientScore}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SetScores(hostScore, clientScore);
            }
        });
    }
}
