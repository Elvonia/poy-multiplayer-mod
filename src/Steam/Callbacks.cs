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
            CSteamID friendID = (CSteamID)callback.m_ulSteamIDUserChanged;
            instance.lobbyUI.UpdateUI(instance.currentLobbyID);

            EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            switch (change)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:

                    // add player to remotePlayers list
                    PlayerClone playerClone = new PlayerClone(friendID, instance.shadowClone);
                    playerClone.DestroyPlayerGameObject();

                    instance.remotePlayers.Add(playerClone);
                    LogManager.Debug($"Added {friendID} to remotePlayers");

                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:

                    // remove player from remotePlayers list
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
                instance.lobbyUI.UpdateUI(instance.currentLobbyID);
            }
        }

        public static void OnLobbyJoined(LobbyEnter_t callback, MultiplayerMod instance)
        {
            instance.currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            instance.lobbyUI.UpdateUI(instance.currentLobbyID);

            int memberCount = SteamMatchmaking.GetNumLobbyMembers(instance.currentLobbyID);
            LogManager.Debug($"{memberCount} existing players detected");

            for (int i = 0; i < memberCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(instance.currentLobbyID, i);

                if (playerID != SteamUser.GetSteamID()) // dont clone yourself
                {
                    if (!instance.remotePlayers.Exists(p => p.GetSteamID() == playerID))
                    {
                        PlayerClone playerClone = new PlayerClone(playerID, instance.shadowClone);
                        instance.remotePlayers.Add(playerClone);
                        LogManager.Debug($"Added {playerID} to remotePlayers");
                    }
                }
            }
        }

        public static void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            // forces p2p connection without sending a packet back
            CSteamID friendID = request.m_steamIDRemote;
            SteamNetworking.AcceptP2PSessionWithUser(friendID);
        }
    }
}
