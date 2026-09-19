using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreppingSync : MonoBehaviour
{
    public static PreppingSync Instance { get; private set; }

    private Dictionary<string, GameObject> syncedObjects = new Dictionary<string, GameObject>();
    private int nextId = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameMessageReceived += HandleGameMessage;
    }

    private void OnDisable()
    {
        GameManager.OnGameMessageReceived -= HandleGameMessage;
    }

    private void HandleGameMessage(string msg)
    {
        if (msg.StartsWith(RoomProtocol.PREP_SPAWN + "|"))
            HandleSpawn(msg);
        else if (msg.StartsWith(RoomProtocol.PREP_MOVE + "|"))
            HandleMove(msg);
        else if (msg.StartsWith(RoomProtocol.PREP_WASH_COMPLETE + "|"))
            HandleWash(msg);
        else if (msg.StartsWith(RoomProtocol.PREP_CHOP_COMPLETE + "|"))
            HandleChop(msg);
        else if (msg.StartsWith(RoomProtocol.PREP_DELETE + "|"))
            HandleDelete(msg);
    }

    public string GenerateId()
    {
        nextId++;
        return $"ING_{nextId}_{DateTime.Now.Ticks % 10000}";
    }

    public void SendSpawn(string id, string ingredientType, Vector2 position, int snapIndex)
    {
        string msg = $"{RoomProtocol.PREP_SPAWN}|{id}|{ingredientType}|{position.x:F2}|{position.y:F2}|{snapIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[PreppingSync] Sent spawn: {msg}");
    }

    public void SendMove(string id, Vector2 position, int snapIndex)
    {
        string msg = $"{RoomProtocol.PREP_MOVE}|{id}|{position.x:F2}|{position.y:F2}|{snapIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[PreppingSync] Sent move: {msg}");
    }

    public void SendWashComplete(string id, string originalTag, string newTag, int spriteIndex)
    {
        string msg = $"{RoomProtocol.PREP_WASH_COMPLETE}|{id}|{originalTag}|{newTag}|{spriteIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[PreppingSync] Sent wash: {msg}");
    }

    public void SendChopComplete(string id, string originalTag, string newTag, int spriteIndex)
    {
        string msg = $"{RoomProtocol.PREP_CHOP_COMPLETE}|{id}|{originalTag}|{newTag}|{spriteIndex}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[PreppingSync] Sent chop: {msg}");
    }

    public void SendDelete(string id)
    {
        string msg = $"{RoomProtocol.PREP_DELETE}|{id}";
        GameManager.Instance.SendGameMessage(msg);
        Debug.Log($"[PreppingSync] Sent delete: {msg}");
    }

    private void HandleSpawn(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 6) return;

        string id = parts[1];
        string ingredientType = parts[2];
        float posX = float.Parse(parts[3]);
        float posY = float.Parse(parts[4]);
        int snapIndex = int.Parse(parts[5]);

        Debug.Log($"[PreppingSync] Remote spawn: {ingredientType} at ({posX},{posY}) snap={snapIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            SpawnRemoteIngredient(id, ingredientType, new Vector2(posX, posY), snapIndex);
        });
    }

    private void HandleMove(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 5) return;

        string id = parts[1];
        float posX = float.Parse(parts[2]);
        float posY = float.Parse(parts[3]);
        int snapIndex = int.Parse(parts[4]);

        Debug.Log($"[PreppingSync] Remote move: {id} to ({posX},{posY}) snap={snapIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (syncedObjects.TryGetValue(id, out GameObject obj))
            {
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(posX, posY);
                }

                DragNDrop dragDrop = obj.GetComponent<DragNDrop>();
                if (dragDrop != null)
                {
                    dragDrop.snapPointIndex = snapIndex;
                }
            }
        });
    }

    private void HandleWash(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 5) return;

        string id = parts[1];
        string originalTag = parts[2];
        string newTag = parts[3];
        int spriteIndex = int.Parse(parts[4]);

        Debug.Log($"[PreppingSync] Remote wash: {id} {originalTag} -> {newTag} spriteIdx={spriteIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (syncedObjects.TryGetValue(id, out GameObject obj))
            {
                obj.tag = newTag;

                Sprite newSprite = IngredientLookup.Instance?.GetWashedSprite(originalTag, spriteIndex);
                if (newSprite != null)
                {
                    Image img = obj.GetComponent<Image>();
                    if (img != null)
                        img.sprite = newSprite;
                }
            }
        });
    }

    private void HandleChop(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 5) return;

        string id = parts[1];
        string originalTag = parts[2];
        string newTag = parts[3];
        int spriteIndex = int.Parse(parts[4]);

        Debug.Log($"[PreppingSync] Remote chop: {id} {originalTag} -> {newTag} spriteIdx={spriteIndex}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (syncedObjects.TryGetValue(id, out GameObject obj))
            {
                obj.tag = newTag;

                Sprite newSprite = IngredientLookup.Instance?.GetChoppedSprite(originalTag, spriteIndex);
                if (newSprite != null)
                {
                    Image img = obj.GetComponent<Image>();
                    if (img != null)
                        img.sprite = newSprite;
                }
            }
        });
    }

    private void HandleDelete(string msg)
    {
        string[] parts = msg.Split('|');
        if (parts.Length < 2) return;

        string id = parts[1];

        Debug.Log($"[PreppingSync] Remote delete: {id}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (syncedObjects.TryGetValue(id, out GameObject obj))
            {
                Destroy(obj);
                syncedObjects.Remove(id);
            }
        });
    }

    public void RegisterSyncedObject(string id, GameObject obj)
    {
        syncedObjects[id] = obj;
        Debug.Log($"[PreppingSync] Registered object: {id}");
    }

    public void UnregisterSyncedObject(string id)
    {
        syncedObjects.Remove(id);
        Debug.Log($"[PreppingSync] Unregistered object: {id}");
    }

    public GameObject GetSyncedObject(string id)
    {
        syncedObjects.TryGetValue(id, out GameObject obj);
        return obj;
    }

    private void SpawnRemoteIngredient(string id, string ingredientType, Vector2 position, int snapIndex)
    {
        Sprite sprite = IngredientLookup.Instance?.GetSprite(ingredientType);
        if (sprite == null)
        {
            Debug.LogWarning($"[PreppingSync] No sprite found for: {ingredientType}");
            return;
        }

        Transform spawnParent = GameObject.Find("SpawnedIngredients")?.transform;
        if (spawnParent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                spawnParent = canvas.transform;
        }

        if (spawnParent == null)
        {
            Debug.LogError("[PreppingSync] No spawn parent found!");
            return;
        }

        GameObject imgObj = new GameObject($"Remote_{ingredientType}_{id}");
        imgObj.transform.SetParent(spawnParent, false);

        Image img = imgObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        RectTransform rect = imgObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(80f, 80f);

        CanvasGroup cg = imgObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;

        DragNDrop dragDrop = imgObj.AddComponent<DragNDrop>();
        dragDrop.spawnPositionCopy = position;
        dragDrop.snapPointIndex = snapIndex;
        dragDrop.syncId = id;

        RegisterSyncedObject(id, imgObj);
    }
}
