using Steamworks;

namespace Multiplayer.Steam
{
    public class Callbacks
    {
        public static void OnFriendJoined(GameLobbyJoinRequested_t callback, MultiplayerMod instance)
        {
            instance.currentLobbyID = callback.m_steamIDLobby;
            SteamMatchmaking.JoinLobby(instance.currentLobbyID);
        }

        public static void OnLobbyChatUpdated(LobbyChatUpdate_t callback, MultiplayerMod instance)
        {
            if (callback.m_ulSteamIDLobby == (ulong)instance.currentLobbyID)
            {
                instance.debugUI.UpdateLobbyUI(instance.currentLobbyID);
            }
        }

        public static void OnLobbyCreated(LobbyCreated_t callback, MultiplayerMod instance)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                instance.currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                instance.debugUI.UpdateLobbyUI(instance.currentLobbyID);
            }
        }

        public static void OnLobbyJoined(LobbyEnter_t callback, MultiplayerMod instance)
        {
            instance.currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            instance.debugUI.UpdateLobbyUI(instance.currentLobbyID);
        }
    }
}
