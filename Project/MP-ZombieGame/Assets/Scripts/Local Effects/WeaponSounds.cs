using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieShooter.LocalEffects
{
    public class WeaponSounds : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource = null;
        [SerializeField] private AudioClip loadClip = null;
        [SerializeField] private AudioClip unloadClip = null;

        public void Load() => audioSource.PlayOneShot(loadClip);
        public void Unload() => audioSource.PlayOneShot(unloadClip);
    }
}