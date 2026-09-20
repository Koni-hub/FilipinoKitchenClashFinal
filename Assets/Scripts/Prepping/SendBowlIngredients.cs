using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendBowlIngredients : MonoBehaviour
{
    public static SendBowlIngredients Instance;
    public string[] bowlIngredients = new string[4];
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void AddBowlIngredient(string bowlIngredient)
    {
        for (int i = 0, n = bowlIngredients.Length; i < n; i++)
        {
             if (string.IsNullOrEmpty(bowlIngredients[i]))
            {
                bowlIngredients[i] = bowlIngredient;
                break;
            }
        }
    }

}
