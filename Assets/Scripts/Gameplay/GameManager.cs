using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<string> OnGameMessageReceived;

    private NetworkManager networkManager;
    private bool isHost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        networkManager = NetworkManager.Instance;
        isHost = (RoomData.PlayerRole == "host");

        if (networkManager == null)
        {
            Debug.LogError("[GameManager] NetworkManager not found!");
            return;
        }

        if (isHost)
        {
            if (networkManager.Host != null)
            {
                networkManager.Host.OnMessageReceived += HandleHostMessage;
                Debug.Log("[GameManager] Subscribed to Host messages");
            }
        }
        else
        {
            if (networkManager.Client != null)
            {
                networkManager.Client.OnMessageReceived += HandleClientMessage;
                networkManager.Client.OnHostDisconnected += HandleHostDisconnected;
                Debug.Log("[GameManager] Subscribed to Client messages");
            }
        }

        Debug.Log($"[GameManager] Initialized as {(isHost ? "HOST" : "CLIENT")}");
    }

    private void HandleHostMessage(string msg)
    {
        if (!RoomProtocol.IsGameMessage(msg)) return;

        Debug.Log($"[GameManager] Host received game message: {msg}");
        OnGameMessageReceived?.Invoke(msg);
    }

    private void HandleClientMessage(string msg)
    {
        if (!RoomProtocol.IsGameMessage(msg)) return;

        Debug.Log($"[GameManager] Client received game message: {msg}");
        OnGameMessageReceived?.Invoke(msg);
    }

    private void HandleHostDisconnected()
    {
        Debug.Log("[GameManager] Host disconnected! Returning to main menu.");
        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("UI_MainMenu");
        });
    }

    public void SendGameMessage(string message)
    {
        if (networkManager == null)
        {
            Debug.LogWarning("[GameManager] NetworkManager is null, cannot send.");
            return;
        }

        if (isHost)
        {
            if (networkManager.Host != null && networkManager.Host.HasClient)
            {
                networkManager.Host.SendToClient(message);
            }
            else
            {
                Debug.LogWarning("[GameManager] Host has no client connected.");
            }
        }
        else
        {
            if (networkManager.Client != null && networkManager.Client.IsConnected)
            {
                networkManager.Client.SendToHost(message);
            }
            else
            {
                Debug.LogWarning("[GameManager] Client is not connected.");
            }
        }
    }

    public bool IsHost => isHost;
    public bool IsConnected
    {
        get
        {
            if (isHost)
                return networkManager?.Host != null && networkManager.Host.HasClient;
            else
                return networkManager?.Client != null && networkManager.Client.IsConnected;
        }
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            if (isHost && networkManager.Host != null)
                networkManager.Host.OnMessageReceived -= HandleHostMessage;

            if (!isHost && networkManager.Client != null)
            {
                networkManager.Client.OnMessageReceived -= HandleClientMessage;
                networkManager.Client.OnHostDisconnected -= HandleHostDisconnected;
            }
        }
    }
}
