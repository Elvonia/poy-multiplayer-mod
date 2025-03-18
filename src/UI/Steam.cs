using Multiplayer.Logger;
using Steamworks;

namespace Multiplayer.UI
{
    public class Steam
    {
        // handle friends list
        public static void OpenSteamFriendsList(MultiplayerMod instance)
        {
            if (instance.currentLobbyID.IsValid())
            {
                SteamFriends.ActivateGameOverlayInviteDialog(instance.currentLobbyID);
                LogManager.Debug("Opened steam friends list");
            }
        }
    }
}
