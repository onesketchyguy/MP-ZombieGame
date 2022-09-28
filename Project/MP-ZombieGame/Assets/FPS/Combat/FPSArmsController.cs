using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class FPSArmsController : MonoBehaviour
    {
        private FPSControls.PlayerActions inputActions;
        [SerializeField] private Animator anim;
        [SerializeField] private string animAimFloat = "Aim";
        [SerializeField] private string animReloadTrigger = "Reload";
        public uint curAmmo
        {
            get
            {
                if (weaponIndex >= 0 && weaponIndex < weapons.Count)
                    return weapons[weaponIndex].ammo;

                return 0;
            }
            private set
            {
                if (weaponIndex >= 0 && weaponIndex < weapons.Count)
                {
                    var weaponData = weapons[weaponIndex];
                    weaponData.ammo = value;
                    weapons[weaponIndex] = weaponData;
                } else Debug.LogError("No weapon to modify ammo!");
            }
        }

        [SerializeField] private List<GameObject> weaponObjects = new List<GameObject>();
        [SerializeField] private Transform weaponParent;
        private List<Weapon> weapons = new List<Weapon>();
        private int weaponIndex = -1;

        [SerializeField] private UnityEngine.UI.Graphic crossheir = null;

        [SerializeField] private bool playParticle = true;
        private ParticleSystem ps = null;

        [SerializeField] internal AnimationClip reloadClip = null; // FIXME: Assign clip based on weapon
        private float reloadTime;

        [SerializeField] private float moveToAimSpd = 10.0f;
        private bool aiming = false;

        [SerializeField] private UnityEngine.Events.UnityEvent onFire;

        public bool GetReloading() => (reloadTime > Time.time);

        public bool GetAiming() => aiming;

        private void OnEnable()
        {
            inputActions = FPSInputManager.GetPlayerInput();
            inputActions.Aim.performed += _ => Aim(true);
            inputActions.Aim.canceled += _ => Aim(false);
            inputActions.Reload.performed += _ => ReloadWeapon();

            ReloadWeapon();
        }

        private void OnDisable()
        {
            inputActions.Aim.performed -= _ => Aim(true);
            inputActions.Aim.canceled -= _ => Aim(false);
            inputActions.Reload.performed -= _ => ReloadWeapon();
        }

        private void Update()
        {
            if (FPSInputManager.enabled == false)
            {
                Debug.LogError("Forced to reenable input");
                FPSInputManager.Enable();
                return;
            }

            if (anim == null || anim.gameObject.activeSelf == false) return;
            Debug.Log($"{gameObject.name} Anim set: {anim.GetFloat(animAimFloat)}");

            float aimValue = anim.GetFloat(animAimFloat);
            anim.SetFloat(animAimFloat, Mathf.Lerp(aimValue, aiming == true ? 1 : 0, moveToAimSpd * Time.deltaTime));

            crossheir.color = Color.Lerp(crossheir.color, 
                new Color(crossheir.color.r, crossheir.color.g, crossheir.color.b, (aiming == true ? 0 : 1) * 255.0f), aimValue);
        }

        public void PickupWeapon(WeaponObject weaponObject)
        {
            if (weaponIndex != -1)
            {
                weaponObjects[weaponIndex].SetActive(false);
            }

            weapons.Add(weaponObject.data);
            EquipWeapon(weapons.Count - 1);
        }

        public void EquipWeapon(int index)
        {
            weaponIndex = index;

            weaponObjects.Add(Instantiate(weapons[weaponIndex].weaponPrefab, weaponParent));
            weaponObjects[weaponIndex].SetActive(true);

            anim = weaponObjects[weaponIndex].GetComponentInChildren<Animator>();

            if (playParticle) ps = weaponObjects[weaponIndex].GetComponentInChildren<ParticleSystem>();
        }

        public void Aim(bool value)
        {
            aiming = value;
        }

        public bool FireWeapon()
        {
            if (reloadTime > Time.time) return false;

            if (curAmmo <= 0)
            {
                ReloadWeapon();
                return false;
            }

            curAmmo--;

            if (playParticle && ps != null) ps.Play();

            onFire?.Invoke();

            return true;
        }

        public void ReloadWeapon()
        {
            if (GetReloading()) return;

            anim.SetTrigger(animReloadTrigger);

            reloadTime = Time.time + reloadClip.length;

            Invoke(nameof(ResetAmmo), reloadClip.length);
        }

        public void ResetAmmo()
        {
            curAmmo = 7; // FIXME: Use magazine size
        }
    }
}
