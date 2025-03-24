using MelonLoader;
using MelonLoader.Utils;
using System.Drawing;
using System.IO;

#if BEPINEX

#elif MELONLOADER

#endif

namespace Multiplayer.Config
{
    public class Configuration
    {
        public string LobbyName;
        public string LobbyPassword;

        public string LobbyType;
        public int MaxPlayers;
        
        public Color PlayerColor;

#if MELONLOADER

        MelonPreferences_Category multiplayerCategory;

        MelonPreferences_Entry<string> lobbyNamePref;
        MelonPreferences_Entry<string> lobbyPasswordPref;

        MelonPreferences_Entry<string> lobbyTypePref;
        MelonPreferences_Entry<int> maxPlayersPref;

        MelonPreferences_Entry<Color> playerColorPref;

        public Configuration()
        {
            string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "com.github.Elvonia.poy-multiplayer-mod.cfg");

            multiplayerCategory = MelonPreferences.CreateCategory("com.github.Elvonia.poy-multiplayer-mod");
            multiplayerCategory.SetFilePath(configPath);

            lobbyNamePref = multiplayerCategory.CreateEntry<string>("LobbyName", string.Empty);
            lobbyPasswordPref = multiplayerCategory.CreateEntry<string>("LobbyPassword", string.Empty);

            lobbyTypePref = multiplayerCategory.CreateEntry<string>("LobbyType", string.Empty);
            maxPlayersPref = multiplayerCategory.CreateEntry<int>("MaxPlayers", 0);

            playerColorPref = multiplayerCategory.CreateEntry<Color>("PlayerColor", Color.Empty);
        }

#endif
    }
}
