using Steamworks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerMod.UI
{
    public class MultiplayerDebugUI
    {
        private GameObject canvasObject;
        private GameObject textObject;

        private Canvas canvas;
        private Text text;

        public bool active;

        public MultiplayerDebugUI()
        {
            SetupUI();
            active = true;
        }

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

        private void SetupUI()
        {
            canvasObject = new GameObject("Multiplayer_Debug");
            textObject = new GameObject("DebugText");

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

            UpdateDebugText("Initializing...");
        }

        public void UpdateLobbyUI(CSteamID lobbyID)
        {
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            string playerList = $"Lobby ID: {lobbyID}\nPlayers: {playerCount}\n";

            for (int i = 0; i < playerCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                string playerName = SteamFriends.GetFriendPersonaName(playerID);
                playerList += $"{playerName}\n";
            }

            UpdateDebugText(playerList);
        }

        private void UpdateDebugText(string message)
        {
            text.text = message;
        }
    }
}
