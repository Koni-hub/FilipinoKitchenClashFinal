using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BowlManager : MonoBehaviour
{
    // REFERENCE TO THE BOWL FUNCTION SCRIPTS TO ACCESS THEIR VARIABLES AND FUNCTIONS
    [SerializeField] private BowlFunction[] bowlFunction;

    // THIS BOOLEAN ARRAY TRACKS WHICH SPAWN POSITIONS IN THE BASKET ARE OCCUPIED BY BOWLS, INITIALIZED TO FALSE TO INDICATE THAT ALL POSITIONS ARE AVAILABLE AT THE START
    public bool[] place = new bool[4];
    // THIS VECTOR2 ARRAY STORES THE PREDEFINED SPAWN POSITIONS IN THE BASKET WHERE THE BOWLS CAN BE PLACED, CORRESPONDING TO THE INDICES OF THE PLACE ARRAY
    public BowlSnapPoints[] snapPoints;

    void Awake()
    {
        snapPoints = FindObjectsOfType<BowlSnapPoints>();
    }

    // THIS FUNCTION RETURNS THE NEXT AVAILABLE SPAWN POSITION IN THE BASKET WHERE A BOWL CAN BE PLACED, UPDATING THE PLACE ARRAY TO INDICATE THAT THE POSITION IS NOW OCCUPIED
    public Vector2 spawnPosition()
    {
        for (int i = 0; i < place.Length; i++)
        {
            if (place[i] == false)
            {
                place[i] = true;
                return snapPoints[i].GetComponent<RectTransform>().anchoredPosition;
            }
        }
        return Vector3.zero;
    }

    // THIS FUNCTION IS RESPONSIBLE FOR REMOVING AN INGREDIENT FROM THE BASKET AT A SPECIFIC POSITION, UPDATING THE PLACE ARRAY TO INDICATE THAT THE POSITION IS NOW AVAILABLE
    public void RemovePlace(Vector2 position)
    {
        for (int i = 0; i < place.Length; i++)
        {
            if (snapPoints[i].GetComponent<RectTransform>().anchoredPosition == position)
            {
                snapPoints[i].GetComponent<BowlSnapPoints>().isOccupied = false;
                place[i] = false;
                return;
            }
        }
    }

    // THIS FUNCTION DESELECTS ALL BOWL SLOTS IN THE BASKET BY DEACTIVATING THEIR SELECTED SHADER AND SETTING THEIR ISSELECTED BOOLEAN TO FALSE, PROVIDING A WAY TO CLEAR ANY SELECTIONS WHEN NECESSARY (E.G., WHEN A NEW SLOT IS SELECTED OR WHEN AN ACTION IS PERFORMED THAT REQUIRES DESELECTION)
    public void DeselectAllSlots()
    {
        foreach (BowlFunction bowl in bowlFunction)
        {
            bowl.selectedShader.SetActive(false);
        }
    }

}
