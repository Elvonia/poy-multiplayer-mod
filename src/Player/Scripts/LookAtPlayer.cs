using UnityEngine;

namespace Multiplayer
{
    public class LookAtPlayer : MonoBehaviour
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
                Vector3 direction = mainCamera.transform.position - transform.position;
                direction.y = 0;

                transform.rotation = Quaternion.LookRotation(-direction);
            }
        }
    }
}
