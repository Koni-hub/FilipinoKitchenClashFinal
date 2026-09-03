using UnityEngine;
using UnityEngine.SceneManagement;

public class TipJarInteraction : MonoBehaviour
{
    [SerializeField] private string ratingSceneName = "UI_Rating";

    void OnMouseDown()
    {
        SceneManager.LoadScene(ratingSceneName);
    }
}
