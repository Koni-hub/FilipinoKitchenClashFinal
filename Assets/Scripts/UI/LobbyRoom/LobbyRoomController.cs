using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Collections;

public class LobbyRoomController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Image statusImage;
    [SerializeField] private TextMeshProUGUI ipPlaceHolder;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI playersCountText;
    [SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject playerJoinedPanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    [Header("Scene")]
    [SerializeField] private string characterSelectionSceneName = "UI_CharacterSelection";

    private NetworkManager networkManager;
    private bool isHost = false;
    private bool hasClientConnected = false;
    private bool hasSentReady = false;
    private bool isReconnecting = false;
    private float connectionMonitorTimer = 0f;
    private float reconnectAttemptTimer = 0f;
    private int reconnectAttempts = 0;
    private const int MAX_RECONNECT_ATTEMPTS = 5;

    private void Start()
    {
        networkManager = NetworkManager.Instance;

        if (networkManager == null)
        {
            Debug.LogError("[Lobby] NetworkManager not found!");
            return;
        }

        isHost = (RoomData.PlayerRole == "host");

        Debug.Log($"[Lobby] Start - isHost: {isHost}, RoomCode: {RoomData.RoomCode}");

        if (isHost)
            InitializeAsHost();
        else
            InitializeAsClient();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void Update()
    {
        if (!isHost && networkManager?.Client != null)
        {
            if (!networkManager.Client.IsConnected && !isReconnecting)
            {
                connectionMonitorTimer += Time.deltaTime;
                if (connectionMonitorTimer > 2f)
                {
                    connectionMonitorTimer = 0f;
                    Debug.LogWarning("[Lobby] Client lost connection! Attempting to reconnect...");
                    StartReconnection();
                }
            }
            else if (networkManager.Client.IsConnected)
            {
                connectionMonitorTimer = 0f;
                reconnectAttempts = 0;
                isReconnecting = false;

                if (hasSentReady && waitingForPlayerPanel.activeSelf)
                {
                    UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                    {
                        UpdatePlayersCount(2, 2);
                        UpdateUIStatus("Connected to host! Waiting for game to start...", Color.green);
                        SetPanel(waitingForPlayerPanel, false);
                        SetPanel(playerJoinedPanel, true);
                    });
                }
            }

            if (isReconnecting)
            {
                reconnectAttemptTimer += Time.deltaTime;
                if (reconnectAttemptTimer >= 3f)
                {
                    reconnectAttemptTimer = 0f;
                    AttemptReconnect();
                }
            }
        }
    }

    private void StartReconnection()
    {
        isReconnecting = true;
        reconnectAttempts = 0;
        reconnectAttemptTimer = 0f;

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            UpdateUIStatus("Connection lost! Reconnecting...", Color.red);
            SetPanel(waitingForPlayerPanel, true);
            SetPanel(playerJoinedPanel, false);
        });
    }

    private void AttemptReconnect()
    {
        if (reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
        {
            Debug.LogError("[Lobby] Max reconnection attempts reached. Returning to join room...");
            isReconnecting = false;
            ReturnToJoinRoom();
            return;
        }

        reconnectAttempts++;
        Debug.Log($"[Lobby] Reconnection attempt {reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            UpdateUIStatus($"Reconnecting... Attempt {reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}", Color.yellow);
        });

        if (networkManager.Client != null)
        {
            networkManager.Client.Reset();
            string roomCode = RoomData.RoomCode;
            networkManager.JoinHost(roomCode, null);
        }
    }

    public void OnBackClicked()
    {
        Debug.Log("[Lobby] Back button clicked.");

        if (isHost)
        {
            networkManager.StopHost();
            SceneManager.LoadScene("UI_MainMenu");
        }
        else
        {
            isReconnecting = false;
            networkManager.StopClient();
            SceneManager.LoadScene("UI_JoinRoom");
        }
    }

    private void InitializeAsHost()
    {
        Debug.Log("[Lobby] Initializing as HOST");
        networkManager.StartHost(RoomData.RoomCode);

        if (networkManager.Host != null)
        {
            networkManager.Host.OnClientJoined += HandleClientJoined;
            networkManager.Host.OnClientLeft += HandleClientLeft;
            networkManager.Host.OnRoomFull += HandleRoomFull;
            networkManager.Host.OnMessageReceived += HandleHostMessage;
            Debug.Log("[Lobby] Host events subscribed successfully");
        }

        SetRoomCodeText(RoomData.RoomCode);
        UpdatePlayersCount(1, 2);
        UpdateUIStatus("Waiting for player to join...", Color.yellow);
        SetPanel(waitingForPlayerPanel, true);
        SetPanel(playerJoinedPanel, false);

        if (startGameButton != null)
            startGameButton.interactable = false;

        if (ipPlaceHolder != null)
        {
            string hostIP = networkManager.GetLocalIPAddress();
            ipPlaceHolder.text = $"Host IP: {hostIP}\nRoom Code: {RoomData.RoomCode}\n\nWaiting for player...";
        }
    }

    private void InitializeAsClient()
    {
        Debug.Log("[Lobby] Initializing as CLIENT");

        bool alreadyConnected = false;

        if (networkManager.Client != null)
        {
            alreadyConnected = networkManager.Client.IsConnected;
            Debug.Log($"[Lobby] Client already connected: {alreadyConnected}");

            networkManager.Client.OnConnected += HandleClientConnected;
            networkManager.Client.OnConnectionFailed += HandleConnectionFailed;
            networkManager.Client.OnMessageReceived += HandleClientMessage;
            networkManager.Client.OnHostDisconnected += HandleHostDisconnected;

            if (alreadyConnected && !hasSentReady)
            {
                HandleClientConnected();
            }
        }
        else
        {
            Debug.LogError("[Lobby] Client is null!");
        }

        SetRoomCodeText(RoomData.RoomCode);

        if (alreadyConnected)
        {
            UpdatePlayersCount(2, 2);
            UpdateUIStatus("Connected to host! Waiting for game to start...", Color.green);
            SetPanel(waitingForPlayerPanel, false);
            SetPanel(playerJoinedPanel, true);
        }
        else
        {
            UpdatePlayersCount(1, 2);
            UpdateUIStatus("Connecting to host...", Color.yellow);
            SetPanel(waitingForPlayerPanel, true);
            SetPanel(playerJoinedPanel, false);
        }

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        if (ipPlaceHolder != null)
        {
            string clientIP = networkManager.GetLocalIPAddress();
            ipPlaceHolder.text = alreadyConnected ? $"Your IP: {clientIP}\n✓ Connected to host" : $"Your IP: {clientIP}\nConnecting to host...";
        }
    }

    // ── CLIENT EVENT HANDLERS ─────────────────────────────────────────────

    private void HandleClientConnected()
    {
        Debug.Log("[Lobby] CLIENT CONNECTED to host!");
        isReconnecting = false;
        reconnectAttempts = 0;

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            Debug.Log("[Lobby] Updating CLIENT UI - Connected!");
            UpdatePlayersCount(2, 2);
            UpdateUIStatus("Connected to host! Waiting for game to start...", Color.green);
            SetPanel(waitingForPlayerPanel, false);
            SetPanel(playerJoinedPanel, true);

            if (ipPlaceHolder != null)
            {
                string clientIP = networkManager.GetLocalIPAddress();
                ipPlaceHolder.text = $"Your IP: {clientIP}\n✓ Connected to host";
            }

            if (!hasSentReady && networkManager.Client != null)
            {
                hasSentReady = true;
                networkManager.Client.SendToHost("CLIENT_READY");
                Debug.Log("[Lobby] Sent CLIENT_READY to host");
            }
        });
    }

    private void HandleConnectionFailed(string errorMessage)
    {
        Debug.LogError($"[Lobby] Connection failed: {errorMessage}");

        if (!isReconnecting)
        {
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                UpdateUIStatus($"Connection failed: {errorMessage}", Color.red);
            });
        }
    }

    private void HandleClientMessage(string msg)
    {
        Debug.Log($"[Lobby] Client received message: {msg}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (msg == RoomProtocol.GO_CHARSELECT)
            {
                Debug.Log("[Lobby] Host says go to character selection.");
                LoadCharacterSelection();
            }
            else if (msg == "HOST_READY")
            {
                Debug.Log("[Lobby] Host is ready!");
                UpdateUIStatus("Host is ready! Waiting to start...", Color.green);
            }
        });
    }

    private void HandleHostDisconnected()
    {
        Debug.Log("[Lobby] Host disconnected!");

        if (!isReconnecting)
        {
            StartReconnection();
        }
    }

    // ── HOST EVENT HANDLERS ───────────────────────────────────────────────

    private void HandleClientJoined(IPEndPoint clientEndpoint)
    {
        Debug.Log($"[Lobby] Client joined from: {clientEndpoint}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            Debug.Log("[Lobby] Updating HOST UI - Client connected!");

            if (!hasClientConnected)
            {
                hasClientConnected = true;

                // Update host UI
                UpdatePlayersCount(2, 2);
                UpdateUIStatus("Player connected! Ready to start.", Color.green);
                SetPanel(waitingForPlayerPanel, false);
                SetPanel(playerJoinedPanel, true);

                if (startGameButton != null)
                {
                    startGameButton.interactable = true;
                    Debug.Log("[Lobby] Host start button enabled");
                }

                if (ipPlaceHolder != null)
                {
                    string hostIP = networkManager.GetLocalIPAddress();
                    ipPlaceHolder.text = $"Host IP: {hostIP}\nRoom Code: {RoomData.RoomCode}\n✓ Player connected!";
                }

                Debug.Log("[Lobby] Host UI update complete - Player count: 2/2");
            }
        });
    }

    private void HandleClientLeft(IPEndPoint clientEndpoint)
    {
        Debug.Log($"[Lobby] Client left: {clientEndpoint}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            hasClientConnected = false;

            // Update host UI
            UpdatePlayersCount(1, 2);
            UpdateUIStatus("Waiting for player to join...", Color.yellow);
            SetPanel(waitingForPlayerPanel, true);
            SetPanel(playerJoinedPanel, false);

            if (startGameButton != null)
                startGameButton.interactable = false;

            if (ipPlaceHolder != null)
            {
                string hostIP = networkManager.GetLocalIPAddress();
                ipPlaceHolder.text = $"Host IP: {hostIP}\nRoom Code: {RoomData.RoomCode}\nWaiting for player...";
            }

            Debug.Log("[Lobby] Host UI updated - Player left, waiting for reconnection");
        });
    }

    private void HandleHostMessage(string msg)
    {
        Debug.Log($"[Lobby] Host received message: {msg}");

        if (msg == "CLIENT_READY")
        {
            Debug.Log("[Lobby] Client is ready! Updating host UI...");

            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                UpdateUIStatus("Client ready! Press Start Game.", Color.green);
                if (startGameButton != null)
                {
                    startGameButton.interactable = true;
                }

                // Send acknowledgment back to client
                if (networkManager.Host != null)
                {
                    networkManager.Host.SendToClient("HOST_READY");
                    Debug.Log("[Lobby] Sent HOST_READY to client");
                }
            });
        }
    }

    private void HandleRoomFull()
    {
        Debug.Log("[Lobby] Room is full.");
    }

    public void OnStartGameClicked()
    {
        Debug.Log("[Lobby] OnStartGameClicked fired!");

        if (!isHost)
        {
            Debug.LogWarning("[Lobby] Client cannot start game!");
            return;
        }

        if (networkManager.Host == null || !networkManager.Host.HasClient)
        {
            Debug.LogWarning("[Lobby] Cannot start: No client connected.");
            UpdateUIStatus("Cannot start: No player connected!", Color.red);
            return;
        }

        Debug.Log("[Lobby] Sending GO_CHARSELECT to client and loading scene...");
        networkManager.Host.SendToClient(RoomProtocol.GO_CHARSELECT);
        LoadCharacterSelection();
    }

    private void LoadCharacterSelection()
    {
        Debug.Log($"[Lobby] Loading scene: {characterSelectionSceneName}");
        SceneManager.LoadScene(characterSelectionSceneName);
    }

    private void ReturnToJoinRoom()
    {
        Debug.Log("[Lobby] Returning to join room...");
        SceneManager.LoadScene("UI_JoinRoom");
    }

    private void SetRoomCodeText(string code)
    {
        if (roomCodeText != null)
            roomCodeText.text = $"Room Code: {code}";
    }

    private void UpdatePlayersCount(int current, int max)
    {
        if (playersCountText != null)
        {
            playersCountText.text = $"Players: {current}/{max}";
            playersCountText.color = current == max ? Color.green : Color.yellow;
            Debug.Log($"[Lobby] Players count updated: {current}/{max}");
        }
    }

    private void UpdateUIStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log($"[Lobby] Status updated: {message}");
        }
        if (statusImage != null)
            statusImage.color = color;
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
            Debug.Log($"[Lobby] Panel {panel.name} set to: {active}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[Lobby] OnDestroy - Cleaning up events");

        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (isHost && networkManager?.Host != null)
        {
            networkManager.Host.OnClientJoined -= HandleClientJoined;
            networkManager.Host.OnClientLeft -= HandleClientLeft;
            networkManager.Host.OnRoomFull -= HandleRoomFull;
            networkManager.Host.OnMessageReceived -= HandleHostMessage;
        }

        if (!isHost && networkManager?.Client != null)
        {
            networkManager.Client.OnConnected -= HandleClientConnected;
            networkManager.Client.OnConnectionFailed -= HandleConnectionFailed;
            networkManager.Client.OnMessageReceived -= HandleClientMessage;
            networkManager.Client.OnHostDisconnected -= HandleHostDisconnected;
        }
    }
}