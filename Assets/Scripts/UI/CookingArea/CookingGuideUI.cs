using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingGuideUI : MonoBehaviour
{
    [Serializable]
    public class IngredientRequirement
    {
        public string ingredientName;
        public Sprite icon;
        public bool isFulfilled;
    }

    [Serializable]
    public class RecipeGuide
    {
        public string dishName;
        public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();
    }

    [Header("UI References")]
    [SerializeField] private Transform contentContainer; // SpeechBubble_Content
    [SerializeField] private GameObject slotPrefab;       // Slot_Template prefab

    [Header("Recipes")]
    [SerializeField] private List<RecipeGuide> recipes = new List<RecipeGuide>();

    private readonly List<IngredientSlotUI> activeSlots = new List<IngredientSlotUI>();

    private void Start()
    {
        if (recipes.Count > 0)
        {
            DisplayRecipe(0);
        }
    }

    public void DisplayRecipe(int index)
    {
        if (index < 0 || index >= recipes.Count) return;
        LoadRecipe(recipes[index]);
    }

    public void DisplayRecipe(string dishName)
    {
        RecipeGuide recipe = recipes.Find(r => r.dishName.Equals(dishName, StringComparison.OrdinalIgnoreCase));
        if (recipe != null)
        {
            LoadRecipe(recipe);
        }
        else
        {
            Debug.LogWarning($"[CookingGuideUI] Recipe '{dishName}' not found.");
        }
    }

    private void LoadRecipe(RecipeGuide recipe)
    {
        if (contentContainer == null)
        {
            Debug.LogError("[CookingGuideUI] contentContainer is null!");
            return;
        }

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear();

        foreach (var ingredient in recipe.requiredIngredients)
        {
            GameObject slotGO;
            if (slotPrefab != null)
            {
                slotGO = Instantiate(slotPrefab, contentContainer);
            }
            else
            {
                slotGO = CreateSlotRuntime(ingredient);
            }
            slotGO.SetActive(true);

            IngredientSlotUI slotUI = slotGO.GetComponent<IngredientSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(ingredient.icon, ingredient.isFulfilled);
                activeSlots.Add(slotUI);
            }
        }
    }

    private GameObject CreateSlotRuntime(IngredientRequirement ingredient)
    {
        GameObject slotGO = new GameObject($"Slot_{ingredient.ingredientName}", typeof(RectTransform));
        slotGO.transform.SetParent(contentContainer, false);

        RectTransform rt = slotGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        Image bg = slotGO.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.3f);

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        iconObj.transform.SetParent(slotGO.transform, false);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = ingredient.icon;
        iconImg.preserveAspect = true;
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.sizeDelta = Vector2.zero;

        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform));
        checkObj.transform.SetParent(slotGO.transform, false);
        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = Color.green;
        RectTransform checkRt = checkObj.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.7f, 0.7f);
        checkRt.anchorMax = Vector2.one;
        checkRt.sizeDelta = Vector2.zero;
        checkObj.SetActive(false);

        IngredientSlotUI slotUI = slotGO.AddComponent<IngredientSlotUI>();
        SetPrivateField(slotUI, "iconImage", iconImg);
        SetPrivateField(slotUI, "checkmarkObj", checkObj);

        return slotGO;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(target, value);
    }

    public void MarkIngredientComplete(int index, bool isComplete = true)
    {
        if (index >= 0 && index < activeSlots.Count)
        {
            activeSlots[index].SetCompleted(isComplete);
        }
    }
}