using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Multiplayer.UI;
using Multiplayer.Steam;
using Multiplayer.Logger;

#if BEPINEX
using BepInEx;

namespace Multiplayer
{
    [BepInPlugin("com.github.Elvonia.poy-multiplayer-mod", "Multiplayer Mod", PluginInfo.PLUGIN_VERSION)]
    public class MultiplayerMod : BaseUnityPlugin 
    {
        public void Awake() 
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        
            CommonAwake();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CommonSceneLoad(scene.buildIndex);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            CommonSceneUnload();
        }

        public void FixedUpdate()
        {
            CommonFixedUpdate();
        }

        public void Update()
        {
            CommonUpdate();
        }

        public void OnDestroy() 
        {
            CommonDestroy();
        }

#elif MELONLOADER
using MelonLoader;

[assembly: MelonInfo(typeof(Multiplayer.MultiplayerMod), "Multiplayer Mod", PluginInfo.PLUGIN_VERSION, "Kalico")]
[assembly: MelonGame("TraipseWare", "Peaks of Yore")]

namespace Multiplayer
{
    public class MultiplayerMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            CommonAwake();
        }

        public override void OnDeinitializeMelon()
        {
            if (SteamNetworking.IsP2PPacketAvailable(out _))
            {
                SteamNetworking.CloseP2PSessionWithUser(currentLobbyID);
            }

            if (currentLobbyID.IsValid())
            {
                SteamMatchmaking.LeaveLobby(currentLobbyID);
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            CommonSceneLoad(buildIndex);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            CommonSceneUnload();
        }

        public override void OnUpdate()
        {
            CommonUpdate();
        }

        public override void OnFixedUpdate()
        {
            CommonFixedUpdate();
        }
#endif

        public CSteamID currentLobbyID;

        public Player player;
        public GameObject shadowClone;

        public MultiplayerUI lobbyUI;
        public PacketManager packetManager;

        public List<PlayerClone> remotePlayers = new List<PlayerClone>();

        public void CommonAwake()
        {
            if (!SteamAPI.Init())
            {
                return;
            }

            ShadowClone.StealPlayerShadow(this);

            Callback<GameLobbyJoinRequested_t>.Create((callback) => Callbacks.OnFriendJoined(callback, this));
            Callback<LobbyChatUpdate_t>.Create((callback) => Callbacks.OnLobbyChatUpdated(callback, this));
            Callback<LobbyCreated_t>.Create((callback) => Callbacks.OnLobbyCreated(callback, this));
            Callback<LobbyEnter_t>.Create((callback) => Callbacks.OnLobbyJoined(callback, this));
            Callback<P2PSessionRequest_t>.Create((callback) => Callbacks.OnP2PSessionRequest(callback));

            // Move to UI to select lobby type + player count
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);

            packetManager = new PacketManager();
        }

        public void CommonDestroy()
        {
            // Close P2P + Lobby to prevent application hang
            foreach (PlayerClone player in remotePlayers)
            {
                SteamNetworking.CloseP2PSessionWithUser(player.GetSteamID());
            }

            if (currentLobbyID.IsValid())
            {
                SteamMatchmaking.LeaveLobby(currentLobbyID);
            }

            SteamAPI.Shutdown();
        }

        public void CommonSceneLoad(int buildIndex)
        {
            // Skip cabins as they lack a PlayerShadow on the Player
            if (buildIndex == 0 || buildIndex == 1 
                || buildIndex == 37 || buildIndex == 67)
            {
                lobbyUI = new MultiplayerUI(this, buildIndex);
                lobbyUI.UpdateUI(currentLobbyID);

                byte[] nullIndexBytes = packetManager.CreateNullSceneUpdatePacket();
                packetManager.SendReliablePacket(currentLobbyID, nullIndexBytes);

                LogManager.Debug("Broadcasting null scene update packet");

                return;
            }

            player = new Player();
            player.SetScene(buildIndex);
            lobbyUI = new MultiplayerUI(this, buildIndex);

            foreach (PlayerClone playerClone in remotePlayers)
            {
                LogManager.Debug($"Checking remote player {playerClone.GetSteamID()} scene index");

                if (buildIndex == playerClone.GetSceneIndex())
                {
                    playerClone.CreatePlayerGameObject(shadowClone);
                    LogManager.Debug($"Rebuilding player object for user {playerClone.GetSteamID()}");
                }
            }

            byte[] sceneUpdatePacket = packetManager.CreateSceneUpdatePacket(player);
            packetManager.SendReliablePacket(currentLobbyID, sceneUpdatePacket);

            LogManager.Debug("Broadcasting scene update packet");
        }

        public void CommonSceneUnload()
        {
            foreach (PlayerClone player in remotePlayers)
            {
                //SteamNetworking.CloseP2PSessionWithUser(player.GetSteamID());
                player.DestroyPlayerGameObject();
                LogManager.Debug($"Destroyed {remotePlayers.Count} PlayerClones");
            }

            lobbyUI = null;
            player = null;
        }

        public void CommonUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                UI.Steam.OpenSteamFriendsList(this);
            }

            if(Input.GetKeyDown(KeyCode.F2))
            {
                if (lobbyUI.active)
                {
                    lobbyUI.DisableUI();
                }
                else
                {
                    lobbyUI.EnableUI();
                }
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (player == null)
                    return;

                Color randomColor = new Color(Random.value, Random.value, Random.value);
                player.SetColor(randomColor);

                byte[] colorPacket = packetManager.CreateColorUpdatePacket(player);
                packetManager.SendReliablePacket(currentLobbyID, colorPacket);

                LogManager.Debug("Setting random player color - " +
                                    $"R:{randomColor.r} " +
                                    $"G:{randomColor.g} " +
                                    $"B:{randomColor.b} " +
                                    $"A:{randomColor.a}");
            }
        }

        public void CommonFixedUpdate()
        {
            if (remotePlayers.Count > 0)
            {
                packetManager.ReceivePackets(this);

                if (player != null)
                {
                    player.UpdatePlayer();

                    byte[] positionPacket = packetManager.CreatePositionUpdatePacket(player);
                    packetManager.SendUnreliableNoDelayPacket(currentLobbyID, positionPacket);
                }
            }
        }
    }
}