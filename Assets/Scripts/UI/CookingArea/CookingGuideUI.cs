using System;
using System.Collections.Generic;
using UnityEngine;

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
        // Clear any previous slots
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear();

        // Instantiate slots dynamically
        foreach (var ingredient in recipe.requiredIngredients)
        {
            GameObject slotGO = Instantiate(slotPrefab, contentContainer);
            slotGO.SetActive(true);

            IngredientSlotUI slotUI = slotGO.GetComponent<IngredientSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(ingredient.icon, ingredient.isFulfilled);
                activeSlots.Add(slotUI);
            }
        }
    }

    public void MarkIngredientComplete(int index, bool isComplete = true)
    {
        if (index >= 0 && index < activeSlots.Count)
        {
            activeSlots[index].SetCompleted(isComplete);
        }
    }
}