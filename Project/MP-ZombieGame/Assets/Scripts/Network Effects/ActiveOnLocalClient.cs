using UnityEngine;

namespace ZombieGame
{
    public class ActiveOnLocalClient : MonoBehaviour
    {
        [SerializeField] private bool activeOnLocal = false;
        [SerializeField] private bool disableOnCompletion = true;
        [SerializeField] private Mirror.NetworkBehaviour client = null;

        private Mirror.NetworkBehaviour GetClient()
        {
            if (client == null)
            {
                var clients = FindObjectsOfType<Mirror.NetworkBehaviour>();
                foreach (var client in clients)
                {
                    if (client.isLocalPlayer) this.client = client;
                }
            }

            return client;
        }

        private void LateUpdate()
        {
            if (GetClient() == null) return;

            if (GetClient().isLocalPlayer) gameObject.SetActive(activeOnLocal);
            else gameObject.SetActive(!activeOnLocal);

            if (disableOnCompletion) this.enabled = false;
        }
    }
}
