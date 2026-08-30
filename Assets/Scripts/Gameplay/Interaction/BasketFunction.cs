using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BasketFunction : MonoBehaviour
{
    // BASKET SPAWN POSITIONS AND PLACEMENT LOGIC
    private bool[] place = new bool[3];
    private Vector2[] spawnPositions = new Vector2[]
    {
        new Vector2(-401.1f, -38.8f),
        new Vector2(-364.3f, -38.8f),
        new Vector2(-327.1f, -38.8f)
    };

    private bool[] imageExists = new bool[3]; // THIS LINE OF CODE HANDLES AN ARRAY OF BOOLEAN VALUES TO TRACK WHETHER AN IMAGE EXISTS IN EACH SPAWN POSITION IN THE BASKET. THIS CAN BE USED TO PREVENT MULTIPLE IMAGES FROM BEING SPAWNED IN THE SAME POSITION AND TO CONTROL THE LOGIC FOR REMOVING IMAGES FROM THE BASKET WHEN INGREDIENTS ARE USED.
    private GameObject[] spawnedImages = new GameObject[3]; // THIS LINE OF CODE HANDLES AN ARRAY OF GAMEOBJECT REFERENCES TO TRACK THE SPAWNED IMAGES IN THE BASKET. THIS CAN BE USED TO DESTROY THE CORRESPONDING IMAGE WHEN AN INGREDIENT IS USED AND TO PREVENT MULTIPLE IMAGES FROM BEING SPAWNED FOR THE SAME ITEM.
    void Awake()
    {
        for (int i = 0; i < place.Length; i++)
        {
            place[i] = false;
        }
    }

    // THIS FUNCTION CHECKS FOR AVAILABLE SPAWN POSITIONS IN THE BASKET AND RETURNS THE FIRST AVAILABLE POSITION. IF THERE ARE NO AVAILABLE POSITIONS, IT RETURNS Vector3.zero.
    public Vector2 spawnPosition()
    {
        for (int i = 0; i < place.Length; i++)
        {
            if (place[i] == false)
            {
                place[i] = true;
                return spawnPositions[i];
            }
        }
        return Vector3.zero;
    }
    
    // THIS FUNCTION REMOVES AN INGREDIENT FROM THE BASKET BASED ON THE PROVIDED POSITION. IT CHECKS THE spawnPositions ARRAY FOR A MATCHING POSITION AND SETS THE CORRESPONDING place VALUE TO false, INDICATING THAT THE POSITION IS NOW AVAILABLE FOR NEW INGREDIENTS TO BE SPAWNED.
    public void RemoveIngredient(Vector2 position)
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            if (spawnPositions[i] == position)
            {
                place[i] = false;
                return;
            }
        }
    }

}
