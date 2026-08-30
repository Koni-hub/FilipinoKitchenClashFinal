using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private void Start()
    {
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);

        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
    }

    private void OnCreateRoomClicked()
    {
        RoomData.RoomCode = GenerateCode();
        RoomData.PlayerRole = "host";

        Debug.Log($"Creating room with code: {RoomData.RoomCode}");
        SceneManager.LoadScene("UI_LobbyRoom");
    }

    private void OnJoinRoomClicked()
    {
        RoomData.PlayerRole = "client";
        SceneManager.LoadScene("UI_JoinRoom");
    }

    private string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rand = new System.Random();

        char[] code = new char[6];
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[rand.Next(chars.Length)];
        }

        return new string(code);
    }

    private void OnDestroy()
    {
        if (createRoomButton != null)
            createRoomButton.onClick.RemoveAllListeners();

        if (joinRoomButton != null)
            joinRoomButton.onClick.RemoveAllListeners();
    }
}