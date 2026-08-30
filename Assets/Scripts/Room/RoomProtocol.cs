public static class RoomProtocol
{
    public const string DISCOVER = "DISCOVER";
    public const string HOST = "HOST";
    public const string JOIN = "JOIN";
    public const string ACCEPT = "ACCEPT";
    public const string FULL = "FULL";
    public const string PING = "PING";

    // ── Lobby → Character Selection ───────────────────────────────────────────
    public const string GO_CHARSELECT = "GO_CHARSELECT"; // Host → Client: go to char select scene

    // ── Character Selection ───────────────────────────────────────────────────
    public const string ROLE_ASSIGN = "ROLE_ASSIGN";   // Host → Client: "ROLE_ASSIGN|0"
    public const string ROLE_ACK = "ROLE_ACK";      // Client → Host: acknowledged
    public const string START_GAME = "START_GAME";    // Host → Client: load game scene

    /// <summary>Builds "ROLE_ASSIGN|{roleIndex}"</summary>
    public static string BuildRoleAssign(RoomData.PlayerRoleEnum hostRole)
        => $"{ROLE_ASSIGN}|{(int)hostRole}";

    /// <summary>Parses role index out of "ROLE_ASSIGN|0". Returns -1 on failure.</summary>
    public static int ParseRoleIndex(string message)
    {
        var parts = message.Split('|');
        if (parts.Length == 2 && int.TryParse(parts[1], out int index))
            return index;
        return -1;
    }
}