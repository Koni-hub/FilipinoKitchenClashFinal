using UnityEngine;

public class RatingManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ratingBarRenderer;
    [SerializeField] private Sprite rate0Sprite;
    [SerializeField] private Sprite rate2Sprite;
    [SerializeField] private Sprite rate3Sprite;
    [SerializeField] private Sprite rate4Sprite;

    private int currentRating = 0;

    void Start()
    {
        if (ratingBarRenderer != null && rate0Sprite != null)
        {
            ratingBarRenderer.sprite = rate0Sprite;
        }
    }

    public void SetRating(int value)
    {
        currentRating = value;

        if (ratingBarRenderer == null) return;

        switch (value)
        {
            case 2:
                ratingBarRenderer.sprite = rate2Sprite;
                break;
            case 3:
                ratingBarRenderer.sprite = rate3Sprite;
                break;
            case 4:
                ratingBarRenderer.sprite = rate4Sprite;
                break;
            default:
                ratingBarRenderer.sprite = rate0Sprite;
                break;
        }

        PlayerPrefs.SetInt("GameRating", value);
        PlayerPrefs.Save();
    }
}
