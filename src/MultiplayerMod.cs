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
                SteamNetworking.CloseP2PSessionWithUser(LobbyManager.LobbyID);
            }

            if (LobbyManager.LobbyID.IsValid())
            {
                SteamMatchmaking.LeaveLobby(LobbyManager.LobbyID);
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

        public Player player;
        public MultiplayerUI lobbyUI;

        public void CommonAwake()
        {
            if (!SteamAPI.Init())
            {
                return;
            }

            LobbyManager.Initialize();
            LobbyManager.CreateLobby();

            ShadowClone.Initialize();
        }

        public void CommonDestroy()
        {
            // Close P2P + Lobby to prevent application hang
            foreach (PlayerClone player in LobbyManager.RemotePlayers)
            {
                SteamNetworking.CloseP2PSessionWithUser(player.GetSteamID());
            }

            LobbyManager.LeaveLobby();
            SteamAPI.Shutdown();
        }

        public void CommonSceneLoad(int buildIndex)
        {
            // Skip cabins as they lack a PlayerShadow on the Player
            if (buildIndex == 0 || buildIndex == 1 
                || buildIndex == 37 || buildIndex == 67)
            {
                lobbyUI = new MultiplayerUI(this, buildIndex);
                lobbyUI.UpdateUI();

                byte[] nullIndexBytes = PacketManager.CreateNullSceneUpdatePacket();
                PacketManager.SendReliablePacket(nullIndexBytes);

                LogManager.Debug("Broadcasting null scene update packet");

                return;
            }

            player = new Player();
            player.SetScene(buildIndex);
            lobbyUI = new MultiplayerUI(this, buildIndex);

            foreach (PlayerClone playerClone in LobbyManager.RemotePlayers)
            {
                LogManager.Debug($"Checking remote player {playerClone.GetSteamID()} scene index");

                if (buildIndex == playerClone.GetSceneIndex())
                {
                    playerClone.CreatePlayerGameObject(ShadowClone.ShadowObject);
                    LogManager.Debug($"Rebuilding player object for user {playerClone.GetSteamID()}");
                }
            }

            byte[] sceneUpdatePacket = PacketManager.CreateSceneUpdatePacket(player);
            PacketManager.SendReliablePacket(sceneUpdatePacket);

            LogManager.Debug("Broadcasting scene update packet");
        }

        public void CommonSceneUnload()
        {
            foreach (PlayerClone player in LobbyManager.RemotePlayers)
            {
                SteamNetworking.CloseP2PSessionWithUser(player.GetSteamID());
                player.DestroyPlayerGameObject();
                LogManager.Debug($"Destroyed {LobbyManager.RemotePlayers.Count} PlayerClones");
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

                byte[] colorPacket = PacketManager.CreateColorUpdatePacket(player);
                PacketManager.SendReliablePacket(colorPacket);

                LogManager.Debug("Setting random player color - " +
                                    $"R:{randomColor.r} " +
                                    $"G:{randomColor.g} " +
                                    $"B:{randomColor.b} " +
                                    $"A:{randomColor.a}");
            }
        }

        public void CommonFixedUpdate()
        {
            if (LobbyManager.RemotePlayers.Count > 0)
            {
                PacketManager.ReceivePackets(this);

                if (player != null)
                {
                    player.UpdatePlayer();

                    byte[] positionPacket = PacketManager.CreatePositionUpdatePacket(player);
                    PacketManager.SendUnreliableNoDelayPacket(positionPacket);
                }
            }
        }
    }
}