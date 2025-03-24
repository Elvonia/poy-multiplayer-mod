using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer
{
    public static class ShadowClone
    {
        public static GameObject ShadowObject { get; private set; }

        public static void Initialize()
        {
            StealPlayerShadow();
        }

        public static void StealPlayerShadow()
        {
            SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive).completed += (operation) =>
            {
                PlayerShadow foundShadow = GameObject.FindObjectOfType<PlayerShadow>();

                if (foundShadow == null)
                {
                    UnloadStolenScene();
                    return;
                }

                ShadowObject = GameObject.Instantiate(foundShadow.gameObject);
                ShadowObject.name = "StolenPlayerShadow";

                GameObject.Destroy(ShadowObject.GetComponent<PlayerShadow>());
                GameObject.DontDestroyOnLoad(ShadowObject);

                ShadowObject.SetActive(false);

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
