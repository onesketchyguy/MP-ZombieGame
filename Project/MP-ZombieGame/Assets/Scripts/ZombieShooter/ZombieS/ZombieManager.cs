using UnityEngine;
using Mirror;

namespace ZombieGame
{
    public class ZombieManager : NetworkBehaviour
    {
        [SerializeField] private GameObject zombiePrefab = null;
        [SerializeField] private Transform[] spawnPoints = null;

        [SerializeField] private float spawnTime = 1.0f;
        private float waveTime = 0.0f;
        private bool waveAlive = false;

        public override void OnStartServer()
        {
            base.OnStartServer();

            InvokeRepeating(nameof(ServerUpdate), 0.1f, 0.1f);
        }

        [Server]
        private void ServerUpdate()
        {
            //if (waveAlive == false)
            //{
            //    waveTime += 0.1f;

            //    if (waveTime >= spawnTime)
            //    {
            //        for (int i = 0; i < spawnPoints.Length; i++)
            //        {
            //            CmdSpawn(i);
            //        }
            //    }
            //}
        }

        // this is called on the server
        [Command]
        void CmdSpawn(int spawnIndex)
        {
            var spawnPoint = spawnPoints[spawnIndex];

            GameObject projectile = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(projectile);
        }
    }
}