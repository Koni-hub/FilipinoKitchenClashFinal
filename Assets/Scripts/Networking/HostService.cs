using System;
using System.Net;
using System.Text;
using UnityEngine;

[System.Serializable]
public class HostService
{
    private UdpTransport transport;
    private readonly string roomCode;
    private readonly string hostLocalIP;

    private IPEndPoint connectedClient = null;
    private bool isFull = false;

    // ── Heartbeat ─────────────────────────────────────────────────────────────
    private float lastPingFromClient = -1f;
    private float nextPingTime = -1f;
    private float connectedAt = -1f;
    private const float PING_INTERVAL = 5f;
    private const float PING_TIMEOUT = 30f;
    private const float GRACE_PERIOD = 15f;
    private volatile bool pingReceived = false;

    public event Action<IPEndPoint> OnClientJoined;
    public event Action OnRoomFull;
    public event Action<string> OnMessageReceived;
    public event Action<IPEndPoint> OnClientLeft;

    public bool IsFull => isFull;
    public bool HasClient => connectedClient != null;

    public HostService(int hostPort, string roomCode)
    {
        this.roomCode = roomCode;
        this.hostLocalIP = NetworkManager.Instance.GetLocalIPAddress();

        Debug.Log($"[HOST] Starting with code: {roomCode} on port {hostPort}");
        Debug.Log($"[HOST] Local IP: {hostLocalIP}");

        try
        {
            transport = new UdpTransport(hostPort, true);
            transport.OnDataReceived += HandleData;
            Debug.Log("[HOST] Listening for players...");
        }
        catch (Exception e)
        {
            Debug.LogError($"[HOST] Transport init failed: {e.Message}");
        }
    }

    public void Tick(float time)
    {
        if (connectedClient == null) return;

        if (pingReceived)
        {
            pingReceived = false;
            lastPingFromClient = time;
            Debug.Log($"[HOST] Ping received from client at time: {time}");
        }

        if (nextPingTime > 0f && time >= nextPingTime)
        {
            nextPingTime = time + PING_INTERVAL;
            transport?.Send(RoomProtocol.PING, connectedClient);
            Debug.Log($"[HOST] Sent PING to client at time: {time}");
        }

        if (connectedAt < 0f || (time - connectedAt) < GRACE_PERIOD)
        {
            return;
        }

        if (lastPingFromClient > 0f && (time - lastPingFromClient) > PING_TIMEOUT)
        {
            Debug.Log($"[HOST] Client timed out! Clearing connection...");
            ClearClientConnection();
            OnClientLeft?.Invoke(connectedClient);
        }
    }

    // Add this method to properly clear client connection
    private void ClearClientConnection()
    {
        connectedClient = null;
        isFull = false;
        lastPingFromClient = -1f;
        nextPingTime = -1f;
        connectedAt = -1f;
        pingReceived = false;
        Debug.Log("[HOST] Client connection cleared, room is now available for new connections");
    }

    // Add this method to manually force disconnect (useful for reconnection scenarios)
    public void ForceDisconnectClient()
    {
        if (connectedClient != null)
        {
            Debug.Log("[HOST] Force disconnecting client for reconnection");
            ClearClientConnection();
        }
    }

    private void HandleData(byte[] data, IPEndPoint remote)
    {
        string msg = Encoding.UTF8.GetString(data).Trim();
        string clientIP = remote.Address.ToString();

        Debug.Log($"[HOST] Received '{msg}' from {clientIP}:{remote.Port}");

        if (msg == RoomProtocol.PING)
        {
            pingReceived = true;
            return;
        }

        if (msg.StartsWith(RoomProtocol.DISCOVER + "|"))
        {
            string[] parts = msg.Split('|');
            if (parts.Length >= 2 && parts[1] == roomCode)
            {
                string response = $"{RoomProtocol.HOST}|{roomCode}|{hostLocalIP}";
                transport.Send(response, remote);
                Debug.Log($"[HOST] Replied to discovery from {clientIP}");
            }
            return;
        }

        if (msg.StartsWith(RoomProtocol.JOIN + "|"))
        {
            string[] parts = msg.Split('|');
            if (parts.Length < 2)
            {
                transport.Send("WRONG_CODE", remote);
                return;
            }

            string receivedCode = parts[1];
            Debug.Log($"[HOST] Join request — received code: '{receivedCode}', expected: '{roomCode}'");

            if (receivedCode != roomCode)
            {
                transport.Send("WRONG_CODE", remote);
                Debug.Log("[HOST] Rejected: wrong code.");
                return;
            }

            // Check if this is a reconnection from the same client
            bool isReconnecting = (connectedClient != null &&
                                   connectedClient.Address.Equals(remote.Address) &&
                                   connectedClient.Port == remote.Port);

            if (isReconnecting)
            {
                Debug.Log("[HOST] Client is reconnecting! Clearing old connection and accepting new one.");
                ClearClientConnection();
            }

            if (isFull || connectedClient != null)
            {
                transport.Send(RoomProtocol.FULL, remote);
                Debug.Log($"[HOST] Rejected: room full. Current client: {connectedClient}");
                UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnRoomFull?.Invoke());
                return;
            }

            connectedClient = remote;
            isFull = true;
            transport.Send(RoomProtocol.ACCEPT, remote);
            Debug.Log($"[HOST] Player ACCEPTED! IP: {clientIP}");

            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                float currentTime = Time.time;
                connectedAt = currentTime;
                lastPingFromClient = currentTime;
                nextPingTime = currentTime + PING_INTERVAL;
                OnClientJoined?.Invoke(remote);
                Debug.Log($"[HOST] Heartbeat initialized for client");
            });

            return;
        }

        if (msg == RoomProtocol.ROLE_ACK)
        {
            Debug.Log("[HOST] Client acknowledged role.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
            return;
        }

        Debug.LogWarning($"[HOST] Unknown message: '{msg}'");
        UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
    }

    public void SendToClient(string message)
    {
        if (connectedClient == null)
        {
            Debug.LogWarning("[HOST] SendToClient called but no client is connected.");
            return;
        }

        if (transport == null)
        {
            Debug.LogWarning("[HOST] SendToClient called but transport is null.");
            return;
        }

        transport.Send(message, connectedClient);
        Debug.Log($"[HOST] Sent to client: {message}");
    }

    public void Dispose()
    {
        Debug.Log("[HOST] Disposing...");
        transport?.Dispose();
        transport = null;
    }
}