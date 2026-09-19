public static class RoomProtocol
{
    public const string DISCOVER = "DISCOVER";
    public const string HOST = "HOST";
    public const string JOIN = "JOIN";
    public const string ACCEPT = "ACCEPT";
    public const string FULL = "FULL";
    public const string PING = "PING";

    // ── Lobby → Game ──────────────────────────────────────────────────────────
    public const string GO_CHARSELECT = "GO_CHARSELECT"; // Legacy: go to char select (unused now)
    public const string START_GAME = "START_GAME";      // Host → Client: load game scene directly

    // ── Character Selection (kept for backward compatibility) ─────────────────
    public const string ROLE_ASSIGN = "ROLE_ASSIGN";   // Host → Client: "ROLE_ASSIGN|0"
    public const string ROLE_ACK = "ROLE_ACK";         // Client → Host: acknowledged

    // ── Game Sync ─────────────────────────────────────────────────────────────
    // Prepping actions (Prep player → Host → Cook player)
    public const string PREP_SPAWN = "PREP_SPAWN";         // "PREP_SPAWN|id|type|posX|posY|snapIndex"
    public const string PREP_MOVE = "PREP_MOVE";           // "PREP_MOVE|id|posX|posY|snapIndex"
    public const string PREP_WASH_COMPLETE = "PREP_WASH";  // "PREP_WASH|id|originalTag|newTag"
    public const string PREP_CHOP_COMPLETE = "PREP_CHOP";  // "PREP_CHOP|id|originalTag|newTag"
    public const string PREP_DELETE = "PREP_DELETE";       // "PREP_DELETE|id"

    // Cooking actions (Cook player → Host → Prep player)
    public const string COOK_CUSTOMER_SPAWN = "COOK_CUST_SPAWN"; // "COOK_CUST_SPAWN|id|dish|slotIndex"
    public const string COOK_TAKE_ORDER = "COOK_TAKE_ORDER";     // "COOK_TAKE_ORDER|custId"
    public const string COOK_ADD_TO_POT = "COOK_ADD_POT";       // "COOK_ADD_POT|ingredientId|potIndex"
    public const string COOK_SERVE_DISH = "COOK_SERVE";         // "COOK_SERVE|dishName|custId"

    // Shared state (Host → Both players)
    public const string SCORE_UPDATE = "SCORE";             // "SCORE|hostScore|clientScore"
    public const string GAME_RESET = "GAME_RESET";         // "GAME_RESET"

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

    /// <summary>Check if a message is a gameplay sync message (not lobby/room).</summary>
    public static bool IsGameMessage(string msg)
    {
        return msg.StartsWith("PREP_") || msg.StartsWith("COOK_") ||
               msg.StartsWith("SCORE") || msg.StartsWith("GAME_");
    }
}