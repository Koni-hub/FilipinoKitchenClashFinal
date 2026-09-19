using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneSetup : MonoBehaviour
{
    private static GameSceneSetup instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        GameObject setupObj = new GameObject("GameSceneSetup");
        setupObj.AddComponent<GameSceneSetup>();
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
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == "UI_PreppingArea" || sceneName == "UI_Cooking_Area" || sceneName == "UI_Mini_Market")
        {
            CreateGameManagers(sceneName);
        }
    }

    private void CreateGameManagers(string sceneName)
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            Debug.Log("[GameSceneSetup] Created GameManager");
        }

        if (IngredientLookup.Instance == null)
        {
            GameObject lookupObj = new GameObject("IngredientLookup");
            lookupObj.AddComponent<IngredientLookup>();
            Debug.Log("[GameSceneSetup] Created IngredientLookup");
        }

        if (sceneName == "UI_PreppingArea")
        {
            if (PreppingSync.Instance == null)
            {
                GameObject preppingSyncObj = new GameObject("PreppingSync");
                preppingSyncObj.AddComponent<PreppingSync>();
                Debug.Log("[GameSceneSetup] Created PreppingSync");
            }

            GameObject navObj = new GameObject("PreppingAreaButtonSpawner");
            navObj.AddComponent<PreppingAreaButtonSpawner>();
            Debug.Log("[GameSceneSetup] Created PreppingAreaButtonSpawner");
        }

        if (sceneName == "UI_Cooking_Area")
        {
            if (CookingSync.Instance == null)
            {
                GameObject cookingSyncObj = new GameObject("CookingSync");
                cookingSyncObj.AddComponent<CookingSync>();
                Debug.Log("[GameSceneSetup] Created CookingSync");
            }

            if (ScoreManager.Instance == null)
            {
                GameObject scoreObj = new GameObject("ScoreManager");
                scoreObj.AddComponent<ScoreManager>();
                Debug.Log("[GameSceneSetup] Created ScoreManager");
            }

            GameObject navObj = new GameObject("CookingAreaButtonSpawner");
            navObj.AddComponent<CookingAreaButtonSpawner>();
            Debug.Log("[GameSceneSetup] Created CookingAreaButtonSpawner");
        }

        if (sceneName == "UI_Mini_Market")
        {
            GameObject navObj = new GameObject("MiniMarketButtonSpawner");
            navObj.AddComponent<MiniMarketButtonSpawner>();
            Debug.Log("[GameSceneSetup] Created MiniMarketButtonSpawner");
        }
    }
}
