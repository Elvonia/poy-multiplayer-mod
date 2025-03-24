using Steamworks;
using System.Collections.Generic;

namespace Multiplayer.Steam
{
    public static class LobbyManager
    {
        public static CSteamID LobbyID { get; set; }
        public static ELobbyType LobbyType { get; private set; } = ELobbyType.k_ELobbyTypeFriendsOnly;
        public static int MaxPlayers { get; private set; } = 4;

        public static CSteamID PlayerID => SteamUser.GetSteamID();
        public static string PlayerName => SteamFriends.GetPersonaName();
        public static bool IsInLobby => LobbyID != CSteamID.Nil;

        public static List<PlayerClone> RemotePlayers { get; private set; }

        private static Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
        private static Callback<LobbyChatUpdate_t> _lobbyChatUpdated;
        private static Callback<LobbyCreated_t> _lobbyCreated;
        private static Callback<LobbyEnter_t> _lobbyEntered;
        private static Callback<P2PSessionRequest_t> _p2pSessionRequest;

        public static void Initialize()
        {
            _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(Callbacks.OnFriendJoined);
            _lobbyChatUpdated = Callback<LobbyChatUpdate_t>.Create(Callbacks.OnLobbyChatUpdated);
            _lobbyCreated = Callback<LobbyCreated_t>.Create(Callbacks.OnLobbyCreated);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(Callbacks.OnLobbyJoined);
            _p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(Callbacks.OnP2PSessionRequest);

            RemotePlayers = new List<PlayerClone>();
        }

        public static void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(LobbyType, MaxPlayers);
        }

        public static void LeaveLobby()
        {
            if (!IsInLobby)
                return;

            SteamMatchmaking.LeaveLobby(LobbyID);
            LobbyID = CSteamID.Nil;
        }

        public static void RestartLobby()
        {
            if (!IsInLobby)
                return;

            SteamMatchmaking.LeaveLobby(LobbyID);
            LobbyID = CSteamID.Nil;

            SteamMatchmaking.CreateLobby(LobbyType, MaxPlayers);
        }

        public static void UpdateLobby(ELobbyType lobbyType, int maxPlayers)
        {
            LobbyType = lobbyType;
            MaxPlayers = maxPlayers;

            SteamMatchmaking.SetLobbyType(LobbyID, LobbyType);
            SteamMatchmaking.SetLobbyMemberLimit(LobbyID, MaxPlayers);
        }
    }
}
