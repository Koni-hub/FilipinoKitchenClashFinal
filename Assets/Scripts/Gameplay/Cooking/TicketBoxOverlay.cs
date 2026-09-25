using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicketBoxOverlay : MonoBehaviour
{
    public static TicketBoxOverlay Instance;

    [Header("References")]
    public Canvas overlayCanvas;
    public GameObject overlayPanel;
    public Transform ticketContainer;
    public GameObject ticketButtonPrefab;

    [Header("Dish Sprites")]
    public Sprite adoboSprite;
    public Sprite sisigSprite;
    public Sprite sinigangSprite;

    [Header("UI Settings")]
    public Color ticketColor = new Color(0.95f, 0.9f, 0.75f, 1f);
    public Color hoverColor = new Color(1f, 0.95f, 0.85f, 1f);
    public Color selectedColor = new Color(0.8f, 0.9f, 0.7f, 1f);

    private Dictionary<string, Sprite> dishSpriteMap = new Dictionary<string, Sprite>();
    private bool isOpen = false;

    public System.Action<string> OnTicketSelected;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeDishSprites();

        if (overlayCanvas != null)
            overlayCanvas.gameObject.SetActive(false);
    }

    private void InitializeDishSprites()
    {
        Sprite loadedAdobo = Resources.Load<Sprite>("Art/CookingArea/ticket order/ADOBO");
        Sprite loadedSisig = Resources.Load<Sprite>("Art/CookingArea/ticket order/SISIG");
        Sprite loadedSinigang = Resources.Load<Sprite>("Art/CookingArea/ticket order/SINIGANG");

        if (loadedAdobo != null) adoboSprite = loadedAdobo;
        if (loadedSisig != null) sisigSprite = loadedSisig;
        if (loadedSinigang != null) sinigangSprite = loadedSinigang;

        dishSpriteMap["Adobo"] = adoboSprite;
        dishSpriteMap["Sisig"] = sisigSprite;
        dishSpriteMap["Sinigang"] = sinigangSprite;
    }

    public void Show()
    {
        if (isOpen) return;

        if (TicketBox.Instance == null || !TicketBox.Instance.HasTickets())
        {
            Debug.Log("[TicketBoxOverlay] No tickets to display.");
            return;
        }

        isOpen = true;

        if (overlayCanvas != null)
            overlayCanvas.gameObject.SetActive(true);

        if (overlayPanel != null)
            overlayPanel.SetActive(true);

        PopulateTickets();

        Debug.Log("[TicketBoxOverlay] Overlay opened.");
    }

    public void Hide()
    {
        isOpen = false;

        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        if (overlayCanvas != null)
            overlayCanvas.gameObject.SetActive(false);

        ClearTickets();

        Debug.Log("[TicketBoxOverlay] Overlay closed.");
    }

    private void PopulateTickets()
    {
        ClearTickets();

        if (TicketBox.Instance == null) return;

        List<string> tickets = TicketBox.Instance.GetTickets();

        foreach (string dishName in tickets)
        {
            CreateTicketButton(dishName);
        }
    }

    private void CreateTicketButton(string dishName)
    {
        if (ticketContainer == null) return;

        GameObject ticketBtn = new GameObject("Ticket_" + dishName, typeof(RectTransform));
        ticketBtn.transform.SetParent(ticketContainer, false);

        RectTransform rectTransform = ticketBtn.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(250f, 250f);

        LayoutElement ticketLE = ticketBtn.AddComponent<LayoutElement>();
        ticketLE.preferredWidth = 250f;
        ticketLE.preferredHeight = 250f;

        Image bgImage = ticketBtn.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.01f);

        Button button = ticketBtn.AddComponent<Button>();
        button.targetGraphic = bgImage;

        ColorBlock colors = button.colors;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = selectedColor;
        button.colors = colors;

        string capturedDishName = dishName;
        button.onClick.AddListener(() => OnTicketButtonClicked(capturedDishName));

        CreateTicketContent(ticketBtn.transform, dishName);
    }

    private void CreateTicketContent(Transform parent, string dishName)
    {
        if (!dishSpriteMap.ContainsKey(dishName) || dishSpriteMap[dishName] == null) return;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        iconObj.transform.SetParent(parent, false);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(250f, 250f);

        LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 250f;
        iconLE.preferredHeight = 250f;

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = dishSpriteMap[dishName];
        iconImage.preserveAspect = true;
    }

    private void OnTicketButtonClicked(string dishName)
    {
        Debug.Log($"[TicketBoxOverlay] Ticket selected: {dishName}");

        if (TicketOrderHang.Instance != null)
        {
            if (TicketOrderHang.Instance.IsFull())
            {
                Debug.Log("[TicketBoxOverlay] Ticket order hang is full! Remove a ticket first.");
                return;
            }

            Sprite dishSprite = null;
            if (dishSpriteMap.ContainsKey(dishName))
                dishSprite = dishSpriteMap[dishName];

            bool hung = TicketOrderHang.Instance.AddTicket(dishName, dishSprite);

            if (hung)
            {
                TicketBox.Instance.RemoveTicket(dishName);
                OnTicketSelected?.Invoke(dishName);
            }
        }

        if (TicketBox.Instance != null && TicketBox.Instance.HasTickets())
        {
            PopulateTickets();
        }
        else
        {
            Hide();
        }
    }

    private void ClearTickets()
    {
        if (ticketContainer == null) return;

        foreach (Transform child in ticketContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
