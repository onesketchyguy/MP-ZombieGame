using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Mirror.Examples.Tanks;

namespace ZombieGame
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Components")]
        public TextMesh healthBar;
        public Animator handsAnimator;

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
        private bool setLayers = false;

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
            }

            SetLayers();
        }

        private void SetLayers()
        {
            if (setLayers == true) return;
            setLayers = true;

            if (isLocalPlayer)
            {
                int hideLayer = LayerMask.NameToLayer(hideFromClient);
                int showLayer = LayerMask.NameToLayer(showClient);

                foreach (var item in body.GetComponentsInChildren<Transform>())
                    item.gameObject.layer = hideLayer;

                foreach (var item in arms.GetComponentsInChildren<Transform>())
                    item.gameObject.layer = showLayer;
            }
            else
            {
                int hideLayer = LayerMask.NameToLayer(showClient);
                int showLayer = LayerMask.NameToLayer(hideFromClient);

                foreach (var item in body.GetComponentsInChildren<Transform>())
                    item.gameObject.layer = hideLayer;

                foreach (var item in arms.GetComponentsInChildren<Transform>())
                    item.gameObject.layer = showLayer;
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
