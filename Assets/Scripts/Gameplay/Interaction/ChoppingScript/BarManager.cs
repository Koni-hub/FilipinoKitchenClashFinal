using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarManager : MonoBehaviour
{
    public Image loadingBar;

    public void UpdateLoadingBar()
    {
        StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        loadingBar.fillAmount = 0f;
        
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.10f);
            loadingBar.fillAmount += 0.20f;
        }
    }
}
