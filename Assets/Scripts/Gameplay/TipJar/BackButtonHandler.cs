using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    [SerializeField] private string cookingAreaSceneName = "UI_Cooking_Area";

    void OnMouseDown()
    {
        SceneManager.LoadScene(cookingAreaSceneName);
    }
}
