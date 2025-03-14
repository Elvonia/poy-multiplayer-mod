using Multiplayer.Logger;
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
                CSteamID friendID = (CSteamID)callback.m_ulSteamIDUserChanged;

                PlayerClone playerClone = new PlayerClone(friendID, instance.playerShadow);
                playerClone.DestroyPlayerGameObject();

                instance.remotePlayers.Add(playerClone);
                LogManager.Debug($"Added {friendID} to remotePlayers");

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

            int memberCount = SteamMatchmaking.GetNumLobbyMembers(instance.currentLobbyID);
            LogManager.Debug($"{memberCount} existing players detected");

            for (int i = 0; i < memberCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(instance.currentLobbyID, i);

                if (playerID != SteamUser.GetSteamID()) // Don't create a clone for yourself
                {
                    if (!instance.remotePlayers.Exists(p => p.GetSteamID() == playerID))
                    {
                        PlayerClone playerClone = new PlayerClone(playerID, instance.playerShadow);
                        instance.remotePlayers.Add(playerClone);
                        LogManager.Debug($"Added {playerID} to remotePlayers");
                    }
                }
            }
        }
    }
}
