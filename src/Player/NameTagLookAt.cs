using UnityEngine;

namespace Multiplayer
{
    public class NameTagLookAt : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(mainCamera.transform);
                transform.Rotate(0, 180, 0); // flip the text
            }
        }
    }
}
