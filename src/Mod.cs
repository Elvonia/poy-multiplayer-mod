using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if BEPINEX
using BepInEx;

namespace CoopMod
{
    [BepInPlugin("com.github.Elvonia.poy-coop-mod", "Coop Mod Test", PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public void Awake() {
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

[assembly: MelonInfo(typeof(CoopMod.Mod), "Coop Mod Test", PluginInfo.PLUGIN_VERSION, "Kalico")]
[assembly: MelonGame("TraipseWare", "Peaks of Yore")]

namespace CoopMod
{
    public class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            CommonAwake();
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
        private CSteamID currentLobbyID;

        private GameObject uiCanvas;
        private Text uiText;

        private Player player;
        private GameObject playerShadow;

        private List<Shadow> remotePlayers = new List<Shadow>();

        public void CommonAwake()
        {
            if (!SteamAPI.Init())
            {
                return;
            }

            StealPlayerShadow();

            Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            Callback<LobbyEnter_t>.Create(OnLobbyJoined);
            Callback<GameLobbyJoinRequested_t>.Create(OnFriendJoined);
            Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdated);

            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 2);
        }

        public void CommonSceneLoad(int buildIndex)
        {
            SetupUI();
            UpdateLobbyUI();

            if (buildIndex == 0 || buildIndex == 1
                || buildIndex == 37 || buildIndex == 67)
            {
                return;
            }

            player = new Player();
        }

        public void CommonSceneUnload()
        {
            player = null;
            remotePlayers.Clear();
            uiCanvas = null;
            uiText = null;
        }

        public void CommonUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OpenSteamFriendsList();
            }

            if(Input.GetKeyDown(KeyCode.F2))
            {
                if (uiCanvas.activeSelf)
                {
                    uiCanvas.SetActive(false);
                }
                else
                {
                    uiCanvas.SetActive(true);
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

        #region UI
        private void SetupUI()
        {
            if (uiCanvas != null) return;

            uiCanvas = new GameObject("CoopModUI");
            Canvas canvas = uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 420;

            GameObject textObject = new GameObject("LobbyText");
            textObject.transform.SetParent(uiCanvas.transform);

            uiText = textObject.AddComponent<Text>();
            uiText.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == "roman-antique.regular");
            uiText.fontSize = 24;
            uiText.alignment = TextAnchor.UpperLeft;
            uiText.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            uiText.rectTransform.anchoredPosition = new Vector2(40, -40);
            uiText.lineSpacing = 3f;

            UpdateUI("Initializing...");
        }

        private void UpdateUI(string message)
        {
            if (uiText != null)
            {
                uiText.text = message;
            }
        }

        private void UpdateLobbyUI()
        {
            if (!currentLobbyID.IsValid()) return;

            int playerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);
            string playerList = $"Lobby ID: {currentLobbyID}\nPlayers: {playerCount}\n";

            for (int i = 0; i < playerCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyID, i);
                string playerName = SteamFriends.GetFriendPersonaName(playerID);
                playerList += $"{playerName}\n";
            }

            UpdateUI(playerList);
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
        #endregion

        #region Lobby Callbacks
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                UpdateLobbyUI();
            }
        }

        private void OnLobbyJoined(LobbyEnter_t callback)
        {
            currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            UpdateLobbyUI();
        }

        private void OnFriendJoined(GameLobbyJoinRequested_t callback)
        {
            CSteamID friendID = callback.m_steamIDFriend;
            currentLobbyID = callback.m_steamIDLobby;
            SteamMatchmaking.JoinLobby(currentLobbyID);
        }

        private void OnLobbyChatUpdated(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby == (ulong)currentLobbyID)
            {
                UpdateLobbyUI();
            }
        }
        #endregion

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
                    Shadow shadow = remotePlayers.Find(s => s.GetSteamID() == senderID);

                    if (shadow == null)
                    {
                        shadow = new Shadow(senderID, playerShadow);
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