using UnityEngine;
using Mirror;

namespace ZombieGame
{
    public class ZombieManager : NetworkBehaviour
    {
        [SerializeField] private NetworkIdentity[] zombiePrefabs = null;
        [SerializeField] private Transform[] spawnPoints = null;

        [SerializeField] private float spawnTime = 1.0f;
        private float waveTime = 0.0f;
        private bool waveAlive = false;

        int spawned = 0;

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(ServerUpdate), 0.1f, 0.1f);
        }

        private void ServerUpdate()
        {
            if (waveAlive == false)
            {
                waveTime += 0.1f;

                if (waveTime >= spawnTime)
                {
                    if (spawned >= spawnPoints.Length)
                    {
                        waveAlive = true;
                    }
                    else
                    {
                        Spawn(spawned);
                        spawned++;
                    }
                }
            }
        }

        void Spawn(int spawnIndex)
        {
            var spawnPoint = spawnPoints[spawnIndex];
            var zombie = Instantiate(zombiePrefabs[Random.Range(0, zombiePrefabs.Length)].gameObject, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(zombie);
        }
    }
}