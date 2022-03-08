using UnityEngine;
using Mirror;
using Mirror.Examples.Tanks;

namespace ZombieGame
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Components")]
        public TextMesh healthBar;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public GameObject projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        [Header("Character setup")]
        [SerializeField] private FPS.FPSController fpsController;
        [SerializeField] private FPS.FPSArmsController fpsArms;
        [SerializeField] private CharacterController characterController;

        [Header("Client setup")]
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject arms;
        [Space]
        [SerializeField] private string hideFromClient = "LocalPlayer";
        [SerializeField] private string showClient = "ClientPlayer";
        private bool setLayers = false;

        private Vector3 moveInput;
        private Vector2 lookInput;
        private float runInput;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (characterController == null) characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().clientAuthority = true;
        }
#endif

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        private void OnEnable() => AiBlackboard.RegisterPlayer(this);
        private void OnDisable() => AiBlackboard.DeregisterPlayer(this);

        void Update()
        {
            // always update health bar.
            // (SyncVar hook would only update on clients, not on server)
            healthBar.text = new string('-', health);

            // movement for local player
            if (isLocalPlayer)
            {
                // shoot
                if (Input.GetMouseButtonDown(0))
                {
                    if (fpsArms.FireWeapon() == true) CmdFire();
                }

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

            // FIXME: Interpret if it's a skinned mesh renderer or mesh renderer

            var bodyMesh = body.GetComponentsInChildren<SkinnedMeshRenderer>();
            var armsMesh = arms.GetComponentsInChildren<SkinnedMeshRenderer>();

            //int bodyLayer = isLocalPlayer ? LayerMask.NameToLayer(hideFromClient) : LayerMask.NameToLayer(showClient);
            int armsLayer = isLocalPlayer ? LayerMask.NameToLayer(showClient) : LayerMask.NameToLayer(hideFromClient);

            if (isLocalPlayer)
            {
                if (bodyMesh.Length == 0) Debug.LogError("No body mesh!");

                foreach (var item in bodyMesh)
                {
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    Debug.Log($"{item.name} set to {item.shadowCastingMode}");
                }
            }

            foreach (var item in armsMesh) item.gameObject.layer = armsLayer;
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
            NetworkServer.Spawn(projectile);
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
