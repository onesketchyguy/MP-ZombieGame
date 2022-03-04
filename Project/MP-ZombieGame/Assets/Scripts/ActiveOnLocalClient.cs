using UnityEngine;

namespace ZombieGame
{
    public class ActiveOnLocalClient : MonoBehaviour
    {
        [SerializeField] private bool activeOnLocal = false;
        [SerializeField] private Mirror.NetworkBehaviour client = null;

        private void LateUpdate()
        {
            if (client == null) return;

            if (client.isLocalPlayer) gameObject.SetActive(activeOnLocal);
            else gameObject.SetActive(!activeOnLocal);

            this.enabled = false;
        }
    }
}
