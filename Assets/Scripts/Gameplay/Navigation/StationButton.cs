using UnityEngine;
using UnityEngine.SceneManagement;

public class StationButton : MonoBehaviour
{
    public string targetScene;

    private void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(targetScene))
            SceneManager.LoadScene(targetScene);
    }
}
