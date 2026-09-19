using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientLookup : MonoBehaviour
{
    public static IngredientLookup Instance { get; private set; }

    private Dictionary<string, Sprite> nameToSprite = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite[]> tagToChoppedSprites = new Dictionary<string, Sprite[]>();
    private Dictionary<string, Sprite[]> tagToWashedSprites = new Dictionary<string, Sprite[]>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(string name, Sprite sprite)
    {
        if (!string.IsNullOrEmpty(name) && sprite != null && !nameToSprite.ContainsKey(name))
        {
            nameToSprite[name] = sprite;
        }
    }

    public Sprite GetSprite(string name)
    {
        nameToSprite.TryGetValue(name, out Sprite sprite);
        return sprite;
    }

    public string FindNameBySprite(Sprite sprite)
    {
        if (sprite == null) return null;
        foreach (var kvp in nameToSprite)
        {
            if (kvp.Value == sprite)
                return kvp.Key;
        }
        return null;
    }

    public bool HasIngredient(string name)
    {
        return nameToSprite.ContainsKey(name);
    }

    public void RegisterChopSprites(string tag, Sprite[] sprites)
    {
        if (!string.IsNullOrEmpty(tag) && sprites != null && !tagToChoppedSprites.ContainsKey(tag))
        {
            tagToChoppedSprites[tag] = sprites;
        }
    }

    public void RegisterWashSprites(string tag, Sprite[] sprites)
    {
        if (!string.IsNullOrEmpty(tag) && sprites != null && !tagToWashedSprites.ContainsKey(tag))
        {
            tagToWashedSprites[tag] = sprites;
        }
    }

    public Sprite GetChoppedSprite(string originalTag, int spriteIndex)
    {
        if (tagToChoppedSprites.TryGetValue(originalTag, out Sprite[] sprites))
        {
            if (spriteIndex >= 0 && spriteIndex < sprites.Length)
                return sprites[spriteIndex];
        }
        return null;
    }

    public Sprite GetWashedSprite(string originalTag, int spriteIndex)
    {
        if (tagToWashedSprites.TryGetValue(originalTag, out Sprite[] sprites))
        {
            if (spriteIndex >= 0 && spriteIndex < sprites.Length)
                return sprites[spriteIndex];
        }
        return null;
    }
}
