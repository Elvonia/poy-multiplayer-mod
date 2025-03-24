using Multiplayer.Logger;
using Multiplayer.Steam;
using Steamworks;

namespace Multiplayer.UI
{
    public class Steam
    {
        // handle friends list
        public static void OpenSteamFriendsList(MultiplayerMod instance)
        {
            if (LobbyManager.LobbyID.IsValid())
            {
                SteamFriends.ActivateGameOverlayInviteDialog(LobbyManager.LobbyID);
                LogManager.Debug("Opened steam friends list");
            }
        }
    }
}
