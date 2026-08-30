using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net;

public class JoinRoomController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_InputField hostIpInput;
    [SerializeField] private Button backButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI ipDisplayText;

    private NetworkManager networkManager;
    private bool isConnecting = false;
    private Coroutine timeoutCoroutine = null;

    private void Start()
    {
        networkManager = NetworkManager.Instance;

        if (networkManager == null)
        {
            SetStatus("NetworkManager not found!");
            Debug.LogError("[JoinRoom] NetworkManager instance not found!");
            return;
        }

        // Ensure Client service exists
        if (networkManager.Client == null)
        {
            Debug.Log("[JoinRoom] Client service is null, initializing...");
            networkManager.InitializeClient();
        }

        if (ipDisplayText != null)
            ipDisplayText.text = $"Your IP: {networkManager.GetLocalIPAddress()}";

        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        // Subscribe to client events
        if (networkManager.Client != null)
        {
            networkManager.Client.OnConnected += OnConnected;
            networkManager.Client.OnConnectionFailed += OnConnectionFailed;
            networkManager.Client.OnMessageReceived += OnClientMessage;
        }
        else
        {
            Debug.LogError("[JoinRoom] networkManager.Client is still null after initialization!");
        }
    }

    private void OnJoinClicked()
    {
        if (isConnecting)
        {
            Debug.Log("[JoinRoom] Already connecting, ignoring click.");
            return;
        }

        string roomCode = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpper() : "";
        string hostIP = hostIpInput != null ? hostIpInput.text.Trim() : "";

        if (string.IsNullOrEmpty(roomCode))
        {
            SetStatus("Please enter a room code!");
            return;
        }

        // Validate room code format (4-6 characters alphanumeric)
        if (roomCode.Length < 4 || roomCode.Length > 6)
        {
            SetStatus("Room code should be 4-6 characters!");
            return;
        }

        // KEY FIX: Always reset client state before a new attempt
        // This clears leftover flags from any previous failed attempt
        if (networkManager.Client != null)
        {
            Debug.Log("[JoinRoom] Resetting client state before new connection attempt.");
            networkManager.Client.Reset();
        }
        else
        {
            SetStatus("Network client not available!");
            Debug.LogError("[JoinRoom] Cannot join - Client is null!");
            return;
        }

        isConnecting = true;
        if (joinButton != null)
            joinButton.interactable = false;

        if (backButton != null)
            backButton.interactable = false;

        // Store room data BEFORE attempting connection
        RoomData.RoomCode = roomCode;
        RoomData.PlayerRole = "client";

        // REMOVED: DontDestroyOnLoad(RoomData.gameObject);
        // Static RoomData values persist automatically across scenes

        if (!string.IsNullOrEmpty(hostIP))
        {
            // Manual connection with specific IP
            try
            {
                if (!IsValidIPAddress(hostIP))
                {
                    throw new System.FormatException("Invalid IP format");
                }

                IPEndPoint endpoint = new IPEndPoint(
                    IPAddress.Parse(hostIP),
                    NetworkManager.HOST_PORT
                );

                SetStatus($"Connecting to {hostIP}...");
                Debug.Log($"[JoinRoom] Manual connect → {hostIP}:{NetworkManager.HOST_PORT}");
                networkManager.JoinHost(roomCode, endpoint);
            }
            catch (System.FormatException)
            {
                isConnecting = false;
                if (joinButton != null) joinButton.interactable = true;
                if (backButton != null) backButton.interactable = true;
                SetStatus("Invalid IP address format! Use format: 192.168.1.100");
                return;
            }
        }
        else
        {
            // Auto-discovery
            SetStatus("Searching for host on network...");
            Debug.Log($"[JoinRoom] Auto-discovering host for room: {roomCode}");
            networkManager.JoinHost(roomCode, null);
        }

        // Start timeout coroutine
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(ConnectionTimeout());
    }

    private IEnumerator ConnectionTimeout()
    {
        // Longer timeout to account for discovery (10s) + join retries (1.5s) + join timeout (6s)
        float timeoutDuration = 20f;
        float startTime = Time.time;

        while (Time.time - startTime < timeoutDuration && isConnecting)
        {
            // Update status with elapsed time
            if (statusText != null && isConnecting)
            {
                int secondsRemaining = Mathf.CeilToInt(timeoutDuration - (Time.time - startTime));
                statusText.text = $"Connecting... ({secondsRemaining}s)";
            }
            yield return new WaitForSeconds(0.5f);
        }

        if (isConnecting)
        {
            Debug.Log("[JoinRoom] Connection timeout reached.");
            isConnecting = false;

            if (joinButton != null)
                joinButton.interactable = true;
            if (backButton != null)
                backButton.interactable = true;

            SetStatus("Connection timed out. Make sure both devices are on the same WiFi.");

            // Stop any ongoing connection attempts
            if (networkManager.Client != null)
            {
                networkManager.Client.Reset();
            }
        }

        timeoutCoroutine = null;
    }

    private void OnBackClicked()
    {
        if (isConnecting)
        {
            // If connecting, stop the attempt first
            Debug.Log("[JoinRoom] Cancelling connection attempt...");
            isConnecting = false;

            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            if (networkManager.Client != null)
            {
                networkManager.Client.Reset();
            }
        }

        SceneManager.LoadScene("UI_MainMenu");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[JoinRoom] Settings clicked");
        // Add settings panel logic here if needed
    }

    // ── Network callbacks ─────────────────────────────────────────────────────

    private void OnConnected()
    {
        Debug.Log("[JoinRoom] OnConnected called!");

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        isConnecting = false;
        SetStatus("Connected! Loading lobby...");

        // Small delay to show success message before scene loads
        StartCoroutine(LoadLobbyWithDelay());
    }

    private IEnumerator LoadLobbyWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("UI_LobbyRoom");
    }

    private void OnConnectionFailed(string reason)
    {
        Debug.LogError($"[JoinRoom] Connection failed: {reason}");

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        isConnecting = false;

        if (joinButton != null)
            joinButton.interactable = true;
        if (backButton != null)
            backButton.interactable = true;

        SetStatus($"Failed: {reason}");

        // Reset client state on failure
        if (networkManager.Client != null)
        {
            networkManager.Client.Reset();
        }
    }

    private void OnClientMessage(string message)
    {
        // Handle any unexpected messages while in join screen
        Debug.Log($"[JoinRoom] Received message while joining: {message}");

        // Could be used for additional connection status updates
        if (message == RoomProtocol.FULL)
        {
            OnConnectionFailed("Room is already full");
        }
        else if (message == "WRONG_CODE")
        {
            OnConnectionFailed("Invalid room code");
        }
        else if (message == "SAME_DEVICE")
        {
            OnConnectionFailed("Cannot connect to yourself! Use a different device");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsValidIPAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return false;

        string[] parts = ip.Split('.');
        if (parts.Length != 4) return false;

        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int num))
                return false;
            if (num < 0 || num > 255)
                return false;
        }

        return true;
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[JoinRoom] Status: {message}");
        if (statusText != null)
            statusText.text = message;
    }

    private void OnDestroy()
    {
        Debug.Log("[JoinRoom] Cleaning up event subscriptions...");

        if (networkManager != null && networkManager.Client != null)
        {
            networkManager.Client.OnConnected -= OnConnected;
            networkManager.Client.OnConnectionFailed -= OnConnectionFailed;
            networkManager.Client.OnMessageReceived -= OnClientMessage;
        }

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }
}