using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Mirror.Examples.Tanks;

namespace ZombieGame
{
    public class ZombieController : NetworkBehaviour
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;
        public TextMesh healthBar;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        private Transform target = null;

        private void SetTarget()
        {
            float maxDist = 100.0f;
            target = null;

            foreach (var item in FindObjectsOfType<PlayerController>())
            {
                var dist = Vector3.Distance(transform.position, item.transform.position);
                if (dist < maxDist)
                {
                    maxDist = dist;
                    target = item.transform;
                }
            }
        }

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(OnServerUpdate), 0, 0.1f);
        }

        [ServerCallback]
        internal virtual void OnServerUpdate()
        {
            if (Time.frameCount % 10 == 0) SetTarget();

            if (target != null)
            {
                RpcSetDestination(target.position);
            }
        }

        [ClientRpc]
        private void RpcSetDestination(Vector3 destination) => agent.SetDestination(destination);

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Projectile>() != null)
            {
                --health;
                if (health == 0)
                    NetworkServer.Destroy(gameObject);
            }
        }
    }
}
