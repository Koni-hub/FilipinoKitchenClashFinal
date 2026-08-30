using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpTransport : IDisposable
{
    private UdpClient udp;
    private readonly object disposeLock = new object();
    private bool isDisposed = false;

    public Action<byte[], IPEndPoint> OnDataReceived;

    public UdpTransport(bool enableBroadcast = false)
    {
        udp = new UdpClient();
        udp.EnableBroadcast = enableBroadcast;

        // Allow multiple sockets on the same port (needed when host + client
        // run on the same machine during testing)
        udp.Client.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReuseAddress, true);

        BeginReceive();
    }

    public UdpTransport(int port, bool enableBroadcast = false)
    {
        try
        {
            udp = new UdpClient(port);
            udp.EnableBroadcast = enableBroadcast;

            udp.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress, true);

            BeginReceive();
            Debug.Log($"[UDP] Listening on port {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP] Failed to bind port {port}: {e.Message}");
            throw;
        }
    }

    // ── Send ─────────────────────────────────────────────────────────────────

    public void Send(string msg, IPEndPoint target)
    {
        lock (disposeLock)
        {
            if (udp == null || isDisposed)
            {
                Debug.LogWarning("[UDP] Send skipped: already disposed.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                udp.Send(data, data.Length, target);
                Debug.Log($"[UDP] Sent '{msg}' → {target}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] Send error: {e.Message}");
            }
        }
    }

    public void SendToAll(string msg, IPEndPoint[] targets)
    {
        foreach (var target in targets)
            Send(msg, target);
    }

    public void Broadcast(string msg, int port)
    {
        lock (disposeLock)
        {
            if (udp == null || isDisposed) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
                udp.Send(data, data.Length, broadcastEndpoint);
                Debug.Log($"[UDP] Broadcast '{msg}' on port {port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] Broadcast error: {e.Message}");
            }
        }
    }

    // ── Receive loop ──────────────────────────────────────────────────────────

    private void BeginReceive()
    {
        lock (disposeLock)
        {
            if (udp == null || isDisposed) return;

            try
            {
                udp.BeginReceive(OnReceive, null);
            }
            catch (ObjectDisposedException)
            {
                // Socket was closed between the check and the call — safe to ignore
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] BeginReceive error: {e.Message}");
            }
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        // Grab data before re-checking disposed state
        byte[] data = null;
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

        lock (disposeLock)
        {
            if (udp == null || isDisposed) return;

            try
            {
                data = udp.EndReceive(ar, ref remote);
            }
            catch (ObjectDisposedException)
            {
                return; // Transport disposed while waiting — normal shutdown path
            }
            catch (SocketException e)
            {
                // 10054 = WSAECONNRESET (Windows-only, harmless on UDP)
                if (e.ErrorCode != 10054)
                    Debug.LogError($"[UDP] SocketException in receive: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] Receive error: {e.Message}");
            }
        }

        // Fire the callback OUTSIDE the lock so handlers can call Send() freely
        // without deadlocking (Send also takes the lock)
        if (data != null && data.Length > 0)
        {
            try
            {
                OnDataReceived?.Invoke(data, remote);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] OnDataReceived handler threw: {e.Message}");
            }
        }

        // Re-arm the receive loop
        BeginReceive();
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    public void Dispose()
    {
        lock (disposeLock)
        {
            if (isDisposed) return;
            isDisposed = true;

            try
            {
                udp?.Close();
                udp?.Dispose();
            }
            catch { /* already closed */ }
            finally
            {
                udp = null;
            }

            Debug.Log("[UDP] Disposed.");
        }
    }
}