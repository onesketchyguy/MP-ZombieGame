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

        [Header("Animations")]
        [SerializeField] private string animX = "xSpd";
        [SerializeField] private string animY = "ySpd";
        [SerializeField] private string animAtt = "attack";
        [SerializeField] private string animDie = "die";
        [SerializeField] private string animHurt = "isHit";

        [Header("Combat")]
        [SerializeField] private float attackRange = 2.0f;

        [Header("Stats")]
        [SyncVar] public float health = 4;
        [SerializeField] private int killScore = 100;

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

            // Update animations
            RpcAnimateMovement();
        }

        private void Update()
        {
            if (Time.frameCount % 6 == 0) healthBar.text = new string('-', Mathf.RoundToInt(health));
        }

        [ClientRpc]
        private void RpcAnimateMovement()
        {
            var forward = Vector3.Dot(agent.velocity, agent.transform.forward);
            var right = Vector3.Dot(agent.velocity, agent.transform.right);

            animator.SetFloat(animX, right);
            animator.SetFloat(animY, forward);
        }

        [ClientRpc]
        private void RpcAnimateDamage() => animator.SetTrigger(animHurt);

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
        public void RecieveDamage(float damage, ref bool killedTarget)
        {
            health -= damage;

            if (health <= 0)
            {
                killedTarget = true;
                NetworkServer.Destroy(gameObject);
            }
            else RpcAnimateDamage();
        }

        public int GetKillScore()
        {
            return killScore;
        }
    }
}
