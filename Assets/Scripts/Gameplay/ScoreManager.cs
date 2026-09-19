using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int hostScore = 0;
    public int clientScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsHost)
        {
            hostScore += amount;
            Debug.Log($"[ScoreManager] Host score: {hostScore}");
        }
        else
        {
            clientScore += amount;
            Debug.Log($"[ScoreManager] Client score: {clientScore}");
        }

        if (CookingSync.Instance != null)
        {
            CookingSync.Instance.SendScoreUpdate(hostScore, clientScore);
        }
    }

    public void SetScores(int host, int client)
    {
        hostScore = host;
        clientScore = client;
        Debug.Log($"[ScoreManager] Scores updated: Host={hostScore}, Client={clientScore}");
    }
}
