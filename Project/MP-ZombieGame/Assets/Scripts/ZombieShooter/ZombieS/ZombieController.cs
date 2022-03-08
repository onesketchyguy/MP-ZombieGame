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

        [Header("Combat")]
        [SerializeField] private float attackRange = 2.0f;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        private Transform target = null;

        private void SetTarget()
        {
            float maxDist = target == null ? 100.0f : Vector3.Distance(transform.position, target.position);
            var players = AiBlackboard.GetPlayers();

            foreach (var item in players)
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
            healthBar.text = new string('-', health);
        }

        [ServerCallback]
        internal virtual void OnServerUpdate()
        {
            SetTarget();

            if (target != null)
            {
                // FIXME: check when in range for attacks

                RpcSetDestination(target.position);
            }
        }

        [ClientRpc]
        private void RpcSetDestination(Vector3 destination) => agent.SetDestination(destination);

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Projectile>() != null)
            {
                --health;
                healthBar.text = new string('-', health);

                if (health == 0)
                    NetworkServer.Destroy(gameObject);
            }
        }
    }
}
