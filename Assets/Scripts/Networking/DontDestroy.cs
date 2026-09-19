using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    public void PlayGame()
    {
        string sceneToLoad = "UI_PreppingArea";
        SceneManager.LoadScene(sceneToLoad);
    }
}
