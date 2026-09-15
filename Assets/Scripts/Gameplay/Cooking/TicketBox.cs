using System.Collections.Generic;
using UnityEngine;

public class TicketBox : MonoBehaviour
{
    public static TicketBox Instance;

    [Header("Settings")]
    public int maxTickets = 5;

    [Header("References")]
    public SpriteRenderer ticketBoxSprite;

    private List<string> tickets = new List<string>();

    public System.Action OnTicketAdded;
    public System.Action OnTicketRemoved;
    public System.Action OnTicketBoxClicked;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddTicket(string dishName)
    {
        if (tickets.Count >= maxTickets)
        {
            Debug.Log("[TicketBox] Ticket box is full!");
            return false;
        }

        tickets.Add(dishName);
        OnTicketAdded?.Invoke();
        Debug.Log($"[TicketBox] Added ticket: {dishName} ({tickets.Count}/{maxTickets})");
        return true;
    }

    public bool RemoveTicket(string dishName)
    {
        if (tickets.Remove(dishName))
        {
            OnTicketRemoved?.Invoke();
            Debug.Log($"[TicketBox] Removed ticket: {dishName} ({tickets.Count}/{maxTickets})");
            return true;
        }
        return false;
    }

    public List<string> GetTickets()
    {
        return new List<string>(tickets);
    }

    public bool IsFull()
    {
        return tickets.Count >= maxTickets;
    }

    public bool HasTickets()
    {
        return tickets.Count > 0;
    }

    public int GetTicketCount()
    {
        return tickets.Count;
    }

    public void ClearTickets()
    {
        tickets.Clear();
        OnTicketRemoved?.Invoke();
    }

    private void OnMouseDown()
    {
        if (tickets.Count == 0)
        {
            Debug.Log("[TicketBox] No tickets to display.");
            return;
        }

        OnTicketBoxClicked?.Invoke();
    }
}
