using UnityEngine;
using Mirror;

namespace ZombieGame
{
    public class CharacterAnimator : NetworkBehaviour
    {
        [SerializeField] private string animVelocity_x = "Strafe";
        [SerializeField] private string animVelocity_y = "Forward";
        [SerializeField] private string animGrounded = "Grounded";
        [SerializeField] private Animator anim;

        [Space]
        [SerializeField] private FPS.FPSController fpsController = null;
        [SerializeField] private CharacterController characterController = null;
        [SerializeField] private float speedMultiplier = 0.5f;

        private Vector3 velocity;

#if UNITY_EDITOR
        private void OnValidate() => ValidateComponents();
#endif

        private void Start() => ValidateComponents();

        private void ValidateComponents()
        {
            if (anim == null) anim = GetComponentInChildren<Animator>();
            if (characterController == null) characterController = GetComponent<CharacterController>();
        }        

        private void Update()
        {
            if (!hasAuthority) return;
            if (Time.frameCount % 3 != 0) return;

            var grounded = fpsController.GetMoveInput().y <= 0 || fpsController.GetGrounded();
            CmdSetBool(animGrounded, fpsController.GetGrounded());

            velocity = transform.InverseTransformDirection(characterController.velocity * speedMultiplier);
            CmdSetFloat(animVelocity_y, velocity.z);
            CmdSetFloat(animVelocity_x, velocity.x);
        }

        [Command]
        private void CmdSetFloat(string key, float val)
        {
            RpcSetFloat(key, val);
        }

        [ClientRpc]
        private void RpcSetFloat(string key, float val)
        {
            anim.SetFloat(key, val);
        }

        [Command]
        private void CmdSetBool(string key, bool val)
        {
            RpcSetBool(key, val);
        }

        [ClientRpc]
        private void RpcSetBool(string key, bool val)
        {
            anim.SetBool(key, val);
        }
    }
}
