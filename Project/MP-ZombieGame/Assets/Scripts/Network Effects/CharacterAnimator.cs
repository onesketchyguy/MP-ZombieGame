using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace ZombieGame
{
    public class CharacterAnimator : NetworkBehaviour
    {
        [SerializeField] private string anim_velocity_x = "Strafe";
        [SerializeField] private string anim_velocity_y = "Forward";
        [SerializeField] private Animator anim;

        [Space]
        [SerializeField] private CharacterController characterController = null;

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

            var velocity = transform.InverseTransformDirection(characterController.velocity);
            CmdSetFloat(anim_velocity_y, velocity.z);
            CmdSetFloat(anim_velocity_x, velocity.x);
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
    }
}
