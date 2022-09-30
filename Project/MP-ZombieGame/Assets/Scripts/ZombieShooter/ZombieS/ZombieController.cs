using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections.Generic;

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
        [Space]
        [SerializeField] private AnimationClip attackClipToOverride = null;
        [SerializeField] private AnimationClip[] attackClips = null;
        private AnimatorOverrideController overrideController = null;
        private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        [Header("Combat")]
        [SerializeField] private float attackRange = 2.0f;
        private float attTime = 0;

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
            if (health <= 0)
            {
                if (agent.isStopped == false) RpcStop();

                return;
            }

            if (attTime >= 0) attTime -= 0.1f;

            SetTarget();

            if (target != null)
            {
                // Check when in range for attacks

                if (Vector3.Distance(transform.position, target.position) < attackRange)
                {
                    if (agent.isStopped == false) RpcStop();

                    if (attTime <= Mathf.Epsilon)
                    {
                        int r = Random.Range(0, attackClips.Length);
                        attTime = attackClips[r].length * 0.75f;

                        RpcFace(target.position);

                        // Attack
                        RpcAnimateAttack(r);
                    }
                }
                else
                {
                    // Move to target
                    if (Vector3.Distance(agent.destination, target.position) > 1.0f)
                        RpcSetDestination(target.position);
                }
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
        private void RpcFace(Vector3 point)
        {
            agent.transform.LookAt(new Vector3(point.x, agent.transform.position.y, point.z));
        }


        [ClientRpc]
        private void RpcAnimateAttack(int att)
        {
            if (overrideController == null)
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                overrideController.GetOverrides(overrideClips);
            }

            // Get the old clip from the animator and remove it
            for (int i = 0; i < overrideClips.Count; i++)
            {
                if (overrideClips[i].Key == attackClipToOverride)
                {
                    overrideClips.RemoveAt(i);
                    break;
                }
            }

            // Override it with a new one
            overrideClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(attackClipToOverride, attackClips[att]));

            // Apply the overrides
            overrideController.ApplyOverrides(overrideClips);
            animator.runtimeAnimatorController = overrideController;

            // Animate it
            animator.SetTrigger(animAtt);

            // FIXME: try to deal damage
        }

        [ClientRpc]
        private void RpcAnimateDamage() => animator.SetTrigger(animHurt);

        [ClientRpc]
        private void RpcAnimateDeath()
        {
            animator.SetTrigger(animDie);
        }

        [ClientRpc]
        private void RpcSetDestination(Vector3 destination)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        [ClientRpc]
        private void RpcStop()
        {
            agent.SetDestination(transform.position);
            agent.isStopped = true;
        }

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
