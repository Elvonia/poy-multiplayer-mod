using Multiplayer.Logger;
using Multiplayer.Steam;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
        private UButton lobbyTypeButton;
        private UButton playerCountButton;
        private UButton playerColorButton;

        private ELobbyType lobbyType;
        private int playerCount;
        private Color playerColor;
        private int playerColorIndex = 0;

        private GameObject multiplayerButton;
        private Text multiplayerButtonText;

        private InGameMenu inGameMenu;

        private GameObject multiplayerPanel;
        private Text lobbyIdText;
        private Text playerCountText;
        private Text playerColorText;
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

            string lobbyTypeString = SteamMatchmaking.GetLobbyData(LobbyManager.LobbyID, "lobby_type");
            if (!System.Enum.TryParse(lobbyTypeString, out lobbyType))
            {
                lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
            }

            playerCount = SteamMatchmaking.GetLobbyMemberLimit(LobbyManager.LobbyID);
            playerColor = instance.player.GetColor();

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

            lobbyTypeButton = CreateButtonFromTemplate("LobbyType", lobbyType.ToString(), 48);
            lobbyTypeButton.onClick.AddListener(() =>
            {
                LogManager.Debug($"Clicked lobby type button");
                ClickLobbyTypeButton();
                EventSystem.current.SetSelectedGameObject(null);
            });

            AddLabel("Max Player Count:", out playerCountText);

            playerCountButton = CreateButtonFromTemplate("PlayerCount", playerCount.ToString(), 48);
            playerCountButton.onClick.AddListener(() =>
            {
                LogManager.Debug($"Clicked player count button");
                ClickPlayerCountButton();
                EventSystem.current.SetSelectedGameObject(null);
            });

            AddLabel("Player Color:", out playerColorText);

            playerColorButton = CreateButtonFromTemplate("PlayerColor", playerColor.ToString(), 48);
            playerColorButton.onClick.AddListener(() =>
            {
                LogManager.Debug($"Clicked player color button");
                ClickPlayerColorButton();
                EventSystem.current.SetSelectedGameObject(null);
            });

            UButton inviteButton = CreateButtonFromTemplate("Invite", "Invite", 64);
            inviteButton.onClick.AddListener(() =>
            {
                LogManager.Debug($"Clicked invite button");
                SteamFriends.ActivateGameOverlayInviteDialog(LobbyManager.LobbyID);
                EventSystem.current.SetSelectedGameObject(null);
            });

            UButton backButton = CreateButtonFromTemplate("Back", "Back", 76);
            backButton.onClick.AddListener(() =>
            {
                LogManager.Debug("Closed multiplayer panel");
                multiplayerPanel.SetActive(false);
            });

            multiplayerPanel.AddComponent<MultiplayerPanelAutoHider>().Init(multiplayerPanel);
        }

        private void ClickLobbyTypeButton()
        {
            if (lobbyType == ELobbyType.k_ELobbyTypeFriendsOnly)
            {
                lobbyType = ELobbyType.k_ELobbyTypePrivate;

                Text buttonText = lobbyTypeButton.GetComponentInChildren<Text>();
                buttonText.text = "Private";

                SteamMatchmaking.SetLobbyType(LobbyManager.LobbyID, lobbyType);
                SteamMatchmaking.SetLobbyData(LobbyManager.LobbyID, "lobby_type", lobbyType.ToString());

                return;
            }

            if (lobbyType == ELobbyType.k_ELobbyTypePrivate)
            {
                lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;

                Text buttonText = lobbyTypeButton.GetComponentInChildren<Text>();
                buttonText.text = "Friends Only";

                SteamMatchmaking.SetLobbyType(LobbyManager.LobbyID, lobbyType);
                SteamMatchmaking.SetLobbyData(LobbyManager.LobbyID, "lobby_type", lobbyType.ToString());

                return;
            }
        }

        private void ClickPlayerCountButton()
        {
            if (playerCount == 4)
            {
                playerCount = 8;

                Text buttonText = playerCountButton.GetComponentInChildren<Text>();
                buttonText.text = playerCount.ToString();

                SteamMatchmaking.SetLobbyMemberLimit(LobbyManager.LobbyID, playerCount);

                return;
            }

            if (playerCount == 8)
            {
                playerCount = 12;

                Text buttonText = playerCountButton.GetComponentInChildren<Text>();
                buttonText.text = playerCount.ToString();

                SteamMatchmaking.SetLobbyMemberLimit(LobbyManager.LobbyID, playerCount);

                return;
            }

            if (playerCount == 12)
            {
                playerCount = 4;

                Text buttonText = playerCountButton.GetComponentInChildren<Text>();
                buttonText.text = playerCount.ToString();

                SteamMatchmaking.SetLobbyMemberLimit(LobbyManager.LobbyID, playerCount);

                return;
            }
        }

        private void ClickPlayerColorButton()
        {
            Dictionary<Color, string> colorNames = new Dictionary<Color, string>()
            {
                { Color.white, "White" },
                { Color.black, "Black" },
                { Color.red, "Red" },
                { Color.green, "Green" },
                { Color.blue, "Blue" },
                { Color.yellow, "Yellow" },
                { Color.cyan, "Cyan" },
                { Color.magenta, "Magenta" },
                { Color.grey, "Grey" }
            };

            Color[] colors = colorNames.Keys.ToArray();

            playerColorIndex = (playerColorIndex + 1) % colors.Length;
            playerColor = colors[playerColorIndex];
            string colorName = colorNames[playerColor];

            Text buttonText = playerColorButton.GetComponentInChildren<Text>();
            buttonText.text = colorName;
            buttonText.color = playerColor;

            instance.player.SetColor(playerColor);
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
            text.fontSize = 64;
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

        public void UpdateUI()
        {
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(LobbyManager.LobbyID);
            string playerList = $"Lobby ID: {LobbyManager.LobbyID}\nPlayers: {playerCount}\n";

            for (int i = 0; i < playerCount; i++)
            {
                CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyManager.LobbyID, i);
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
