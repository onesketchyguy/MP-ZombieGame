using UnityEngine;
using UnityEngine.AI;
using Mirror;

namespace ZombieGame
{
    public class ZombieController : NetworkBehaviour, IDamagable
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;
        public TextMesh healthBar;

        [Header("Combat")]
        [SerializeField] private float attackRange = 2.0f;

        [Header("Stats")]
        [SyncVar] public float health = 4;

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

        private void Update()
        {
            if (Time.frameCount % 6 == 0) healthBar.text = new string('-', Mathf.RoundToInt(health));
        }

        [ClientRpc]
        private void RpcSetDestination(Vector3 destination) => agent.SetDestination(destination);

        //[ServerCallback]
        //void OnTriggerEnter(Collider other)
        //{
        //    if (other.GetComponent<Projectile>() != null)
        //    {
        //        --health;
        //    }
        //}

        [ServerCallback]
        public void RecieveDamage(float damage)
        {
            health -= damage;

            if (health <= 0) NetworkServer.Destroy(gameObject);
        }
    }
}
