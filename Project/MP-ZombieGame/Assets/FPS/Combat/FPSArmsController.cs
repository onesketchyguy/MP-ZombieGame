using UnityEngine;

namespace FPS
{
    public class FPSArmsController : MonoBehaviour
    {
        private FPSControls.PlayerActions inputActions;
        [SerializeField] private Animator anim;
        [SerializeField] private string animAimFloat = "Aim";
        [SerializeField] private string animReloadTrigger = "Reload";
        private uint currentWeaponAmmo = 0;

        [SerializeField] internal AnimationClip reloadClip = null; // FIXME: Assign clip based on weapon
        private float reloadTime;

        [SerializeField] private float moveToAimSpd = 10.0f;
        private bool aiming = false;

        public uint GetBulletCount()
        {
            return currentWeaponAmmo;
        }

        public bool GetReloading()
        {
            return (reloadTime > Time.time);
        }

        private void OnEnable()
        {
            FPSInputManager.Enable();
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
            FPSInputManager.Disable();
        }

        private void Update()
        {
            if (anim.gameObject.activeSelf == false) return;

            float aimValue = anim.GetFloat(animAimFloat);
            anim.SetFloat(animAimFloat, Mathf.Lerp(aimValue, aiming == true ? 1 : 0, moveToAimSpd * Time.deltaTime));
        }

        private void Aim(bool value)
        {
            aiming = value;
            Debug.Log($"Aiming: {value}");
        }

        public bool FireWeapon()
        {
            if (reloadTime > Time.time) return false;

            if (currentWeaponAmmo <= 0)
            {
                ReloadWeapon();
                return false;
            }

            currentWeaponAmmo--;

            return true;
        }

        private void ReloadWeapon()
        {
            anim.SetTrigger(animReloadTrigger);

            reloadTime = Time.time + reloadClip.length;

            Invoke(nameof(ResetAmmo), reloadClip.length);
        }

        public void ResetAmmo()
        {
            currentWeaponAmmo = 7; // FIXME: Use magazine size
        }
    }
}
