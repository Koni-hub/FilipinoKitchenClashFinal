using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    public string dishName;

    public void PlayGame()
    {
        switch (dishName)
        {
            case "Adobo":
                SceneManager.LoadScene("UI_AdoboPreppingArea");
                break;
            case "Sinigang":
                SceneManager.LoadScene("UI_SinigangPreppingArea");
                break;
            case "Sisig":
                SceneManager.LoadScene("UI_SisigPreppingArea");
                break;
        }
    }

    public void GoToCookingArea()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        if (sceneName == "UI_AdoboPreppingArea" || sceneName == "UI_SinigangPreppingArea" || sceneName == "UI_SisigPreppingArea")
            SceneManager.LoadScene("UI_Cooking_Area");
    }
    
}
