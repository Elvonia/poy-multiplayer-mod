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
            CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            CSteamID friendID = (CSteamID)callback.m_ulSteamIDUserChanged;
            CSteamID friendMakingChangeID = new CSteamID(callback.m_ulSteamIDMakingChange);

            // add new player

            PlayerClone playerClone = new PlayerClone(friendID, instance.playerShadow);
            playerClone.DestroyPlayerGameObject();

            instance.remotePlayers.Add(playerClone);
            LogManager.Debug($"Added {friendID} to remotePlayers");

            instance.debugUI.UpdateLobbyUI(instance.currentLobbyID);

            // remove player who left/dced

            EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            switch (change)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:

                    foreach(PlayerClone player in instance.remotePlayers)
                    {
                        if (player.GetSteamID() == friendID)
                        {
                            player.DestroyPlayerGameObject();
                            instance.remotePlayers.Remove(player);

                            LogManager.Debug($"Removed {friendID} from remotePlayers");
                            break;
                        }
                    }

                    break;
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

        public static void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            CSteamID friendID = request.m_steamIDRemote;
            SteamNetworking.AcceptP2PSessionWithUser(friendID);
        }
    }
}
