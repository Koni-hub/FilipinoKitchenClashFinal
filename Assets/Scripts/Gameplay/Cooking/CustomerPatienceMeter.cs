using UnityEngine;

public class CustomerPatienceMeter : MonoBehaviour
{
    [Header("Customer Info")]
    public string dishOrder;
    public Sprite dishSprite;

    [Header("Visual Feedback")]
    public Color hoverColor = new Color(0.8f, 0.8f, 1f, 1f);
    public Color normalColor = Color.white;
    public Color clickedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private SpriteRenderer spriteRenderer;
    private bool isOrderTaken = false;
    private Customer customer;

    public System.Action<string, Sprite> OnOrderTaken;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(Customer parentCustomer, string order, Sprite dishSpriteParam)
    {
        customer = parentCustomer;
        dishOrder = order;
        dishSprite = dishSpriteParam;
        isOrderTaken = false;

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void OnMouseEnter()
    {
        if (isOrderTaken) return;

        if (spriteRenderer != null)
            spriteRenderer.color = hoverColor;
    }

    private void OnMouseExit()
    {
        if (isOrderTaken) return;

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void OnMouseDown()
    {
        if (isOrderTaken) return;

        if (TicketBox.Instance == null)
        {
            Debug.Log("[CustomerPatienceMeter] TicketBox instance not found!");
            return;
        }

        if (TicketBox.Instance.IsFull())
        {
            Debug.Log("[CustomerPatienceMeter] Ticket box is full! Cannot add more orders.");
            return;
        }

        isOrderTaken = true;

        if (spriteRenderer != null)
            spriteRenderer.color = clickedColor;

        bool added = TicketBox.Instance.AddTicket(dishOrder);

        if (added)
        {
            OnOrderTaken?.Invoke(dishOrder, dishSprite);
            Debug.Log($"[CustomerPatienceMeter] Order '{dishOrder}' sent to ticket box.");

            if (customer != null)
            {
                customer.OnOrderTaken();
            }
        }
        else
        {
            isOrderTaken = false;
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }
    }

    public bool IsOrderTaken()
    {
        return isOrderTaken;
    }

    public void ResetMeter()
    {
        isOrderTaken = false;
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }
}
