using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer
{
    public class ShadowClone
    {
        public static void StealPlayerShadow(MultiplayerMod instance)
        {
            SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive).completed += (operation) =>
            {
                PlayerShadow foundShadow = GameObject.FindObjectOfType<PlayerShadow>();

                if (foundShadow == null)
                {
                    UnloadStolenScene();
                    return;
                }

                instance.shadowClone = GameObject.Instantiate(foundShadow.gameObject);
                instance.shadowClone.name = "StolenPlayerShadow";

                GameObject.Destroy(instance.shadowClone.GetComponent<PlayerShadow>());
                GameObject.DontDestroyOnLoad(instance.shadowClone);

                instance.shadowClone.SetActive(false);

                UnloadStolenScene();
            };
        }

        private static void UnloadStolenScene()
        {
            SceneManager.UnloadSceneAsync(2).completed += (operation) =>
            {
                ReloadTitleMenu();
            };
        }

        private static void ReloadTitleMenu()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
