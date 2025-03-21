using Multiplayer.Logger;
using Steamworks;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MelonLoader.MelonLogger;
using UButton = UnityEngine.UI.Button;

namespace Multiplayer.UI
{
    public class MultiplayerUI
    {
        private MultiplayerMod instance;

        // new ui
        private GameObject inGameMenuButtonsParent;
        private GameObject inGameMenuButtonTemplate;

        private UButton buttonTemplate;

        private GameObject multiplayerButton;
        private Text multiplayerButtonText;

        private InGameMenu inGameMenu;

        private GameObject multiplayerPanel;
        private Text lobbyIdText;
        private Text playerCountText;
        private Text playerColor;
        //

        private GameObject canvasObject;
        private GameObject textObject;

        private Canvas canvas;
        private Text text;

        public bool active;

        public MultiplayerUI(MultiplayerMod instance, int sceneIndex)
        {
            if (sceneIndex == 0 || sceneIndex == 1
                || sceneIndex == 37 || sceneIndex == 67)
            {
                SetupMenuUI();
                active = true;

                return;
            }

            this.instance = instance;
            inGameMenu = GameObject.FindObjectOfType<InGameMenu>();

            SetupInGameMenuUI();
            active = true;

            if (inGameMenuButtonsParent != null)
                return;
            
            if (inGameMenu != null)
            {
                canvas.transform.SetParent(inGameMenu.inGameMenuObject.transform);
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

        private void SetupInGameMenuUI()
        {
            inGameMenuButtonsParent = inGameMenu.inGameMenuObject.transform.Find("menu_pg/MenuContainer").gameObject;
            LogManager.Debug("Found menu container");
            inGameMenuButtonTemplate = inGameMenuButtonsParent.transform.Find("Options").gameObject;
            LogManager.Debug("Found options button");

            if (inGameMenuButtonTemplate != null)
            {
                multiplayerButton = GameObject.Instantiate(inGameMenuButtonTemplate.gameObject, inGameMenuButtonTemplate.transform.parent);
                multiplayerButton.transform.SetSiblingIndex(inGameMenuButtonTemplate.transform.GetSiblingIndex());
                multiplayerButton.name = "Multiplayer";

                multiplayerButtonText = multiplayerButton.GetComponentInChildren<Text>();

                if (multiplayerButtonText != null)
                {
                    multiplayerButtonText.text = "Multiplayer";
                    multiplayerButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    LogManager.Debug("Set button text");
                }

                buttonTemplate = inGameMenuButtonTemplate.GetComponent<UButton>();
                GameObject.DestroyImmediate(multiplayerButton.GetComponent<UButton>());

                UButton button = multiplayerButton.AddComponent<UButton>();

                button.colors = buttonTemplate.colors;
                button.targetGraphic = multiplayerButton.GetComponentInChildren<Text>();
                button.transition = Selectable.Transition.ColorTint;

                button.onClick.AddListener(() => {
                    LogManager.Debug("Clicked multiplayer button");

                    if (multiplayerPanel == null)
                        CreateMultiplayerMenu();

                    multiplayerPanel.SetActive(true);
                });
            }
        }

        private void CreateMultiplayerMenu()
        {

            multiplayerPanel = new GameObject("MultiplayerPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            multiplayerPanel.transform.SetParent(inGameMenu.inGameMenuObject.transform, false);
            multiplayerPanel.SetActive(false);

            RectTransform rect = multiplayerPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Screen.width, Screen.height);
            rect.anchoredPosition = Vector2.zero;

            Image bg = multiplayerPanel.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 1f);

            VerticalLayoutGroup layout = multiplayerPanel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 10;
            layout.padding = new RectOffset(60, 60, 60, 60);

            AddLabel("Lobby Type:", out lobbyIdText);
            AddLabel("Max Player Count:", out playerCountText);
            AddLabel("Player Color:", out playerColor);

            UButton inviteButton = CreateButtonFromTemplate("Invite", "Invite", 72);
            inviteButton.onClick.AddListener(() =>
            {
                LogManager.Debug($"Clicked invite button");
                SteamFriends.ActivateGameOverlayInviteDialog(instance.currentLobbyID);
                EventSystem.current.SetSelectedGameObject(null);
            });

            UButton backButton = CreateButtonFromTemplate("Back", "Back", 96);
            backButton.onClick.AddListener(() =>
            {
                LogManager.Debug("Closed multiplayer panel");
                multiplayerPanel.SetActive(false);
            });

            multiplayerPanel.AddComponent<MultiplayerPanelAutoHider>().Init(multiplayerPanel);
        }

        private UButton CreateButtonFromTemplate(string name, string text, int fontSize)
        {
            GameObject buttonObj = Object.Instantiate(inGameMenuButtonTemplate, multiplayerPanel.transform);
            buttonObj.name = name;

            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = text;
                buttonText.fontSize = fontSize;
            }

            UButton oldButton = buttonObj.GetComponent<UButton>();
            if (oldButton != null)
            {
                Object.DestroyImmediate(oldButton);
            }

            UButton newButton = buttonObj.AddComponent<UButton>();
            newButton.colors = buttonTemplate.colors;
            newButton.transition = Selectable.Transition.ColorTint;
            newButton.targetGraphic = buttonText;

            return newButton;
        }

        private void AddLabel(string prefix, out Text labelText)
        {
            GameObject go = new GameObject(prefix, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(multiplayerPanel.transform, false);
            Text text = go.GetComponent<Text>();
            text.text = prefix + " ";
            text.font = multiplayerButtonText.font;
            text.fontSize = 48;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.lineSpacing = 3f;

            text.rectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);

            labelText = text;
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
