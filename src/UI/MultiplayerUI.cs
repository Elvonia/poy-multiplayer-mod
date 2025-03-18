using Multiplayer.Logger;
using Multiplayer.Steam;
using Steamworks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    public class MultiplayerUI
    {
        private GameObject canvasObject;
        private GameObject textObject;

        private Canvas canvas;
        private Text text;

        public bool active;

        public MultiplayerUI(int sceneIndex)
        {
            if (sceneIndex == 0 || sceneIndex == 1
                || sceneIndex == 37 || sceneIndex == 67)
            {
                SetupMenuUI();
                active = true;

                return;
            }

            SetupInGameMenuUI();
            active = true;

            InGameMenu inGameMenu = GameObject.FindObjectOfType<InGameMenu>();
            
            if (inGameMenu != null)
            {
                canvas.transform.parent = inGameMenu.inGameMenuObject.transform;
            }
            else
            {
                text.alignment = TextAnchor.UpperLeft;
            }
        }

        // InGameMenu - comp InGameMenu
        // InGameMenuObj_DisableMe

        public void EnableUI()
        {
            canvasObject.SetActive(true);
            active = true;
        }

        public void DisableUI()
        {
            canvasObject.SetActive(false);
            active = false;
        }

        public void SetupInGameMenuUI()
        {
            canvasObject = new GameObject("Multiplayer_UI");
            textObject = new GameObject("LobbyInfo");

            textObject.transform.SetParent(canvasObject.transform);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 22;

            text = textObject.AddComponent<Text>();
            text.alignment = TextAnchor.UpperRight;
            text.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == "roman-antique.regular");
            text.fontSize = 24;
            text.lineSpacing = 3f;

            text.rectTransform.anchoredPosition = new Vector2(-40, -40);
            text.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

            UpdateText("Initializing...");
        }

        private void SetupMenuUI()
        {
            canvasObject = new GameObject("Multiplayer_UI");
            textObject = new GameObject("LobbyInfo");

            textObject.transform.SetParent(canvasObject.transform);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 22;

            text = textObject.AddComponent<Text>();
            text.alignment = TextAnchor.UpperLeft;
            text.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == "roman-antique.regular");
            text.fontSize = 24;
            text.lineSpacing = 3f;

            text.rectTransform.anchoredPosition = new Vector2(40, -40);
            text.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

            UpdateText("Initializing...");
        }

        public void UpdateUI(CSteamID lobbyID)
        {
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            string playerList = $"Lobby ID: {lobbyID}\nPlayers: {playerCount}\n";

            for (int i = 0; i < playerCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                string playerName = SteamFriends.GetFriendPersonaName(playerID);
                playerList += $"{playerName}\n";
            }

            UpdateText(playerList);
        }

        private void UpdateText(string message)
        {
            text.text = message;
        }
    }
}
