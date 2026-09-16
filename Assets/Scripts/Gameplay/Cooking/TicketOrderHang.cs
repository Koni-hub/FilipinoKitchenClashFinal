using System.Collections.Generic;
using UnityEngine;

public class TicketOrderHang : MonoBehaviour
{
    public static TicketOrderHang Instance;

    [Header("Settings")]
    public int maxSlots = 2;

    [Header("Hang Slots")]
    public Transform[] hangSlots;

    [Header("References")]
    public Transform ticketOrderSprite;

    private List<ActiveTicket> activeTickets = new List<ActiveTicket>();

    public System.Action OnTicketHung;
    public System.Action OnTicketRemoved;

    private struct ActiveTicket
    {
        public string dishName;
        public Sprite dishSprite;
        public GameObject ticketVisual;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (hangSlots == null || hangSlots.Length == 0)
        {
            CreateHangSlots();
        }
    }

    private void CreateHangSlots()
    {
        hangSlots = new Transform[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = new GameObject("HangSlot_" + i);
            slot.transform.SetParent(transform);
            slot.transform.localPosition = new Vector3(i * 0.5f - 0.25f, 0, 0);
            hangSlots[i] = slot.transform;
        }
    }

    public bool AddTicket(string dishName, Sprite dishSprite)
    {
        if (activeTickets.Count >= maxSlots)
        {
            Debug.Log("[TicketOrderHang] All hang slots are full!");
            return false;
        }

        int slotIndex = activeTickets.Count;

        GameObject ticketObj = new GameObject("Ticket_" + dishName);
        ticketObj.transform.SetParent(hangSlots[slotIndex]);
        ticketObj.transform.localPosition = new Vector3(0f, -1.33f, 0f);
        ticketObj.transform.localScale = new Vector3(0.0375f, 0.041f, 1f);

        SpriteRenderer sr = ticketObj.AddComponent<SpriteRenderer>();
        sr.sprite = dishSprite;
        sr.sortingOrder = 15;

        ActiveTicket newTicket = new ActiveTicket
        {
            dishName = dishName,
            dishSprite = dishSprite,
            ticketVisual = ticketObj
        };

        activeTickets.Add(newTicket);
        OnTicketHung?.Invoke();

        Debug.Log($"[TicketOrderHang] Hung ticket: {dishName} at slot {slotIndex} ({activeTickets.Count}/{maxSlots})");
        return true;
    }

    public bool RemoveTicket(string dishName)
    {
        for (int i = 0; i < activeTickets.Count; i++)
        {
            if (activeTickets[i].dishName == dishName)
            {
                if (activeTickets[i].ticketVisual != null)
                    Destroy(activeTickets[i].ticketVisual);

                activeTickets.RemoveAt(i);
                OnTicketRemoved?.Invoke();

                Debug.Log($"[TicketOrderHang] Removed ticket: {dishName} ({activeTickets.Count}/{maxSlots})");
                return true;
            }
        }
        return false;
    }

    public List<string> GetActiveTicketNames()
    {
        List<string> names = new List<string>();
        foreach (var ticket in activeTickets)
        {
            names.Add(ticket.dishName);
        }
        return names;
    }

    public bool HasTicket(string dishName)
    {
        foreach (var ticket in activeTickets)
        {
            if (ticket.dishName == dishName)
                return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return activeTickets.Count >= maxSlots;
    }

    public bool HasActiveTickets()
    {
        return activeTickets.Count > 0;
    }

    public int GetActiveCount()
    {
        return activeTickets.Count;
    }
}
