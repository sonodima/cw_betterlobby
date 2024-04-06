
using HarmonyLib;

using Photon.Pun;
using Steamworks;

namespace BetterLobby;


internal static class LobbyHelpers
{
    private static readonly int s_dNumPlayers = 1;
    private static readonly int s_dMaxPlayers = 4;

    internal static bool IsMasterClient
        => MainMenuHandler.SteamLobbyHandler?.MasterClient ?? false;

    /// <summary>
    /// Gets the current number of active players in the lobby, if available.
    /// </summary>
    internal static int? NumPlayers => PlayerHandler.instance?.players.Count;

    internal static bool IsFull => (NumPlayers ?? s_dNumPlayers)
        >= (MaxPlayers ?? s_dMaxPlayers);

    /// <summary>
    /// Gets the maximum number of players that can be in the lobby, if available.
    /// </summary>
    internal static int? MaxPlayers =>
        Traverse.Create(MainMenuHandler.SteamLobbyHandler)
            .Field("m_MaxPlayers")
            .GetValue<int>();

    /// <summary>
    /// Gets the Steam identifier of the current lobby, if available. 
    /// </summary>
    internal static CSteamID? CurrentID =>
        Traverse.Create(MainMenuHandler.SteamLobbyHandler)
            .Field("m_CurrentLobby")
            .GetValue<CSteamID>();

    /// <summary>
    /// Sets whether the current Steam lobby should be public or friends only.
    /// </summary>
    internal static bool SetPublic(bool value)
    {
        if (!RunChecks(out CSteamID? id) || !id.HasValue)
        {
            return false;
        }

        Plugin.CurLogger?.LogInfo("Changing Steam lobby type to "
            + (value ? "PUBLIC" : "FRIENDS ONLY"));

        SteamMatchmaking.SetLobbyType(id.Value, value
            ? ELobbyType.k_ELobbyTypePublic
            : ELobbyType.k_ELobbyTypeFriendsOnly);
        return true;
    }

    internal static bool SetJoinable(bool value)
    {
        if (!RunChecks(out CSteamID? id) || !id.HasValue)
        {
            return false;
        }

        Plugin.CurLogger?.LogInfo("Changing the current lobby to "
            + (value ? "JOINABLE" : "NOT JOINABLE"));

        SteamMatchmaking.SetLobbyJoinable(id.Value, value);
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = value;
            PhotonNetwork.CurrentRoom.IsVisible = value;
        }

        return true;
    }

    private static bool RunChecks(out CSteamID? id)
    {
        id = CurrentID;
        if (!id.HasValue)
        {
            Plugin.CurLogger?.LogWarning("Failed to get the current lobby's CSteamID value.");
            return false;
        }

        if (!IsMasterClient)
        {
            Plugin.CurLogger?.LogWarning("You can't perform this operation because you are not the master client!");
            return false;
        }

        return true;
    }
}