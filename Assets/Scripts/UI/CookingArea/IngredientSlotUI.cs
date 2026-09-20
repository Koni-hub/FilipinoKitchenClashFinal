using UnityEngine;
using UnityEngine.UI;

public class IngredientSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkmarkObj;

    public void Setup(Sprite icon, bool isCompleted = false)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        SetCompleted(isCompleted);
    }

    public void SetCompleted(bool isCompleted)
    {
        if (checkmarkObj != null)
        {
            checkmarkObj.SetActive(isCompleted);
        }
    }
}