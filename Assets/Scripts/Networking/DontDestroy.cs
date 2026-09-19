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
}
