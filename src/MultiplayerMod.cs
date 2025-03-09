using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MultiplayerMod.UI;


#if BEPINEX
using BepInEx;

namespace MultiplayerMod
{
    [BepInPlugin("com.github.Elvonia.poy-multiplayer-mod", "Multiplayer Mod Test", PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public void Awake() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        
            CommonAwake();
        }

        public void OnDestroy() 
        {
            CommonClose();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CommonSceneLoad(scene.buildIndex);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            CommonSceneUnload();
        }

        public void Update()
        {
            CommonUpdate();
        }

        public void FixedUpdate()
        {
            CommonFixedUpdate();
        }

#elif MELONLOADER
using MelonLoader;

[assembly: MelonInfo(typeof(MultiplayerMod.MultiplayerMod), "Multiplayer Mod Test", PluginInfo.PLUGIN_VERSION, "Kalico")]
[assembly: MelonGame("TraipseWare", "Peaks of Yore")]

namespace MultiplayerMod
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

        public MultiplayerDebugUI debugUI;

        private Player player;
        private GameObject playerShadow;

        private List<PlayerClone> remotePlayers = new List<PlayerClone>();

        public void CommonAwake()
        {
            if (!SteamAPI.Init())
            {
                return;
            }

            StealPlayerShadow();

            Callback<GameLobbyJoinRequested_t>.Create((callback) => Steam.Callbacks.OnFriendJoined(callback, this));
            Callback<LobbyChatUpdate_t>.Create((callback) => Steam.Callbacks.OnLobbyChatUpdated(callback, this));
            Callback<LobbyCreated_t>.Create((callback) => Steam.Callbacks.OnLobbyCreated(callback, this));
            Callback<LobbyEnter_t>.Create((callback) => Steam.Callbacks.OnLobbyJoined(callback, this));

            // Move to UI to select lobby type + player count
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }

        public void CommonDestroy()
        {
            // Close P2P + Lobby to prevent application hang
            if (SteamNetworking.IsP2PPacketAvailable(out _))
            {
                SteamNetworking.CloseP2PSessionWithUser(currentLobbyID);
            }

            if (currentLobbyID.IsValid())
            {
                SteamMatchmaking.LeaveLobby(currentLobbyID);
            }

            SteamAPI.Shutdown();
        }

        public void CommonSceneLoad(int buildIndex)
        {
            debugUI = new MultiplayerDebugUI();
            debugUI.UpdateLobbyUI(currentLobbyID);

            // Skip cabins as they lack a PlayerShadow on the Player
            if (buildIndex == 0 || buildIndex == 1 
                || buildIndex == 37 || buildIndex == 67)
            {
                return;
            }

            player = new Player();
        }

        public void CommonSceneUnload()
        {
            // Close P2P session to prevent application hang
            if (SteamNetworking.IsP2PPacketAvailable(out _))
            {
                SteamNetworking.CloseP2PSessionWithUser(currentLobbyID);
            }

            debugUI = null;
            player = null;
            remotePlayers.Clear();
        }

        public void CommonUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OpenSteamFriendsList();
            }

            if(Input.GetKeyDown(KeyCode.F2))
            {
                if (debugUI.active)
                {
                    debugUI.DisableUI();
                }
                else
                {
                    debugUI.EnableUI();
                }
            }
        }

        public void CommonFixedUpdate()
        {
            if (player != null)
            {
                player.UpdatePlayer();
                SendPlayerTransforms();
            }

            ReceivePlayerTransforms();
        }

        private void OpenSteamFriendsList()
        {
            if (currentLobbyID.IsValid())
            {
                SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
            }
            else
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
            }
        }

        #region PlayerShadow Duplication
        private void StealPlayerShadow()
        {
            SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive).completed += (operation) =>
            {
                PlayerShadow foundShadow = GameObject.FindObjectOfType<PlayerShadow>();

                if (foundShadow == null)
                {
                    UnloadStolenScene();
                    return;
                }

                playerShadow = GameObject.Instantiate(foundShadow.gameObject);
                playerShadow.name = "StolenPlayerShadow";

                GameObject.Destroy(playerShadow.GetComponent<PlayerShadow>());
                GameObject.DontDestroyOnLoad(playerShadow);

                playerShadow.SetActive(false);

                UnloadStolenScene();
            };
        }

        private void UnloadStolenScene()
        {
            SceneManager.UnloadSceneAsync(2).completed += (operation) =>
            {
                ReloadTitleMenu();
            };
        }

        private void ReloadTitleMenu()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        #endregion

        #region Send + Receive Player Transforms
        private void SendPlayerTransforms()
        {
            if (!currentLobbyID.IsValid()) return;

            byte[] data = player.GetPlayerDataBytes();
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            for (int i = 0; i < playerCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyID, i);
                if (playerID != SteamUser.GetSteamID())
                {
                    SteamNetworking.SendP2PPacket(playerID, data, (uint)data.Length, EP2PSend.k_EP2PSendUnreliable);
                }
            }
        }

        private void ReceivePlayerTransforms()
        {
            uint msgSize;
            while (SteamNetworking.IsP2PPacketAvailable(out msgSize))
            {
                byte[] buffer = new byte[msgSize];
                uint bytesRead;
                CSteamID senderID;

                if (SteamNetworking.ReadP2PPacket(buffer, msgSize, out bytesRead, out senderID))
                {
                    PlayerClone shadow = remotePlayers.Find(s => s.GetSteamID() == senderID);

                    if (shadow == null)
                    {
                        shadow = new PlayerClone(senderID, playerShadow);
                        remotePlayers.Add(shadow);
                    }

                    shadow.SetShadowDataFromBytes(buffer);
                    shadow.UpdateShadowTransforms();
                }
            }
        }
        #endregion
    }
}