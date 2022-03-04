using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using UnityEngine.UI;

namespace ZombieGame.NetworkUI
{
    public class NetworkDiscoveryHUD : MonoBehaviour
    {
        private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

        public NetworkDiscovery networkDiscovery;

        public UnityEngine.Events.UnityEvent onJoinServerEvent;
        public UnityEngine.Events.UnityEvent onHostServerEvent;

        [SerializeField] private GameObject hostButton = null;

        public GameObject serverButtonPrefab;
        public Transform serverButtonParent;

        private void OnEnable()
        {
            networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
            RefreshServerList();
        }

        private void CreateServerButton(ServerResponse info)
        {
            var go = Instantiate(serverButtonPrefab, serverButtonParent);
            var textComp = go.GetComponentInChildren<Text>();
            textComp.text = $"{info.serverId}";//$"{info.serverHost}:{info.EndPoint.Address}";
            go.name = textComp.text;//info.serverHost.ToString();

            go.GetComponentInChildren<Button>().onClick.AddListener(() => Connect(info));
        }

        public void ClearServerList()
        {
            networkDiscovery.StopDiscovery();

            discoveredServers.Clear();
            for (int i = 0; i < serverButtonParent.childCount; i++)
            {
                var child = serverButtonParent.GetChild(i);
                if (child.transform == serverButtonParent.transform)
                    continue;

                Destroy(child.gameObject);
            }
        }

        public void RefreshServerList()
        {
            ClearServerList();
            networkDiscovery.StartDiscovery();
        }

        // FIXME: Allow user to start a dedicated server!
        //public void StartServer()
        //{
        //    ClearServerList();
        //    NetworkManager.singleton.StartServer();

        //    networkDiscovery.AdvertiseServer();
        //}

        public void Host()
        {
            if (NetworkServer.localConnection != null)
            {
                // A local host exists connect to them instead of trying to host.
                NetworkManager.singleton.StartClient();

                if (onJoinServerEvent != null) onJoinServerEvent.Invoke();

                return;
            }

            ClearServerList();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();

            if (onHostServerEvent != null) onHostServerEvent.Invoke();
        }

        public void SoloHost()
        {
            if (NetworkServer.localConnection != null)
            {
                // A local host exists connect to them instead of trying to host.
                NetworkManager.singleton.StartClient();

                if (onJoinServerEvent != null)
                    onJoinServerEvent.Invoke();

                return;
            }

            ClearServerList();
            NetworkManager.singleton.StartHost();

            if (onHostServerEvent != null) onHostServerEvent.Invoke();
        }

        public void Connect(ServerResponse info)
        {
            NetworkManager.singleton.StartClient(info.uri);

            if (onJoinServerEvent != null)
                onJoinServerEvent.Invoke();
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            Debug.Log($"Discovered server: {info.EndPoint.Address}");

            // FIXME: check the versioning to decide if you can connect to the server or not using this method

            if (discoveredServers.ContainsKey(info.serverId)) return;

            discoveredServers[info.serverId] = info;
            CreateServerButton(info);

            if (NetworkServer.localConnection != null)
            {
                hostButton.SetActive(false);
            }
        }
    }
}
