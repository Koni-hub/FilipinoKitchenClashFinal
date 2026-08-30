using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Role Display")]
    [SerializeField] private TextMeshProUGUI playerH1Role_Txt;
    [SerializeField] private TextMeshProUGUI playerC2Role_Txt;

    [Header("Character Visuals")]
    [SerializeField] private Image hostCharacterImage;
    [SerializeField] private Image clientCharacterImage;
    [SerializeField] private Sprite[] characterSprites; // [0] = Prep, [1] = Cook

    [Header("UI")]
    [SerializeField] private Button rngButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject revealPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private bool isHost;
    private bool roleAssigned = false;
    private bool clientAcknowledged = false;
    private NetworkManager networkManager;
    private bool isRevealing = false;
    private bool gameStarting = false;
    private bool isReconnecting = false;
    private float reconnectTimer = 0f;
    private const float RECONNECT_DELAY = 3f;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Start()
    {
        networkManager = NetworkManager.Instance;

        if (networkManager == null)
        {
            Debug.LogError("[CharSelect] NetworkManager not found!");
            return;
        }

        isHost = (RoomData.PlayerRole == "host");

        // ── Wire up buttons via code ──────────────────────────────────────────
        if (rngButton != null)
        {
            rngButton.onClick.RemoveAllListeners();
            rngButton.onClick.AddListener(RNGRole);
            rngButton.gameObject.SetActive(isHost);
        }
        else
        {
            Debug.LogError("[CharSelect] rngButton is NULL!");
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }
        else
        {
            Debug.LogError("[CharSelect] confirmButton is NULL!");
        }

        if (revealPanel != null)
            revealPanel.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        ResetRoleText();

        if (isHost)
        {
            SetStatus("Click ROLL to assign roles!");
            SubscribeHostEvents();
        }
        else
        {
            SetStatus("Waiting for host to assign roles...");
            SubscribeClientEvents();
        }
    }

    private void Update()
    {
        // Handle reconnection for client
        if (!isHost && isReconnecting)
        {
            reconnectTimer += Time.deltaTime;
            if (reconnectTimer >= RECONNECT_DELAY)
            {
                reconnectTimer = 0f;
                AttemptReconnect();
            }
        }

        // Check connection status for client
        if (!isHost && networkManager?.Client != null && !isReconnecting)
        {
            if (!networkManager.Client.IsConnected)
            {
                Debug.LogWarning("[CharSelect] Connection lost! Attempting to reconnect...");
                StartReconnection();
            }
        }
    }

    private void SubscribeHostEvents()
    {
        if (networkManager.Host != null)
        {
            networkManager.Host.OnMessageReceived += HandleHostMessage;
            networkManager.Host.OnClientLeft += HandleClientLeft;
            Debug.Log("[CharSelect] Host events subscribed");
        }
        else
        {
            Debug.LogError("[CharSelect] Host is null!");
        }
    }

    private void SubscribeClientEvents()
    {
        if (networkManager.Client != null)
        {
            networkManager.Client.OnMessageReceived += HandleClientMessage;
            networkManager.Client.OnHostDisconnected += HandleHostDisconnected;
            networkManager.Client.OnConnected += HandleClientReconnected;
            Debug.Log("[CharSelect] Client events subscribed");
        }
        else
        {
            Debug.LogError("[CharSelect] Client is null!");
        }
    }

    private void StartReconnection()
    {
        isReconnecting = true;
        reconnectTimer = 0f;

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            SetStatus("Connection lost! Reconnecting...");
            if (rngButton != null) rngButton.interactable = false;
            if (confirmButton != null) confirmButton.interactable = false;
        });
    }

    private void AttemptReconnect()
    {
        Debug.Log("[CharSelect] Attempting to reconnect...");

        if (networkManager.Client != null)
        {
            networkManager.Client.Reset();
            string roomCode = RoomData.RoomCode;
            networkManager.JoinHost(roomCode, null);
        }
    }

    private void HandleClientReconnected()
    {
        Debug.Log("[CharSelect] Client reconnected to host!");
        isReconnecting = false;

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            SetStatus("Reconnected! Waiting for host to assign roles...");

            // Request role assignment again
            if (networkManager.Client != null)
            {
                networkManager.Client.SendToHost("REQUEST_ROLE");
            }
        });
    }

    private void HandleHostDisconnected()
    {
        Debug.Log("[CharSelect] Host disconnected!");

        if (!isReconnecting)
        {
            StartReconnection();
        }
    }

    private void HandleClientLeft(IPEndPoint clientEndpoint)
    {
        Debug.Log("[CharSelect] Client disconnected!");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            SetStatus("Client disconnected! Waiting for reconnect...");
            clientAcknowledged = false;
            roleAssigned = false;

            if (confirmButton != null)
                confirmButton.interactable = false;

            ResetRoleText();
            if (revealPanel != null)
                revealPanel.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (rngButton != null)
            rngButton.onClick.RemoveListener(RNGRole);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);

        if (isHost && networkManager?.Host != null)
        {
            networkManager.Host.OnMessageReceived -= HandleHostMessage;
            networkManager.Host.OnClientLeft -= HandleClientLeft;
        }

        if (!isHost && networkManager?.Client != null)
        {
            networkManager.Client.OnMessageReceived -= HandleClientMessage;
            networkManager.Client.OnHostDisconnected -= HandleHostDisconnected;
            networkManager.Client.OnConnected -= HandleClientReconnected;
        }
    }

    // ── Network message handlers ───────────────────────────────────────────────

    private void HandleHostMessage(string msg)
    {
        Debug.Log($"[CharSelect] Host received: {msg}");

        if (msg == RoomProtocol.ROLE_ACK)
        {
            Debug.Log("[CharSelect] Client acknowledged roles.");
            clientAcknowledged = true;

            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                SetStatus("Client ready! Press CONFIRM to start.");
                if (confirmButton != null)
                    confirmButton.interactable = true;
            });
        }
        else if (msg == "REQUEST_ROLE")
        {
            Debug.Log("[CharSelect] Client requested role assignment, resending...");
            if (roleAssigned)
            {
                string roleMsg = RoomProtocol.BuildRoleAssign(RoomData.GamePlayerRole);
                networkManager.Host.SendToClient(roleMsg);
                Debug.Log($"[CharSelect] Resent role: {RoomData.GamePlayerRole}");
            }
        }
    }

    private void HandleClientMessage(string msg)
    {
        Debug.Log($"[CharSelect] Client received: {msg}");

        if (msg.StartsWith(RoomProtocol.ROLE_ASSIGN + "|"))
        {
            int hostRoleIndex = RoomProtocol.ParseRoleIndex(msg);

            if (hostRoleIndex < 0)
            {
                Debug.LogError($"[CharSelect] Failed to parse role index from: {msg}");
                return;
            }

            RoomData.GamePlayerRole = (RoomData.PlayerRoleEnum)hostRoleIndex;
            roleAssigned = true;

            Debug.Log($"[CharSelect] Role received from host: {RoomData.GamePlayerRole}");

            if (networkManager.Client != null)
            {
                networkManager.Client.SendToHost(RoomProtocol.ROLE_ACK);
                Debug.Log("[CharSelect] Sent ROLE_ACK to host");
            }

            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                StartCoroutine(RevealSequence());
            });
            return;
        }

        if (msg == RoomProtocol.START_GAME && !gameStarting)
        {
            Debug.Log("[CharSelect] Start game received from host.");
            gameStarting = true;
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                LoadGameScene();
            });
        }
    }

    // ── RNG Button (Host only) ─────────────────────────────────────────────────

    public void RNGRole()
    {
        if (!isHost)
        {
            Debug.LogWarning("[CharSelect] RNGRole called on client — ignoring.");
            return;
        }

        if (gameStarting)
        {
            Debug.Log("[CharSelect] Game already starting, ignoring roll.");
            return;
        }

        Debug.Log("[CharSelect] Rolling random role...");

        clientAcknowledged = false;
        roleAssigned = true;

        if (confirmButton != null)
            confirmButton.interactable = false;

        RoomData.GamePlayerRole = (RoomData.PlayerRoleEnum)Random.Range(0, 2);

        string roleMsg = RoomProtocol.BuildRoleAssign(RoomData.GamePlayerRole);

        if (networkManager.Host != null)
        {
            networkManager.Host.SendToClient(roleMsg);
            Debug.Log($"[CharSelect] Assigned roles — Host: {RoomData.GamePlayerRole}, sent: {roleMsg}");
        }
        else
        {
            Debug.LogError("[CharSelect] Host is null! Cannot send role assignment.");
            return;
        }

        SetStatus("Waiting for client to acknowledge...");
        StartCoroutine(RevealSequence());
    }

    // ── Confirm Button (Host only) ─────────────────────────────────────────────

    public void OnConfirmClicked()
    {
        if (!isHost)
        {
            Debug.LogWarning("[CharSelect] OnConfirmClicked called on client — ignoring.");
            return;
        }

        if (gameStarting)
        {
            Debug.Log("[CharSelect] Game already starting, ignoring confirm.");
            return;
        }

        if (!roleAssigned)
        {
            Debug.LogWarning("[CharSelect] No role assigned yet — roll first.");
            SetStatus("Roll first before confirming!");
            return;
        }

        if (!clientAcknowledged)
        {
            Debug.LogWarning("[CharSelect] Client has not acknowledged yet.");
            SetStatus("Waiting for client to acknowledge...");
            return;
        }

        Debug.Log("[CharSelect] Roles confirmed — sending START_GAME.");
        gameStarting = true;

        if (networkManager.Host != null)
        {
            networkManager.Host.SendToClient(RoomProtocol.START_GAME);
            LoadGameScene();
        }
        else
        {
            Debug.LogError("[CharSelect] Host is null! Cannot send start game.");
        }
    }

    // ── Reveal sequence ────────────────────────────────────────────────────────

    private IEnumerator RevealSequence()
    {
        if (isRevealing) yield break;
        isRevealing = true;

        Debug.Log("[CharSelect] Starting reveal sequence...");

        if (rngButton != null)
            rngButton.interactable = false;
        if (confirmButton != null)
            confirmButton.interactable = false;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            string[] steps = { "3", "2", "1", "GO!" };

            foreach (string step in steps)
            {
                countdownText.text = step;
                yield return new WaitForSeconds(0.6f);
            }

            countdownText.gameObject.SetActive(false);
        }

        ApplyRolesToUI();

        if (revealPanel != null)
        {
            revealPanel.SetActive(true);

            // Fade in animation
            CanvasGroup canvasGroup = revealPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                float fadeTime = 0.3f;
                for (float t = 0; t < fadeTime; t += Time.deltaTime)
                {
                    canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeTime);
                    yield return null;
                }
                canvasGroup.alpha = 1;
            }
        }

        yield return StartCoroutine(PunchScale(playerH1Role_Txt?.transform, 0.3f));
        yield return StartCoroutine(PunchScale(playerC2Role_Txt?.transform, 0.3f));

        if (rngButton != null && isHost && !gameStarting)
            rngButton.interactable = true;

        if (!isHost)
        {
            SetStatus("Waiting for host to start the game...");
        }
        else
        {
            if (clientAcknowledged)
            {
                SetStatus("Client ready! Press CONFIRM to start.");
                if (confirmButton != null && !gameStarting)
                    confirmButton.interactable = true;
            }
            else
            {
                SetStatus("Waiting for client to acknowledge...");
            }
        }

        isRevealing = false;
    }

    private IEnumerator PunchScale(Transform target, float duration)
    {
        if (target == null) yield break;

        Vector3 original = target.localScale;
        Vector3 big = original * 1.4f;
        float half = duration / 2f;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(original, big, t / half);
            yield return null;
        }

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(big, original, t / half);
            yield return null;
        }

        target.localScale = original;
    }

    // ── UI Helpers ─────────────────────────────────────────────────────────────

    private void ApplyRolesToUI()
    {
        RoomData.PlayerRoleEnum hostRole = RoomData.GamePlayerRole;
        RoomData.PlayerRoleEnum clientRole = hostRole == RoomData.PlayerRoleEnum.Prep
                                           ? RoomData.PlayerRoleEnum.Cook
                                           : RoomData.PlayerRoleEnum.Prep;

        string hostRoleName = GetRoleName(hostRole);
        string clientRoleName = GetRoleName(clientRole);

        Debug.Log($"[CharSelect] Applying roles - Host: {hostRoleName}, Client: {clientRoleName}");

        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (playerH1Role_Txt != null)
                playerH1Role_Txt.text = $"Player 1 {(isHost ? "(Host)" : "(Client)")}\n<size=120%><b>{hostRoleName}</b></size>";

            if (playerC2Role_Txt != null)
                playerC2Role_Txt.text = $"Player 2 {(isHost ? "(Client)" : "(Host)")}\n<size=120%><b>{clientRoleName}</b></size>";

            if (characterSprites != null && characterSprites.Length >= 2)
            {
                if (hostCharacterImage != null)
                    hostCharacterImage.sprite = characterSprites[(int)hostRole];

                if (clientCharacterImage != null)
                    clientCharacterImage.sprite = characterSprites[(int)clientRole];
            }
        });
    }

    private void ResetRoleText()
    {
        if (playerH1Role_Txt != null)
            playerH1Role_Txt.text = "Player 1\n<b>???</b>";

        if (playerC2Role_Txt != null)
            playerC2Role_Txt.text = "Player 2\n<b>???</b>";
    }

    private string GetRoleName(RoomData.PlayerRoleEnum role)
    {
        return role switch
        {
            RoomData.PlayerRoleEnum.Prep => "Prep Chef",
            RoomData.PlayerRoleEnum.Cook => "Head Cook",
            _ => "Unknown"
        };
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[CharSelect] Status: {message}");
        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            if (statusText != null)
                statusText.text = message;
        });
    }

    // ── Scene loading ──────────────────────────────────────────────────────────

    private void LoadGameScene()
    {
        Debug.Log($"[CharSelect] Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
}