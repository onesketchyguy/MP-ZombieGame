using UnityEngine;

namespace ZombieShooter.LocalEffects
{
    public class RandomClipPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips = null;
        [SerializeField] private AudioSource audioSource = null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }
#endif

        public void PlayClip()
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}