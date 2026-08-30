using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [System.Serializable]
    public class IngredientData
    {
        public string ingredientName;
        public Sprite sprite;
    }

    public IngredientData[] ingredients;
    public GameObject ingredientPrefab;

    private float spawnTimer;
    private float spawnInterval = 1.5f;
    private float fallSpeed = 3f;
    private float minSpawnX = 2f;
    private float maxSpawnX = 6f;
    private float spawnY = 3f;
    private bool canSpawn = false;

    void Update()
    {
        if (!canSpawn) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnIngredient();
        }
    }

    public void SpawnIngredient()
    {
        if (ingredients == null || ingredients.Length == 0)
        {
            Debug.Log("No ingredients to spawn!");
            return;
        }

        IngredientData data = ingredients[Random.Range(0, ingredients.Length)];

        GameObject obj = Instantiate(ingredientPrefab, transform);
        obj.SetActive(true);

        float randomX = Random.Range(minSpawnX, maxSpawnX);
        obj.transform.position = new Vector3(randomX, spawnY, 0f);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (data.sprite != null)
            {
                sr.sprite = data.sprite;
            }
            else
            {
                if (sr.sprite == null)
                    sr.sprite = CreateWhiteSquare();
                sr.color = Color.yellow;
            }

            sr.transform.localScale = Vector3.one * 0.3f;
            sr.sortingOrder = 10;
        }

        FallingIngredient fi = obj.GetComponent<FallingIngredient>();
        fi.Init(fallSpeed);
        fi.ingredientName = data.ingredientName;
        Debug.Log("Spawned: " + data.ingredientName + " at " + obj.transform.position);
    }

    public void SetIngredients(string[] names, Sprite[] sprites)
    {
        ingredients = new IngredientData[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            ingredients[i] = new IngredientData { ingredientName = names[i], sprite = sprites[i] };
        }
    }

    public void SetDifficulty(int level)
    {
        spawnInterval = Mathf.Max(0.5f, 1.5f - (level * 0.1f));
        fallSpeed = 3f + (level * 0.5f);
    }

    public void StopSpawning()
    {
        canSpawn = false;
    }

    private Sprite CreateWhiteSquare()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    public void StartSpawning()
    {
        canSpawn = true;
        spawnTimer = spawnInterval;
        Debug.Log("Spawner started! Ingredients: " + (ingredients != null ? ingredients.Length : 0));
    }
}
