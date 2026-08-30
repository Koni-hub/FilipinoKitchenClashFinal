using UnityEngine;

public static class RoomData
{
    public enum PlayerRoleEnum
    {
        Prep = 0,
        Cook = 1
    }

    private static string _roomCode;
    private static string _playerRole;
    private static PlayerRoleEnum _gamePlayerRole;

    public static string RoomCode
    {
        get => _roomCode;
        set => _roomCode = value;
    }

    public static string PlayerRole
    {
        get => _playerRole;
        set => _playerRole = value;
    }

    public static PlayerRoleEnum GamePlayerRole
    {
        get => _gamePlayerRole;
        set => _gamePlayerRole = value;
    }

    // Optional: Helper method to reset all data
    public static void Reset()
    {
        _roomCode = null;
        _playerRole = null;
        _gamePlayerRole = PlayerRoleEnum.Prep;
    }
}