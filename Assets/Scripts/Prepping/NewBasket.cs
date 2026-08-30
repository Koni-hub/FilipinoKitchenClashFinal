using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBasket : MonoBehaviour
{
    // BASKET PROPERTIES
    public Image[] icons = new Image[3];
    public bool[] isOccupied;

    void Start()
    {
        isOccupied = new bool[3];
    }

    public Image iconPosition()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            if (!isOccupied[i])
            {
                return icons[i];
            }
        }
        return null;
    }
}
