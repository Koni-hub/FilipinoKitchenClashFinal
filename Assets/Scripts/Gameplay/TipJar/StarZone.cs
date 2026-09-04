using UnityEngine;

public class StarZone : MonoBehaviour
{
    [SerializeField] private int ratingValue;
    private RatingManager ratingManager;

    void Start()
    {
        ratingManager = GetComponentInParent<RatingManager>();
    }

    void OnMouseDown()
    {
        if (ratingManager != null)
        {
            ratingManager.SetRating(ratingValue);
        }
    }
}
