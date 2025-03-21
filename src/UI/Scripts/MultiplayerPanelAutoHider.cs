using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerPanelAutoHider : MonoBehaviour
    {
        private GameObject panel;

        public void Init(GameObject panelToHide)
        {
            panel = panelToHide;
        }

        private void Update()
        {
            if (!InGameMenu.isCurrentlyNavigationMenu && panel.activeSelf)
            {
                panel.SetActive(false);
            }
        }
    }
}
