using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatchGameManager : MonoBehaviour
{
    public static CatchGameManager Instance;

    [Header("UI References")]
    public TMP_Text timerText;

    [Header("Game Objects")]
    public CatchPlayer player;
    public IngredientSpawner spawner;

    private bool isPlaying = false;
    public List<FallingIngredient> activeIngredients = new List<FallingIngredient>();

    private Dictionary<string, Dictionary<string, int>> dishCaughtCounts = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, List<string>> dishCaughtOrders = new Dictionary<string, List<string>>();
    private Dictionary<string, List<GameObject>> dishCaughtRows = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, Dictionary<string, TextMesh>> dishCountTexts = new Dictionary<string, Dictionary<string, TextMesh>>();

    private float gridStartX = -6.98f;
    private float gridStartY = 1.21f;
    private float colSpacing = 1.0f;
    private float rowSpacing = 0.685f;
    private int maxRows = 5;

    private string currentDishName = "";
    private string[] currentDishNames = new string[0];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("CatchGameManager created and persisting.");
        }
        else
            Destroy(gameObject);
    }

    public void StartGame()
    {
        Debug.Log("StartGame called! Spawner: " + (spawner != null ? "OK" : "NULL"));
        isPlaying = true;

        ClearIngredients();

        if (spawner != null)
        {
            spawner.StartSpawning();
            spawner.SetDifficulty(1);
        }

        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    public void StopGame()
    {
        isPlaying = false;

        if (spawner != null)
            spawner.StopSpawning();

        ClearIngredients();

        Debug.Log("Game stopped! All ingredients cleared. Press START to play again.");
    }

    void Update()
    {
        if (!isPlaying) return;

        CleanNullIngredients();
    }

    public void CatchIngredient(string ingredientName)
    {
        if (!dishCaughtCounts.ContainsKey(currentDishName))
            dishCaughtCounts[currentDishName] = new Dictionary<string, int>();

        if (!dishCaughtOrders.ContainsKey(currentDishName))
            dishCaughtOrders[currentDishName] = new List<string>();

        var counts = dishCaughtCounts[currentDishName];
        var order = dishCaughtOrders[currentDishName];

        if (counts.ContainsKey(ingredientName) && counts[ingredientName] >= 5)
            return;

        if (counts.ContainsKey(ingredientName))
            counts[ingredientName]++;
        else
        {
            counts[ingredientName] = 1;
            order.Add(ingredientName);
        }

        UpdateCaughtText(ingredientName);
        RepositionRows();
    }

    public void MissedIngredient()
    {
    }

    public void RegisterIngredient(FallingIngredient ingredient)
    {
        if (!activeIngredients.Contains(ingredient))
            activeIngredients.Add(ingredient);
    }

    public void SwitchDish(string dishName, string[] ingredientNames, Sprite[] ingredientSprites)
    {
        isPlaying = false;

        if (spawner != null) spawner.StopSpawning();

        ClearIngredients();

        currentDishName = dishName;
        currentDishNames = ingredientNames;

        HideAllRows();

        BuildDishGrid(dishName, ingredientNames, ingredientSprites);

        spawner.SetIngredients(ingredientNames, ingredientSprites);
    }

    private void HideAllRows()
    {
        foreach (var kvp in dishCaughtRows)
        {
            foreach (var row in kvp.Value)
            {
                if (row != null) row.SetActive(false);
            }
        }
    }

    private void BuildDishGrid(string dishName, string[] names, Sprite[] sprites)
    {
        if (!dishCaughtRows.ContainsKey(dishName))
            dishCaughtRows[dishName] = new List<GameObject>();

        if (!dishCaughtCounts.ContainsKey(dishName))
            dishCaughtCounts[dishName] = new Dictionary<string, int>();

        if (!dishCaughtOrders.ContainsKey(dishName))
            dishCaughtOrders[dishName] = new List<string>();

        if (!dishCountTexts.ContainsKey(dishName))
            dishCountTexts[dishName] = new Dictionary<string, TextMesh>();

        var rows = dishCaughtRows[dishName];
        var counts = dishCaughtCounts[dishName];
        var texts = dishCountTexts[dishName];

        float iconSize = 0.12f;

        for (int i = 0; i < names.Length; i++)
        {
            int col = i / maxRows;
            int row = i % maxRows;

            float posX = gridStartX + (col * colSpacing);
            float posY = gridStartY - (row * rowSpacing);

            string rowName = "Row_" + dishName + "_" + names[i];
            GameObject rowObj = null;

            for (int j = 0; j < rows.Count; j++)
            {
                if (rows[j] != null && rows[j].name == rowName)
                {
                    rowObj = rows[j];
                    break;
                }
            }

            if (rowObj == null)
            {
                rowObj = new GameObject(rowName);
                rowObj.transform.position = new Vector3(posX, posY, 0f);

                GameObject iconObj = new GameObject("Icon_" + names[i]);
                iconObj.transform.SetParent(rowObj.transform);
                iconObj.transform.localPosition = Vector3.zero;
                SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
                if (sprites[i] != null)
                    sr.sprite = sprites[i];
                else
                {
                    if (sr.sprite == null)
                    {
                        Texture2D tex = new Texture2D(64, 64);
                        Color[] pixels = new Color[64 * 64];
                        for (int p = 0; p < pixels.Length; p++)
                            pixels[p] = Color.white;
                        tex.SetPixels(pixels);
                        tex.Apply();
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
                    }
                    sr.color = Color.yellow;
                }
                sr.sortingOrder = 20;
                iconObj.transform.localScale = Vector3.one * iconSize;

                GameObject textObj = new GameObject("Count_" + names[i]);
                textObj.transform.SetParent(rowObj.transform);
                textObj.transform.localPosition = new Vector3(0.2f, 0f, 0f);
                TextMesh countText = textObj.AddComponent<TextMesh>();
                countText.text = "0x";
                countText.characterSize = 0.035f;
                countText.fontSize = 100;
                countText.color = Color.black;
                countText.anchor = TextAnchor.MiddleLeft;
                countText.alignment = TextAlignment.Left;
                MeshRenderer tmr = textObj.GetComponent<MeshRenderer>();
                tmr.sortingOrder = 20;

                rows.Add(rowObj);
                texts[names[i]] = countText;
            }
            else
            {
                rowObj.transform.position = new Vector3(posX, posY, 0f);

                SpriteRenderer sr = rowObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sprites[i] != null)
                    sr.sprite = sprites[i];
            }

            int count = counts.ContainsKey(names[i]) ? counts[names[i]] : 0;
            if (texts.ContainsKey(names[i]) && texts[names[i]] != null)
                texts[names[i]].text = count + "x";

            rowObj.SetActive(count > 0);
        }

        RepositionRows();
    }

    private void UpdateCaughtText(string ingredientName)
    {
        if (!dishCaughtRows.ContainsKey(currentDishName)) return;
        if (!dishCaughtCounts.ContainsKey(currentDishName)) return;
        if (!dishCountTexts.ContainsKey(currentDishName)) return;

        var rows = dishCaughtRows[currentDishName];
        var counts = dishCaughtCounts[currentDishName];
        var texts = dishCountTexts[currentDishName];

        int count = counts.ContainsKey(ingredientName) ? counts[ingredientName] : 0;

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null && rows[i].name == "Row_" + currentDishName + "_" + ingredientName)
            {
                rows[i].SetActive(true);
                break;
            }
        }

        if (texts.ContainsKey(ingredientName) && texts[ingredientName] != null)
            texts[ingredientName].text = count + "x";
    }

    private void RepositionRows()
    {
        if (!dishCaughtRows.ContainsKey(currentDishName)) return;
        if (!dishCaughtOrders.ContainsKey(currentDishName)) return;

        var rows = dishCaughtRows[currentDishName];
        var order = dishCaughtOrders[currentDishName];

        for (int i = 0; i < order.Count; i++)
        {
            string name = order[i];

            int col = i / maxRows;
            int row = i % maxRows;

            float newX = gridStartX + (col * colSpacing);
            float newY = gridStartY - (row * rowSpacing);

            for (int j = 0; j < rows.Count; j++)
            {
                if (rows[j] != null && rows[j].name == "Row_" + currentDishName + "_" + name)
                {
                    rows[j].transform.position = new Vector3(newX, newY, 0f);
                    break;
                }
            }
        }
    }

    private void ClearIngredients()
    {
        foreach (var ingredient in activeIngredients)
        {
            if (ingredient != null) Destroy(ingredient.gameObject);
        }
        activeIngredients.Clear();
    }

    private void CleanNullIngredients()
    {
        activeIngredients.RemoveAll(item => item == null);
    }

    public int GetIngredientCount(string dishName, string ingredientName)
{
    if (dishCaughtCounts.ContainsKey(dishName) && 
        dishCaughtCounts[dishName].ContainsKey(ingredientName))
        return dishCaughtCounts[dishName][ingredientName];
    return 0;
}

    public Dictionary<string, int> GetDishCounts(string dishName)
    {
        if (dishCaughtCounts.ContainsKey(dishName))
            return dishCaughtCounts[dishName];
        return new Dictionary<string, int>();
    }
}

