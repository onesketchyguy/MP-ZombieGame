using UnityEngine;
using Mirror;
using FPS;
using FPS.Inventory;
using TMPro;

namespace ZombieGame
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Components")]
        public TextMesh healthBar;
        [SerializeField] private TextMeshProUGUI scoreDisplay = null;
        [SerializeField] private TextMeshProUGUI healthDisplay = null;
        [SerializeField] private UnityEngine.UI.Slider healthSlider = null;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        [SerializeField] private FPSArmsController fpsArms;
        [SerializeField] private RayCaster rayCaster;
        [SerializeField] private UnityEngine.Events.UnityEvent onFire;
        [SerializeField] private WeaponObject startWeapon;

        [Header("Stats")]
        [SyncVar] public int score = 0;
        [SyncVar] public int health = 4;
        [SyncVar] public int maxHealth = 4;

        [Header("Character setup")]
        [SerializeField] private FPSController fpsController;
        [SerializeField] private InventoryManager inventory;
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

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            FPSInputManager.Enable();
            fpsController.SetMouseLocked(true);

            CmdInit();
        }

        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            FPSInputManager.Disable();
            fpsController.SetMouseLocked(false);
        }

        [Command]
        private void CmdInit()
        {
            RpcInit();
        }

        [ClientRpc]
        private void RpcInit()
        {
            health = maxHealth;
            rayCaster.onKillEvent.AddListener((v) => { score += v; Debug.Log($"netID({netId}) recieving {v} score"); });
            int id = inventory.GetItemID(startWeapon.idObj);
            inventory.EquipItem(id);
        }

        private void OnEnable() => AiBlackboard.RegisterPlayer(this);
        private void OnDisable() => AiBlackboard.DeregisterPlayer(this);

        public void Start()
        {
            if (isLocalPlayer)
            {
                fpsArms.PickupWeapon(startWeapon);
                scoreDisplay.text = 0.ToString();
            }
        }

        void Update()
        {
            // always update health bar.
            // (SyncVar hook would only update on clients, not on server)
            healthBar.text = $"{health} / {maxHealth}";

            // movement for local player
            if (isLocalPlayer)
            {
                // UI
                int oldScore = int.Parse(scoreDisplay.text);
                int newScore = oldScore < score ? oldScore + 1 : oldScore > score ? oldScore - 1 : score;
                scoreDisplay.text = $"{newScore}";

                healthDisplay.text = $"{health} / {maxHealth}";
                healthSlider.value = health / (float)maxHealth;
                healthSlider.maxValue = 1.0f;

                // shoot
                if (Input.GetMouseButtonDown(0))
                {
                    if (fpsArms.FireWeapon() == true) CmdFire(fpsArms.GetActiveWeapon().damage);
                }

                moveInput.x = Input.GetAxisRaw("Horizontal");
                moveInput.z = Input.GetAxisRaw("Vertical");
                moveInput.y = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

                runInput = Mathf.Lerp(fpsController.GetRunInput(),
                                Input.GetKey(KeyCode.LeftShift) ? 1.1f : -0.1f, 10.0f * Time.deltaTime);

                runInput = Mathf.Clamp01(runInput);

                lookInput.x = Input.GetAxisRaw("Mouse X");
                lookInput.y = Input.GetAxisRaw("Mouse Y");

                fpsController.SetMoveInput(moveInput, runInput).SetLookInput(lookInput);
            }

            SetLayers();
        }

        private void SetLayers()
        {
            if (setLayers == true) return;
            setLayers = true;

            // FIXME: Interpret if it's a skinned mesh renderer or mesh renderer

            var skinnedMesh = body.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bodyMesh = body.GetComponentsInChildren<MeshRenderer>();

            if (isLocalPlayer)
            {
                if (skinnedMesh.Length == 0) Debug.LogError("No skinned body mesh!");

                foreach (var item in skinnedMesh)
                {
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    Debug.Log($"{item.name} set to {item.shadowCastingMode}");
                }

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
        private void CmdFire(int damage)
        {
            rayCaster.Cast(damage);
            RpcFire();
        }

        [ClientRpc]
        private void RpcFire()
        {
            onFire?.Invoke();
        }

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            var damager = other.GetComponent<PhysicalDamager>();
            if (damager != null)
            {
                health -= damager.damage;
                if (health <= 0) NetworkServer.Destroy(gameObject); // Died
            }
        }
    }
}
