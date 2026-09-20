using UnityEngine;
using UnityEngine.UI;

public class GetBowlFromPrepping : MonoBehaviour
{
    public string[] bowlIngredients = new string[4];
    public bool[] place = new bool[4];
    Vector2[] position = new Vector2[4];
    public GameObject[] bowlIngredientImages;

    void Awake()
    {
        position[0] = new Vector2(-7.77f, -1.05f);
        position[1] = new Vector2(-6.98f, -1.1f);
        position[2] = new Vector2(-7.77f, -1.62f);
        position[3] = new Vector2(-6.98f, -1.63f);
        GetBowlIngredients();
    }

    public void GetBowlIngredients()
    {
        if (SendBowlIngredients.Instance != null)
        {
            for (int i = 0; i < bowlIngredients.Length; i++)
            {
                bowlIngredients[i] = SendBowlIngredients.Instance.bowlIngredients[i];
            }
            SpawnBowlIngredients();
        }
        else
        {
            Debug.LogWarning("SendBowlIngredients Instance not found.");
        }
    }

    
    public void SpawnBowlIngredients()
    {
        for (int i = 0; i < bowlIngredients.Length; i++)
        {
            GameObject newSprite = null; // declare before switch
            switch(bowlIngredients[i])
            {
                case "SilverBowlLaurelLeaves":
                    newSprite = Instantiate(bowlIngredientImages[0], SpawnPosition(), Quaternion.identity);
                    Debug.Log("Successfully spawned SilverBowlLaurelLeaves");
                    break;
                case "SilverBowlPorkBelly":
                    newSprite = Instantiate(bowlIngredientImages[1], SpawnPosition(), Quaternion.identity);
                    Debug.Log("Successfully spawned SilverBowlPorkBelly");
                    break;
                case "BlueBowlOnionMinced":
                    newSprite = Instantiate(bowlIngredientImages[2], SpawnPosition(), Quaternion.identity);
                    Debug.Log("Successfully spawned BlueBowlOnionMinced");
                    break;
                case "WhiteBowlGarlic":
                    newSprite = Instantiate(bowlIngredientImages[3], SpawnPosition(), Quaternion.identity);
                    Debug.Log("Successfully spawned WhiteBowlGarlic");
                    break;    
            }
        }
    }

    public Vector2 SpawnPosition()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!place[i])
            {
                place[i] = true; // mark as occupied
                return position[i];
            }
        }
        return Vector2.zero;
    }
}