using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatchGameSetup : MonoBehaviour
{
    [Header("Adobo Ingredients")]
    public Sprite adoboOnion;
    public Sprite adoboGarlic;
    public Sprite adoboTomato;
    public Sprite adoboLaurel;
    public Sprite adoboBlackPepper;
    public Sprite adoboSalt;
    public Sprite adoboPorkCubes;
    public Sprite adoboSoySauce;
    public Sprite adoboVinegar;
    public Sprite adoboBrownSugar;

    [Header("Sinigang Ingredients")]
    public Sprite sinigangOnion;
    public Sprite sinigangTomato;
    public Sprite sinigangLabanos;
    public Sprite sinigangGreenChillies;
    public Sprite sinigangKangkong;
    public Sprite sinigangSitaw;
    public Sprite sinigangOkra;
    public Sprite sinigangTalong;
    public Sprite sinigangPorkRibs;
    public Sprite sinigangPepper;
    public Sprite sinigangSalt;
    public Sprite sinigangSampalok;
    public Sprite sinigangSoySauce;
    public Sprite sinigangVinegar;
    public Sprite sinigangOliveOil;

    [Header("Sisig Ingredients")]
    public Sprite sisigOnion;
    public Sprite sisigGarlic;
    public Sprite sisigGreenChillies;
    public Sprite sisigCalamansi;
    public Sprite sisigKnorrSeasoning;
    public Sprite sisigSalt;
    public Sprite sisigPepper;
    public Sprite sisigOliveOil;

    [Header("Optional: Drag your BASKET here")]
    public Transform basketTransform;

    private IngredientSpawner spawner;
    private CatchPlayer player;
    private CatchGameManager gameManager;
    private string currentDish = "Adobo";
    private bool dishSelected = false;

    private string[] adoboNames = { "Onion", "Garlic", "Tomato", "LaurelLeaves", "Black Pepper", "Salt", "Pork Cubes", "Soy Sauce", "Vinegar", "Brown Sugar" };
    private string[] sinigangNames = { "Onion", "Tomato", "Labanos", "Green Chillies", "Kangkong", "Sitaw", "Okra", "Talong", "Pork Ribs", "Pepper", "Salt", "Sampalok", "Soy Sauce", "Vinegar", "Olive Oil" };
    private string[] sisigNames = { "Onion", "Garlic", "Green Chillies", "Calamansi", "Knorr Seasoning", "Salt", "Pepper", "Olive Oil" };

    private GameObject startButtonObj;
    private TMP_Text startBtnText;
    private Dictionary<string, Image> dishButtons = new Dictionary<string, Image>();
    private GameObject stopButtonObj;

    void Awake()
    {
        SetupGame();
    }

    void SetupGame()
    {
        // Player
        GameObject playerObj = new GameObject("CatchPlayer");
        playerObj.transform.position = new Vector3(4f, -2.72f, 0f);
        player = playerObj.AddComponent<CatchPlayer>();
        if (basketTransform != null)
            player.basketVisual = basketTransform;

        // Spawner
        GameObject spawnerObj = new GameObject("Spawner");
        spawner = spawnerObj.AddComponent<IngredientSpawner>();

        // Ingredient prefab
        GameObject prefab = new GameObject("IngredientPrefab");
        prefab.transform.SetParent(transform);
        prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent<FallingIngredient>();
        prefab.SetActive(false);
        spawner.ingredientPrefab = prefab;

        // Set default Adobo ingredients
        spawner.SetIngredients(adoboNames, GetAdoboSprites());

        // UI Canvas
        GameObject canvasObj = new GameObject("GameUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Dish Buttons + Start Button in a row at top
        CreateDishButtons(canvasObj.transform);

        // Stop Button next to "Go to prepping area" button (bottom-left)
        CreateStopButton(canvasObj.transform);

        // Game Manager
        GameObject managerObj = new GameObject("CatchGameManager");
        gameManager = managerObj.AddComponent<CatchGameManager>();
        gameManager.timerText = null;
        gameManager.player = player;
        gameManager.spawner = spawner;

        // Create initial Adobo caught display
        gameManager.SwitchDish("Adobo", adoboNames, GetAdoboSprites());

        Debug.Log("Catch Game ready! Drag ingredient sprites in Inspector.");
    }

    private Sprite[] GetAdoboSprites()
    {
        return new Sprite[] { adoboOnion, adoboGarlic, adoboTomato, adoboLaurel, adoboBlackPepper, adoboSalt, adoboPorkCubes, adoboSoySauce, adoboVinegar, adoboBrownSugar };
    }

    private Sprite[] GetSinigangSprites()
    {
        return new Sprite[] { sinigangOnion, sinigangTomato, sinigangLabanos, sinigangGreenChillies, sinigangKangkong, sinigangSitaw, sinigangOkra, sinigangTalong, sinigangPorkRibs, sinigangPepper, sinigangSalt, sinigangSampalok, sinigangSoySauce, sinigangVinegar, sinigangOliveOil };
    }

    private Sprite[] GetSisigSprites()
    {
        return new Sprite[] { sisigOnion, sisigGarlic, sisigGreenChillies, sisigCalamansi, sisigKnorrSeasoning, sisigSalt, sisigPepper, sisigOliveOil };
    }

    private void CreateDishButtons(Transform parent)
    {
        string[] dishNames = { "Adobo", "Sinigang", "Sisig" };
        float btnWidth = 150f;
        float btnHeight = 50f;
        float spacing = 15f;
        float startBtnWidth = 200f;

        float totalWidth = dishNames.Length * btnWidth + (dishNames.Length - 1) * spacing + spacing + startBtnWidth;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < dishNames.Length; i++)
        {
            GameObject btnObj = new GameObject("DishBtn_" + dishNames[i]);
            btnObj.transform.SetParent(parent, false);
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            Button btn = btnObj.AddComponent<Button>();
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1f);
            btnRt.anchorMax = new Vector2(0.5f, 1f);
            btnRt.anchoredPosition = new Vector2(startX + i * (btnWidth + spacing), -40f);
            btnRt.sizeDelta = new Vector2(btnWidth, btnHeight);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TMP_Text txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = dishNames[i];
            txt.fontSize = 26;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            string dishName = dishNames[i];
            dishButtons[dishName] = btnBg;
            btn.onClick.AddListener(() => OnDishButtonClicked(dishName));
        }

        // Start Button - always visible, right after dish buttons
        float startBtnX = startX + dishNames.Length * btnWidth + (dishNames.Length - 1) * spacing + spacing * 3;
        startButtonObj = new GameObject("StartButton");
        startButtonObj.transform.SetParent(parent, false);
        Image startBg = startButtonObj.AddComponent<Image>();
        startBg.color = new Color(0.2f, 0.7f, 0.2f, 1f);
        Button startBtn = startButtonObj.AddComponent<Button>();
        RectTransform startRt = startButtonObj.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.5f, 1f);
        startRt.anchorMax = new Vector2(0.5f, 1f);
        startRt.anchoredPosition = new Vector2(startBtnX, -40f);
        startRt.sizeDelta = new Vector2(startBtnWidth, btnHeight);

        GameObject startTextObj = new GameObject("Text");
        startTextObj.transform.SetParent(startButtonObj.transform, false);
        startBtnText = startTextObj.AddComponent<TextMeshProUGUI>();
        startBtnText.text = "START";
        startBtnText.fontSize = 28;
        startBtnText.color = Color.white;
        startBtnText.alignment = TextAlignmentOptions.Center;
        RectTransform startTxtRt = startTextObj.GetComponent<RectTransform>();
        startTxtRt.anchorMin = Vector2.zero;
        startTxtRt.anchorMax = Vector2.one;
        startTxtRt.sizeDelta = Vector2.zero;

        startBtn.onClick.AddListener(OnStartButtonClicked);
    }

    private void CreateStopButton(Transform parent)
    {
        float stopBtnWidth = 150f;
        float stopBtnHeight = 50f;

        stopButtonObj = new GameObject("StopButton");
        stopButtonObj.transform.SetParent(parent, false);
        Image stopBg = stopButtonObj.AddComponent<Image>();
        stopBg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button stopBtn = stopButtonObj.AddComponent<Button>();
        RectTransform stopRt = stopButtonObj.GetComponent<RectTransform>();
        stopRt.anchorMin = new Vector2(0.5f, 0.5f);
        stopRt.anchorMax = new Vector2(0.5f, 0.5f);
        stopRt.anchoredPosition = new Vector2(-450f, -441f);
        stopRt.sizeDelta = new Vector2(stopBtnWidth, stopBtnHeight);

        GameObject stopTextObj = new GameObject("Text");
        stopTextObj.transform.SetParent(stopButtonObj.transform, false);
        TMP_Text stopTxt = stopTextObj.AddComponent<TextMeshProUGUI>();
        stopTxt.text = "STOP";
        stopTxt.fontSize = 28;
        stopTxt.color = Color.white;
        stopTxt.alignment = TextAlignmentOptions.Center;
        RectTransform stopTxtRt = stopTextObj.GetComponent<RectTransform>();
        stopTxtRt.anchorMin = Vector2.zero;
        stopTxtRt.anchorMax = Vector2.one;
        stopTxtRt.sizeDelta = Vector2.zero;

        stopBtn.onClick.AddListener(OnStopButtonClicked);
    }

    private void OnStopButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.StopGame();
        }
    }

    private void OnDishButtonClicked(string dishName)
    {
        currentDish = dishName;
        dishSelected = true;
        Debug.Log("Dish selected: " + dishName);

        foreach (var kvp in dishButtons)
        {
            if (kvp.Key == dishName)
                kvp.Value.color = new Color(0.2f, 0.7f, 0.2f, 1f);
            else
                kvp.Value.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        }

        switch (dishName)
        {
            case "Adobo":
                gameManager.SwitchDish("Adobo", adoboNames, GetAdoboSprites());
                break;
            case "Sinigang":
                gameManager.SwitchDish("Sinigang", sinigangNames, GetSinigangSprites());
                break;
            case "Sisig":
                gameManager.SwitchDish("Sisig", sisigNames, GetSisigSprites());
                break;
        }
    }

    private void OnStartButtonClicked()
    {
        if (!dishSelected)
        {
            Debug.Log("Select a dish first!");
            return;
        }

        Debug.Log("Start clicked! Game starting...");
        gameManager.StartGame();
    }
}
