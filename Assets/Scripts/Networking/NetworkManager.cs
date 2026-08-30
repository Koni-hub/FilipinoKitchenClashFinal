using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public ClientService Client { get; private set; }
    public HostService Host { get; private set; }

    public const int HOST_PORT = 8888;
    public const int CLIENT_PORT = 7777;

    private string actualHostIP = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // CRITICAL: Initialize the dispatcher here, on the main thread, before
        // any background UDP threads are created.
        UnityMainThreadDispatcher.EnsureInitialized();

        InitializeClient();
    }

    private void Update()
    {
        Host?.Tick(Time.time);
        Client?.Tick(Time.time);
    }

    // Changed from private to public
    public void InitializeClient()
    {
        if (Client != null)
        {
            Client.OnConnected -= HandleClientConnected;
            Client.OnConnectionFailed -= HandleConnectionFailed;
            Client.OnMessageReceived -= HandleClientMessage;
            Client.Dispose();
        }

        Client = new ClientService(CLIENT_PORT, HOST_PORT);
        Client.OnConnected += HandleClientConnected;
        Client.OnConnectionFailed += HandleConnectionFailed;
        Client.OnMessageReceived += HandleClientMessage;
        Debug.Log("[NetworkManager] Client initialized.");
    }

    // Optional: Method to check if client is ready
    public bool IsClientReady()
    {
        return Client != null;
    }

    // Optional: Method to reset client state
    public void ResetClient()
    {
        if (Client != null)
        {
            Client.Reset();
            Debug.Log("[NetworkManager] Client state reset.");
        }
    }

    public void StartHost(string roomCode)
    {
        StopHost();

        Host = new HostService(HOST_PORT, roomCode);
        Host.OnClientJoined += HandleClientJoined;
        Host.OnClientLeft += HandleClientLeft;
        Host.OnRoomFull += HandleRoomFull;

        actualHostIP = GetLocalIPAddress();
        Debug.Log($"[NetworkManager] Host started on IP: {actualHostIP}, Port: {HOST_PORT}");
    }

    public void JoinHost(string roomCode, IPEndPoint hostEndpoint = null)
    {
        if (Client == null)
        {
            Debug.LogError("[NetworkManager] Client is null, reinitializing...");
            InitializeClient();
        }

        Debug.Log($"[NetworkManager] Joining host with room code: {roomCode}");

        if (hostEndpoint == null)
            Client.DiscoverHost(roomCode);
        else
            Client.JoinRoom(roomCode, hostEndpoint);
    }

    public void StopHost()
    {
        if (Host != null)
        {
            Host.OnClientJoined -= HandleClientJoined;
            Host.OnClientLeft -= HandleClientLeft;
            Host.OnRoomFull -= HandleRoomFull;
            Host.Dispose();
            Host = null;
            Debug.Log("[NetworkManager] Host stopped.");
        }
    }

    public void StopClient()
    {
        if (Client != null)
        {
            Client.OnConnected -= HandleClientConnected;
            Client.OnConnectionFailed -= HandleConnectionFailed;
            Client.OnMessageReceived -= HandleClientMessage;
            Client.Dispose();
            Client = null;
            Debug.Log("[NetworkManager] Client stopped.");
        }

        // Don't automatically reinitialize - let the caller decide when to initialize
        // InitializeClient();
    }

    public string GetLocalIPAddress()
    {
        string localIP = "";

        try
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;

                string address = ip.ToString();
                if (address.StartsWith("127.")) continue;

                if (address.StartsWith("192.168.") ||
                    address.StartsWith("10.") ||
                    address.StartsWith("172."))
                {
                    localIP = address;
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NetworkManager] GetLocalIPAddress failed: {e.Message}");
        }

        if (string.IsNullOrEmpty(localIP))
            localIP = "127.0.0.1";

        return localIP;
    }

    public string GetHostIPForClients() => actualHostIP ?? GetLocalIPAddress();

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleClientJoined(IPEndPoint clientEndpoint)
    {
        Debug.Log($"[NetworkManager] Client joined from: {clientEndpoint}");
        // You can add more logic here if needed
    }

    private void HandleClientLeft(IPEndPoint clientEndpoint)
    {
        Debug.Log($"[NetworkManager] Client left: {clientEndpoint}");
        // You can add more logic here if needed
    }

    private void HandleRoomFull()
    {
        Debug.Log("[NetworkManager] Room is full!");
        // You can add more logic here if needed
    }

    private void HandleClientConnected()
    {
        Debug.Log("[NetworkManager] Successfully connected to host.");
    }

    private void HandleConnectionFailed(string reason)
    {
        Debug.LogError($"[NetworkManager] Connection failed: {reason}");
    }

    private void HandleClientMessage(string msg)
    {
        Debug.Log($"[NetworkManager] Client message: {msg}");
        // You can add more logic here if needed
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        StopHost();
        StopClient();
    }
}