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
        [SerializeField] private FPS.FPSArmsController fpsArms;
        [SerializeField] private FPS.RayCaster rayCaster;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        [Header("Character setup")]
        [SerializeField] private FPS.FPSController fpsController;
        [SerializeField] private CharacterController characterController;

        [Header("Client setup")]
        [SerializeField] private GameObject body;
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

            if (isLocalPlayer)
            {
                if (bodyMesh.Length == 0) Debug.LogError("No body mesh!");

                foreach (var item in bodyMesh)
                {
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    Debug.Log($"{item.name} set to {item.shadowCastingMode}");
                }
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            rayCaster.Cast(1); // FIXME: Use weapon damage
        }

        //[ServerCallback]
        //void OnTriggerEnter(Collider other)
        //{
        //    if (other.GetComponent<Projectile>() != null)
        //    {
        //        --health;
        //        if (health == 0)
        //            NetworkServer.Destroy(gameObject);
        //    }
        //}
    }
}
