using CoopMod;
using MelonLoader;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Shadow = CoopMod.Shadow;

[assembly: MelonInfo(typeof(Mod), "Coop Mod", PluginInfo.PLUGIN_VERSION, "Kalico")]
[assembly: MelonGame("TraipseWare", "Peaks of Yore")]

public class Mod : MelonMod
{
    private CSteamID currentLobbyID;

    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequestedCallback;
    private Callback<LobbyCreated_t> lobbyCreatedCallback;
    private Callback<LobbyEnter_t> lobbyEnterCallback;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;

    private GameObject uiCanvas;
    private Text uiText;

    private Player player;
    private GameObject playerShadow;
    private Transform localPlayerTransform;
    private List<Shadow> remotePlayers = new List<Shadow>();

    public override void OnInitializeMelon()
    {
        if (!SteamAPI.Init())
        {
            MelonLogger.Msg("Steam API failed to initialize!");
            return;
        }

        MelonLogger.Msg("Steam API initialized. Your Steam ID: " + SteamUser.GetSteamID());

        StealPlayerShadow();

        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyJoined);
        lobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnFriendJoined);
        lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdated);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 2);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        SetupUI();
        UpdateLobbyUI();

        player = new Player();

        /*if (currentLobbyID.IsValid())
        {
            SpawnPlayerShadows();
        }*/
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        player = null;

        uiCanvas = null;
        uiText = null;
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OpenSteamFriendsList();
        }

        if (localPlayerTransform == null)
        {
            FindLocalPlayer();
        }
        else
        {
            SendPlayerTransforms();
        }

        ReceivePlayerTransforms();
    }

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

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            UpdateLobbyUI();

            MelonLogger.Msg($"Lobby created! Lobby ID: {currentLobbyID}");
        }
        else
        {
            MelonLogger.Msg("Failed to create Steam lobby.");
        }
    }

    private void OnLobbyJoined(LobbyEnter_t callback)
    {
        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        UpdateLobbyUI();

        MelonLogger.Msg($"Joined lobby {currentLobbyID}");
    }

    private void OnFriendJoined(GameLobbyJoinRequested_t callback)
    {
        CSteamID friendID = callback.m_steamIDFriend;
        currentLobbyID = callback.m_steamIDLobby;
        SteamMatchmaking.JoinLobby(currentLobbyID);

        MelonLogger.Msg($"Friend {friendID} accepted the invite. Joining lobby {currentLobbyID}");
    }

    private void OnLobbyChatUpdated(LobbyChatUpdate_t callback)
    {
        if (callback.m_ulSteamIDLobby == (ulong)currentLobbyID)
        {
            UpdateLobbyUI();
        }
    }

    private void OpenSteamFriendsList()
    {
        if (currentLobbyID.IsValid())
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
        }
        else
        {
            MelonLogger.Msg("No active lobby found. Creating one...");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
        }
    }

    private void StealPlayerShadow()
    {
        SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive).completed += (operation) =>
        {
            PlayerShadow foundShadow = GameObject.FindObjectOfType<PlayerShadow>();

            if (foundShadow == null)
            {
                MelonLogger.Msg("PlayerShadow not found");
                UnloadStolenScene();
                return;
            }

            playerShadow = GameObject.Instantiate(foundShadow.gameObject);
            playerShadow.name = "StolenPlayerShadow";

            GameObject.Destroy(playerShadow.GetComponent<PlayerShadow>());
            GameObject.DontDestroyOnLoad(playerShadow);

            playerShadow.SetActive(false);

            MelonLogger.Msg("Successfully stole PlayerShadow");
            UnloadStolenScene();
        };
    }

    private void UnloadStolenScene()
    {
        SceneManager.UnloadSceneAsync(2).completed += (operation) =>
        {
            MelonLogger.Msg("Scene unloaded.");
            ReloadTitleMenu();
        };
    }

    private void ReloadTitleMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void FindLocalPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            localPlayerTransform = playerObject.transform;
            MelonLogger.Msg("Local player transform found.");
        }
    }

    public void SendPlayerTransforms()
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

    public void ReceivePlayerTransforms()
    {
        uint msgSize;
        while (SteamNetworking.IsP2PPacketAvailable(out msgSize))
        {
            byte[] buffer = new byte[msgSize];
            uint bytesRead;
            CSteamID senderID;

            if (SteamNetworking.ReadP2PPacket(buffer, msgSize, out bytesRead, out senderID))
            {
                SetPlayerDataFromBytes(buffer);
                Debug.Log($"Received player data from {senderID} and applied it.");
            }
        }
    }

    /*private void SpawnPlayerShadows()
    {
        int playerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

        for (int i = 0; i < playerCount; i++)
        {
            CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyID, i);
            ulong playerSteamID = playerID.m_SteamID;

            if (playerID != SteamUser.GetSteamID() && !remotePlayers.ContainsKey(playerSteamID))
            {
                GameObject shadow = GameObject.Instantiate(playerShadow);
                shadow.name = $"PlayerShadow_{playerSteamID}";
                shadow.SetActive(true);
                remotePlayers[playerSteamID] = shadow;

                //ApplyPlayerShadowMaterial(remotePlayers[playerSteamID]);
            }
        }
    }*/
}