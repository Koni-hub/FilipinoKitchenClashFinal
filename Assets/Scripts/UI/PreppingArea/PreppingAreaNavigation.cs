using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreppingAreaNavigation : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button goToMiniMarketButton;
    [SerializeField] private Button backButton;

    [Header("Scene Names")]
    [SerializeField] private string miniMarketScene = "UI_Mini_Market";
    [SerializeField] private string previousScene = "UI_PreppingArea";

    private void Start()
    {
        if (goToMiniMarketButton != null)
            goToMiniMarketButton.onClick.AddListener(OnGoToMiniMarket);

        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    private void OnGoToMiniMarket()
    {
        Debug.Log("[PreppingNav] Navigating to Mini Market");
        SceneManager.LoadScene(miniMarketScene);
    }

    private void OnBack()
    {
        Debug.Log("[PreppingNav] Going back to Prepping Area");
        SceneManager.LoadScene(previousScene);
    }

    private void OnDestroy()
    {
        if (goToMiniMarketButton != null)
            goToMiniMarketButton.onClick.RemoveAllListeners();

        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}
