using Multiplayer.Logger;
using Steamworks;

namespace Multiplayer.Steam
{
    public static class Callbacks
    {
        // move remote player logic out of callbacks

        public static void OnFriendJoined(GameLobbyJoinRequested_t callback)
        {
            LobbyManager.LobbyID = callback.m_steamIDLobby;
            SteamMatchmaking.JoinLobby(LobbyManager.LobbyID);
        }

        public static void OnLobbyChatUpdated(LobbyChatUpdate_t callback)
        {
            CSteamID friendID = (CSteamID)callback.m_ulSteamIDUserChanged;
            //instance.lobbyUI.UpdateUI(instance.currentLobbyID);

            EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            switch (change)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:

                    // add player to remotePlayers list
                    PlayerClone playerClone = new PlayerClone(friendID, ShadowClone.ShadowObject);
                    playerClone.DestroyPlayerGameObject();

                    LobbyManager.RemotePlayers.Add(playerClone);
                    LogManager.Debug($"Added {friendID} to remotePlayers");

                    // request scene index
                    PacketManager.RequestSceneUpdate(friendID);

                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:

                    // remove player from remotePlayers list
                    foreach(PlayerClone player in LobbyManager.RemotePlayers)
                    {
                        if (player.GetSteamID() == friendID)
                        {
                            player.DestroyPlayerGameObject();
                            LobbyManager.RemotePlayers.Remove(player);

                            LogManager.Debug($"Removed {friendID} from remotePlayers");
                            break;
                        }
                    }

                    break;
            }
        }

        public static void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                LobbyManager.LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                //instance.lobbyUI.UpdateUI(instance.currentLobbyID);
            }
        }

        public static void OnLobbyJoined(LobbyEnter_t callback)
        {
            LobbyManager.LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            //instance.lobbyUI.UpdateUI(instance.currentLobbyID);

            int memberCount = SteamMatchmaking.GetNumLobbyMembers(LobbyManager.LobbyID);
            LogManager.Debug($"{memberCount} existing players detected");

            for (int i = 0; i < memberCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyManager.LobbyID, i);

                if (playerID != SteamUser.GetSteamID()) // dont clone yourself
                {
                    if (!LobbyManager.RemotePlayers.Exists(p => p.GetSteamID() == playerID))
                    {
                        PlayerClone playerClone = new PlayerClone(playerID, ShadowClone.ShadowObject);

                        LobbyManager.RemotePlayers.Add(playerClone);
                        LogManager.Debug($"Added {playerID} to remotePlayers");

                        // request scene index
                        PacketManager.RequestSceneUpdate(playerID);
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
