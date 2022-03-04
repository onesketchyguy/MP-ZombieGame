using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Mirror.Examples.Tanks;

namespace ZombieGame
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Components")]
        public Animator handsAnimator;
        public Animator bodyAnimator;
        [SerializeField] private string anim_velocity_x = "Strafe";
        [SerializeField] private string anim_velocity_y = "Forward";
        public TextMesh healthBar;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public GameObject projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        [SerializeField] private FPS.FPSController fpsController;
        [SerializeField] private CharacterController characterController;

        [Header("Client setup")]
        [SerializeField] GameObject body;
        [SerializeField] GameObject arms;

        [SerializeField] private string hideFromClient = "LocalPlayer";
        [SerializeField] private string showClient = "ClientPlayer";

        private Vector3 moveInput;
        private Vector2 lookInput;
        private float runInput;


        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().clientAuthority = true;
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        void Update()
        {
            // always update health bar.
            // (SyncVar hook would only update on clients, not on server)
            healthBar.text = new string('-', health);

            // movement for local player
            if (isLocalPlayer)
            {
                //// rotate
                //float horizontal = Input.GetAxis("Horizontal");
                //transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

                //// move
                //float vertical = Input.GetAxis("Vertical");
                //Vector3 forward = transform.TransformDirection(Vector3.forward);
                //agent.velocity = forward * vertical * agent.speed;
                //animator.SetBool("Moving", agent.velocity != Vector3.zero);

                // shoot
                if (Input.GetMouseButtonDown(0)) CmdFire();

                moveInput.x = Input.GetAxisRaw("Horizontal");
                moveInput.z = Input.GetAxisRaw("Vertical");
                moveInput.y = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

                runInput = Mathf.Lerp(fpsController.GetRunInput(),
                                Input.GetKey(KeyCode.LeftShift) ? 1.1f : -0.1f, 10.0f * Time.deltaTime);

                runInput = Mathf.Clamp01(runInput);

                lookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                fpsController.SetMoveInput(moveInput, runInput).SetLookInput(lookInput);

                body.layer = LayerMask.NameToLayer(hideFromClient);
                arms.layer = LayerMask.NameToLayer(showClient);

                CmdAnimate();
            }
            else
            {
                body.layer = LayerMask.NameToLayer(showClient);
                arms.layer = LayerMask.NameToLayer(hideFromClient);
            }
        }

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
            if (handsAnimator != null) handsAnimator.SetTrigger("Shoot");
        }

        [Command]
        void CmdAnimate()
        {
            RpcAnimate();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcAnimate()
        {
            if (bodyAnimator != null)
            {
                bodyAnimator.SetFloat(anim_velocity_x, moveInput.x);
                bodyAnimator.SetFloat(anim_velocity_y, moveInput.z);
            }
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
