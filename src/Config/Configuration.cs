using System.Drawing;
using System.IO;

#if BEPINEX

using BepInEx.Configuration;

#elif MELONLOADER

using MelonLoader;
using MelonLoader.Utils;

#endif

namespace Multiplayer.Config
{
    public static class Configuration
    {
        public static string LobbyName;
        public static string LobbyPassword;

        public static string LobbyType;
        public static int MaxPlayers;

        public static Color PlayerColor;

#if BEPINEX

        public static ConfigEntry<string> lobbyNameEntry;
        public static ConfigEntry<string> lobbyPasswordEntry;

        public static ConfigEntry<string> lobbyTypeEntry;
        public static ConfigEntry<int> maxPlayersEntry;

        public static ConfigEntry<Color> playerColorEntry;

#elif MELONLOADER            

        private static MelonPreferences_Category multiplayerCategory;

        private static MelonPreferences_Entry<string> lobbyNamePref;
        private static MelonPreferences_Entry<string> lobbyPasswordPref;

        private static MelonPreferences_Entry<string> lobbyTypePref;
        private static MelonPreferences_Entry<int> maxPlayersPref;

        private static MelonPreferences_Entry<Color> playerColorPref;

        public static void Initialize()
        {
            string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "com.github.Elvonia.poy-multiplayer-mod.cfg");

            multiplayerCategory = MelonPreferences.CreateCategory("com.github.Elvonia.poy-multiplayer-mod");
            multiplayerCategory.SetFilePath(configPath);

            lobbyNamePref = multiplayerCategory.CreateEntry<string>("LobbyName", string.Empty);
            lobbyPasswordPref = multiplayerCategory.CreateEntry<string>("LobbyPassword", string.Empty);

            lobbyTypePref = multiplayerCategory.CreateEntry<string>("LobbyType", string.Empty);
            maxPlayersPref = multiplayerCategory.CreateEntry<int>("MaxPlayers", 0);

            playerColorPref = multiplayerCategory.CreateEntry<Color>("PlayerColor", Color.Empty);

            if (File.Exists(configPath))
                LoadConfig();
        }


        private static void LoadConfig()
        {
            multiplayerCategory.LoadFromFile();

            LobbyName = lobbyNamePref.Value;
            LobbyPassword = lobbyPasswordPref.Value;

            LobbyType = lobbyTypePref.Value;
            MaxPlayers = maxPlayersPref.Value;

            PlayerColor = playerColorPref.Value;
        }

        public static void SaveConfig()
        {
            lobbyNamePref.Value = LobbyName;
            lobbyPasswordPref.Value = LobbyPassword;

            lobbyTypePref.Value = LobbyType;
            maxPlayersPref.Value = MaxPlayers;

            playerColorPref.Value = PlayerColor;

            multiplayerCategory.SaveToFile();
        }

#endif

    }
}
