using UnityEngine;
using Mirror;

namespace ZombieShooter.NetworkEffects
{
    public class NetworkStepManager : NetworkBehaviour
    {
        [SerializeField] private FPS.StepManager stepManager = null;

        private const string FOOTSTEP = "Footstep", JUMP = "Jump", LAND = "Land";

        private void OnEnable()
        {
            stepManager.onFootstep += () => HandleFootstep();
            stepManager.onJump += () => HandleJump();
            stepManager.onLand += () => HandleLand();
        }

        private void OnDisable()
        {
            stepManager.onFootstep -= () => HandleFootstep();
            stepManager.onJump -= () => HandleJump();
            stepManager.onLand -= () => HandleLand();
        }

        private void HandleFootstep()
        {
            if (hasAuthority) CmdHandleSound(FOOTSTEP);
        }

        private void HandleJump()
        {
            if (hasAuthority) CmdHandleSound(JUMP);
        }

        private void HandleLand()
        {
            if (hasAuthority) CmdHandleSound(LAND);
        }

        [Command]
        private void CmdHandleSound(string tag) => RpcHandleSound(tag);

        [ClientRpc]
        private void RpcHandleSound(string tag)
        {
            switch (tag)
            {
                case FOOTSTEP:
                    stepManager.PlayFootStep();
                    break;
                case JUMP:
                    stepManager.PlayJump();
                    break;
                case LAND:
                    stepManager.PlayLand();
                    break;
                default:
                    break;
            }
        }
    }
}