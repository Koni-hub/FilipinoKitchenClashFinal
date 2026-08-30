using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ClientService : IDisposable
{
    private UdpTransport transport;
    private IPEndPoint hostEndpoint;

    private bool isConnected = false;
    private bool isJoining = false;
    private bool isDiscovering = false;

    private string pendingRoomCode = "";

    // ── Heartbeat ─────────────────────────────────────────────────────────────
    private float lastPingFromHost = -1f;   // -1 = not started yet
    private float nextPingTime = -1f;   // -1 = not started yet
    private float connectedAt = -1f;   // time connection was accepted
    private const float PING_INTERVAL = 5f;     // Match host
    private const float PING_TIMEOUT = 30f;     // Match host
    private const float GRACE_PERIOD = 15f;     // Match host
    private volatile bool pingReceived = false;

    public event Action OnConnected;
    public event Action<string> OnConnectionFailed;
    public event Action<string> OnMessageReceived;
    public event Action OnHostDisconnected;

    public bool IsConnected => isConnected;

    public ClientService(int clientPort, int hostPort)
    {
        Debug.Log($"[CLIENT] Initializing on port {clientPort}");

        try
        {
            transport = new UdpTransport(clientPort, true);
            transport.OnDataReceived += HandleData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CLIENT] Init failed: {e.Message}");
        }
    }

    // ── Tick — called from NetworkManager.Update() (main thread) ─────────────
    public void Tick(float time)
    {
        if (!isConnected || hostEndpoint == null) return;

        if (pingReceived)
        {
            pingReceived = false;
            lastPingFromHost = time;
            Debug.Log($"[CLIENT] Ping received from host at time: {time}");
        }

        if (nextPingTime > 0f && time >= nextPingTime)
        {
            nextPingTime = time + PING_INTERVAL;
            transport?.Send(RoomProtocol.PING, hostEndpoint);
            Debug.Log($"[CLIENT] Sent PING to host at time: {time}");
        }

        if (connectedAt < 0f || (time - connectedAt) < GRACE_PERIOD)
        {
            Debug.Log($"[CLIENT] Grace period active: {time - connectedAt:F2}s / {GRACE_PERIOD}s");
            return;
        }

        if (lastPingFromHost > 0f && (time - lastPingFromHost) > PING_TIMEOUT)
        {
            Debug.LogError($"[CLIENT] Host timed out! Last ping: {time - lastPingFromHost:F2}s ago");
            isConnected = false;
            lastPingFromHost = -1f;
            nextPingTime = -1f;
            connectedAt = -1f;

            OnHostDisconnected?.Invoke();
        }
    }

    // ── Reset ──────────────────────────────────────────────────────────────────

    public void Reset()
    {
        Debug.Log("[CLIENT] Resetting state for new attempt.");
        isConnected = false;
        isJoining = false;
        isDiscovering = false;
        hostEndpoint = null;
        pendingRoomCode = "";
        lastPingFromHost = -1f;
        nextPingTime = -1f;
        connectedAt = -1f;
        pingReceived = false;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void DiscoverHost(string roomCode)
    {
        Reset();
        isDiscovering = true;
        pendingRoomCode = roomCode;

        Debug.Log($"[CLIENT] Starting discovery for room: {roomCode}");
        NetworkManager.Instance.StartCoroutine(DiscoveryWithRetry(roomCode));
    }

    public void JoinRoom(string roomCode, IPEndPoint hostEndPoint)
    {
        Debug.Log($"[CLIENT] JoinRoom called | isConnected={isConnected} isJoining={isJoining} endpoint={hostEndPoint}");

        if (isConnected)
        {
            Debug.LogWarning("[CLIENT] JoinRoom called but already connected.");
            return;
        }

        if (isJoining)
        {
            Debug.LogWarning("[CLIENT] JoinRoom called but already joining.");
            return;
        }

        pendingRoomCode = roomCode;
        isJoining = true;
        hostEndpoint = hostEndPoint;

        Debug.Log($"[CLIENT] Starting join to {hostEndpoint} with code '{roomCode}'");
        NetworkManager.Instance.StartCoroutine(SendJoinWithRetry(roomCode, hostEndPoint));
    }

    // ── Broadcast helpers ──────────────────────────────────────────────────────

    private List<IPAddress> GetAllBroadcastAddresses()
    {
        var addresses = new List<IPAddress>();
        addresses.Add(IPAddress.Broadcast);

        try
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                    string ipStr = ip.Address.ToString();
                    if (ipStr.StartsWith("127.")) continue;

                    byte[] ipBytes = ip.Address.GetAddressBytes();
                    byte[] maskBytes = ip.IPv4Mask.GetAddressBytes();
                    byte[] broadcastBytes = new byte[4];

                    for (int i = 0; i < 4; i++)
                        broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);

                    IPAddress broadcastAddr = new IPAddress(broadcastBytes);

                    if (!addresses.Contains(broadcastAddr))
                    {
                        addresses.Add(broadcastAddr);
                        Debug.Log($"[CLIENT] Found broadcast address: {broadcastAddr} (from {ipStr})");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CLIENT] Could not enumerate network interfaces: {e.Message}");
        }

        return addresses;
    }

    private void BroadcastToAll(string msg, int port)
    {
        foreach (IPAddress addr in GetAllBroadcastAddresses())
        {
            try
            {
                transport.Send(msg, new IPEndPoint(addr, port));
                Debug.Log($"[CLIENT] Broadcast '{msg}' -> {addr}:{port}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CLIENT] Broadcast to {addr} failed: {e.Message}");
            }
        }
    }

    // ── Retry coroutines ───────────────────────────────────────────────────────

    private IEnumerator DiscoveryWithRetry(string roomCode)
    {
        const float retryInterval = 1f;
        const float totalTimeout = 10f;
        float elapsed = 0f;

        string discoverMsg = $"{RoomProtocol.DISCOVER}|{roomCode}";

        while (elapsed < totalTimeout && !isConnected && isDiscovering)
        {
            BroadcastToAll(discoverMsg, NetworkManager.HOST_PORT);
            Debug.Log($"[CLIENT] Discovery attempt #{Mathf.FloorToInt(elapsed / retryInterval) + 1} for room: {roomCode}");

            yield return new WaitForSeconds(retryInterval);
            elapsed += retryInterval;
        }

        if (isDiscovering && !isConnected)
        {
            isDiscovering = false;
            Debug.Log("[CLIENT] Discovery timed out after retries.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                OnConnectionFailed?.Invoke("No host found. Make sure both devices are on the same WiFi or Hotspot.");
            });
        }
    }

    private IEnumerator SendJoinWithRetry(string roomCode, IPEndPoint endpoint)
    {
        string joinMsg = $"{RoomProtocol.JOIN}|{roomCode}";

        for (int i = 0; i < 3; i++)
        {
            if (isConnected) yield break;

            transport.Send(joinMsg, endpoint);
            Debug.Log($"[CLIENT] JOIN attempt #{i + 1} to {endpoint}");

            yield return new WaitForSeconds(0.5f);
        }

        NetworkManager.Instance.StartCoroutine(JoinTimeout());
    }

    private IEnumerator JoinTimeout()
    {
        yield return new WaitForSeconds(6f);

        if (isJoining && !isConnected)
        {
            isJoining = false;
            Debug.Log("[CLIENT] Join timed out.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                OnConnectionFailed?.Invoke("Connection timed out. Try again.");
            });
        }
    }

    // ── Data handler ───────────────────────────────────────────────────────────

    private void HandleData(byte[] data, IPEndPoint remote)
    {
        string msg = Encoding.UTF8.GetString(data).Trim();
        Debug.Log($"[CLIENT] Received '{msg}' from {remote} | discovering={isDiscovering} joining={isJoining} connected={isConnected}");

        // ── PING from host ────────────────────────────────────────────────────
        if (msg == RoomProtocol.PING)
        {
            pingReceived = true;
            Debug.Log("[CLIENT] PING received from host");
            return;
        }

        // ── Discovery response ────────────────────────────────────────────────
        if (msg.StartsWith(RoomProtocol.HOST + "|"))
        {
            if (!isDiscovering)
            {
                Debug.Log("[CLIENT] Got HOST response but not discovering -- ignoring.");
                return;
            }

            string[] parts = msg.Split('|');
            if (parts.Length < 2) return;

            string discoveredCode = parts[1];

            if (discoveredCode != pendingRoomCode)
            {
                Debug.Log($"[CLIENT] Room code mismatch: got '{discoveredCode}', want '{pendingRoomCode}'");
                return;
            }

            Debug.Log($"[CLIENT] Discovered host at {remote} for room '{discoveredCode}'");
            isDiscovering = false;

            UnityMainThreadDispatcher.ExecuteOnMainThread(() => JoinRoom(pendingRoomCode, remote));
            return;
        }

        // ── Accepted ──────────────────────────────────────────────────────────
        if (msg == RoomProtocol.ACCEPT)
        {
            if (isConnected) return;

            isConnected = true;
            isJoining = false;
            isDiscovering = false;

            Debug.Log("[CLIENT] Connection accepted by host!");

            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                // CRITICAL FIX: Initialize heartbeat timers
                float currentTime = Time.time;
                connectedAt = currentTime;
                lastPingFromHost = currentTime;  // Start with current time
                nextPingTime = currentTime + PING_INTERVAL;  // Schedule first ping

                Debug.Log($"[CLIENT] Heartbeat initialized: connectedAt={connectedAt}, lastPingFromHost={lastPingFromHost}, nextPingTime={nextPingTime}");

                OnConnected?.Invoke();
            });

            return;
        }

        // ── GO_CHARSELECT ─────────────────────────────────────────────────────
        if (msg == RoomProtocol.GO_CHARSELECT)
        {
            Debug.Log("[CLIENT] Host sent GO_CHARSELECT.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
            return;
        }

        // ── Room full ─────────────────────────────────────────────────────────
        if (msg == RoomProtocol.FULL)
        {
            isJoining = false;
            Debug.Log("[CLIENT] Room is full.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                OnConnectionFailed?.Invoke("Room is full (2/2 players)"));
            return;
        }

        // ── Wrong code ────────────────────────────────────────────────────────
        if (msg == "WRONG_CODE")
        {
            isJoining = false;
            Debug.Log("[CLIENT] Wrong room code.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                OnConnectionFailed?.Invoke("Invalid room code"));
            return;
        }

        // ── Same device ───────────────────────────────────────────────────────
        if (msg == "SAME_DEVICE")
        {
            isJoining = false;
            Debug.Log("[CLIENT] Cannot connect to yourself.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                OnConnectionFailed?.Invoke("Cannot connect to yourself -- use a different device"));
            return;
        }

        // ── ROLE_ASSIGN ───────────────────────────────────────────────────────
        if (msg.StartsWith(RoomProtocol.ROLE_ASSIGN + "|"))
        {
            Debug.Log($"[CLIENT] Role assignment received: {msg}");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
            return;
        }

        // ── START_GAME ────────────────────────────────────────────────────────
        if (msg == RoomProtocol.START_GAME)
        {
            Debug.Log("[CLIENT] Start game signal received.");
            UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
            return;
        }

        // ── Forward unknown ───────────────────────────────────────────────────
        Debug.LogWarning($"[CLIENT] Unknown message: '{msg}'");
        UnityMainThreadDispatcher.ExecuteOnMainThread(() => OnMessageReceived?.Invoke(msg));
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    public void SendToHost(string message)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[CLIENT] SendToHost called but not connected.");
            return;
        }

        if (transport == null || hostEndpoint == null)
        {
            Debug.LogWarning("[CLIENT] SendToHost called but transport/endpoint is null.");
            return;
        }

        transport.Send(message, hostEndpoint);
        Debug.Log($"[CLIENT] Sent to host: {message}");
    }

    public void Dispose()
    {
        isDiscovering = false;
        isJoining = false;
        Debug.Log("[CLIENT] Disposing...");
        transport?.Dispose();
        transport = null;
    }
}