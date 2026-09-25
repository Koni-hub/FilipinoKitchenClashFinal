using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class CustomerSystemSetup : MonoBehaviour
{
    private static CustomerSystemSetup instance;

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        GameObject setupObj = new GameObject("CustomerSystemSetup");
        setupObj.AddComponent<CustomerSystemSetup>();
        DontDestroyOnLoad(setupObj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SetupDelayed());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "UI_Cooking_Area")
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(SetupDelayed());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator SetupDelayed()
    {
        yield return new WaitForSeconds(0.2f);

        if (IsCustomerSystemAlreadySetup()) yield break;

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        LoadSprites();
        SetupCustomerSystem();
    }

    private bool IsCustomerSystemAlreadySetup()
    {
        return GameObject.Find("CustomerSystem") != null;
    }

    private void LoadSprites()
    {
        spriteCache.Clear();

        string[] customerNames = { "Customer 1", "Customer 2", "Customer 3", "Customer 4" };
        foreach (string name in customerNames)
        {
            Sprite s = Resources.Load<Sprite>($"Art/CookingArea/{name}");
            if (s != null) spriteCache[name] = s;
        }

        spriteCache["Adobo"] = Resources.Load<Sprite>("Art/CookingArea/ticket order/ADOBO");
        spriteCache["Sisig"] = Resources.Load<Sprite>("Art/CookingArea/ticket order/SISIG");
        spriteCache["Sinigang"] = Resources.Load<Sprite>("Art/CookingArea/ticket order/SINIGANG");

        LoadMeterSprites("Adobo", "Art/CookingArea/adobo meter/mt {0} adobo", 6);
        LoadMeterSprites("Sisig", "Art/CookingArea/sisig meter/mt {0} sisig", 6);
        LoadMeterSprites("Sinigang", "Art/CookingArea/sinigang meter/mt {0} sinigang", 5);
    }

    private void LoadMeterSprites(string dish, string pathFormat, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            string path = string.Format(pathFormat, i);
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null) spriteCache[$"{dish}_meter_{i}"] = s;
        }
    }

    private void SetupCustomerSystem()
    {
        GameObject systemObj = new GameObject("CustomerSystem");
        systemObj.transform.position = Vector3.zero;

        CustomerManager manager = systemObj.AddComponent<CustomerManager>();
        manager.customerSprites = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            string key = $"Customer {i + 1}";
            if (spriteCache.ContainsKey(key))
                manager.customerSprites[i] = spriteCache[key];
        }

        manager.dishOrders = new string[] { "Adobo", "Sisig", "Sinigang" };

        manager.adoboMeter = GetMeterSprites("Adobo", 6);
        manager.sisigMeter = GetMeterSprites("Sisig", 6);
        manager.sinigangMeter = GetMeterSprites("Sinigang", 5);

        manager.adoboDishSprite = spriteCache.ContainsKey("Adobo") ? spriteCache["Adobo"] : null;
        manager.sisigDishSprite = spriteCache.ContainsKey("Sisig") ? spriteCache["Sisig"] : null;
        manager.sinigangDishSprite = spriteCache.ContainsKey("Sinigang") ? spriteCache["Sinigang"] : null;

        CreateWindowSlots(manager);

        GameObject customerPrefab = CreateCustomerPrefab();
        manager.customerPrefab = customerPrefab;

        CreateTicketBox();
        CreateTicketOrderHang();
        CreateOverlayCanvas();
        SetupStationButtons();
    }

    private Sprite[] GetMeterSprites(string dish, int count)
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 1; i <= count; i++)
        {
            string key = $"{dish}_meter_{i}";
            if (spriteCache.ContainsKey(key))
                sprites.Add(spriteCache[key]);
        }
        return sprites.ToArray();
    }

    private void CreateWindowSlots(CustomerManager manager)
    {
        GameObject slotsParent = new GameObject("WindowSlots");

        WindowSlot slot0 = CreateWindowSlot(slotsParent, "WindowSlot_0", 0, new Vector3(2.56f, 1.2089f, 0));
        WindowSlot slot1 = CreateWindowSlot(slotsParent, "WindowSlot_1", 1, new Vector3(5.1075f, 1.2736f, 0));

        manager.windowSlots = new WindowSlot[] { slot0, slot1 };
    }

    private WindowSlot CreateWindowSlot(GameObject parent, string name, int index, Vector3 position)
    {
        GameObject slotObj = new GameObject(name);
        slotObj.transform.SetParent(parent.transform);
        slotObj.transform.position = position;

        WindowSlot slot = slotObj.AddComponent<WindowSlot>();
        slot.slotIndex = index;
        slot.cooldownTime = 30f;

        return slot;
    }

    private GameObject CreateCustomerPrefab()
    {
        GameObject prefab = new GameObject("CustomerPrefab");
        prefab.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(4f, 6f);

        Customer customer = prefab.AddComponent<Customer>();
        customer.spriteRenderer = sr;

        GameObject meterObj = new GameObject("PatienceMeter");
        meterObj.transform.SetParent(prefab.transform);
        meterObj.transform.localPosition = new Vector3(2.1f, 3.21f, 0f);
        meterObj.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

        SpriteRenderer meterSR = meterObj.AddComponent<SpriteRenderer>();
        meterSR.sortingOrder = 11;

        BoxCollider2D meterCollider = meterObj.AddComponent<BoxCollider2D>();
        meterCollider.size = new Vector2(2f, 1.5f);

        CustomerPatienceMeter patienceMeter = meterObj.AddComponent<CustomerPatienceMeter>();
        customer.patienceMeter = patienceMeter;
        customer.meterRenderer = meterSR;

        prefab.SetActive(false);
        return prefab;
    }

    private void CreateTicketBox()
    {
        GameObject ticketBoxObj = new GameObject("TicketBox");
        ticketBoxObj.transform.position = new Vector3(1.07f, 1.04f, 0);
        ticketBoxObj.transform.localScale = new Vector3(0.068f, 0.061f, 1f);

        SpriteRenderer sr = ticketBoxObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = Resources.Load<Sprite>("Art/CookingArea/ticket box");

        BoxCollider2D collider = ticketBoxObj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10.5f, 10.8f);

        ticketBoxObj.AddComponent<TicketBox>();

        TicketBox.Instance.OnTicketBoxClicked += () =>
        {
            if (TicketBoxOverlay.Instance != null)
                TicketBoxOverlay.Instance.Show();
        };
    }

    private void CreateTicketOrderHang()
    {
        GameObject hangObj = new GameObject("TicketOrderHang");
        hangObj.transform.position = new Vector3(-2.06f, 4.22f, 0);
        hangObj.transform.localScale = new Vector3(0.114f, 0.174f, 1f);

        SpriteRenderer sr = hangObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = Resources.Load<Sprite>("Art/CookingArea/ticker order hang");

        TicketOrderHang hang = hangObj.AddComponent<TicketOrderHang>();

        hang.maxSlots = 2;
        hang.hangSlots = new Transform[2];

        Vector3[] slotPositions = new Vector3[]
        {
            new Vector3(-10.614f, 0f, 0f),
            new Vector3(-0.088f, 0f, 0f)
        };

        for (int i = 0; i < 2; i++)
        {
            GameObject slot = new GameObject("HangSlot_" + i);
            slot.transform.SetParent(hangObj.transform);
            slot.transform.localPosition = slotPositions[i];
            hang.hangSlots[i] = slot.transform;
        }
    }

    private void CreateOverlayCanvas()
    {
        GameObject canvasObj = new GameObject("TicketOverlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("OverlayPanel", typeof(RectTransform));
        panel.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.7f);

        Button panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelBg;
        panelBtn.onClick.AddListener(() =>
        {
            if (TicketBoxOverlay.Instance != null)
                TicketBoxOverlay.Instance.Hide();
        });

        GameObject scrollArea = new GameObject("TicketContainer", typeof(RectTransform));
        scrollArea.transform.SetParent(panel.transform, false);

        RectTransform scrollRect = scrollArea.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.2f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRect.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup hlg = scrollArea.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 45f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        TicketBoxOverlay overlay = canvasObj.AddComponent<TicketBoxOverlay>();
        overlay.overlayCanvas = canvas;
        overlay.overlayPanel = panel;
        overlay.ticketContainer = scrollArea.transform;

        panel.SetActive(false);
    }

    private void SetupStationButtons()
    {
        SetupStationButton("bt cooking station", "UI_Cooking_Area");
        SetupStationButton("bt prepping station", "UI_AdoboPreppingArea");
        SetupStationButton("bt mini market", "UI_Mini_Market");
    }

    private void SetupStationButton(string buttonName, string sceneName)
    {
        GameObject btn = GameObject.Find(buttonName);
        if (btn == null)
        {
            Debug.LogWarning($"[CustomerSystemSetup] Button '{buttonName}' not found!");
            return;
        }

        if (btn.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = btn.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = btn.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Vector2 spriteSize = sr.sprite.bounds.size;
                col.size = new Vector2(spriteSize.x * 1.2f, spriteSize.y * 1.2f);
            }
            else
            {
                col.size = new Vector2(12f, 4f);
            }
        }

        StationButton stationBtn = btn.AddComponent<StationButton>();
        stationBtn.targetScene = sceneName;
    }
}
