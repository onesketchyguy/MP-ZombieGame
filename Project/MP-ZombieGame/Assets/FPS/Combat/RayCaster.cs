using System.Collections;
using UnityEngine;

namespace FPS
{
    public class RayCaster : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image hitMarker = null;
        [SerializeField] private float hitMarkerDecaySpeed = 50.0f;

        [SerializeField] public UnityEngine.Events.UnityEvent onHitEvent;
        [SerializeField] public UnityEngine.Events.UnityEvent<int> onKillEvent;

        private void Start()
        {
            hitMarker.color = new Color(hitMarker.color.r, hitMarker.color.g, hitMarker.color.b, 0.0f);
        }

        public bool Cast(float damage)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

                var damagable = hit.transform.gameObject.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    bool killedTarget = false;
                    int s = damagable.GetKillScore();
                    damagable.RecieveDamage(damage, ref killedTarget);

                    if (killedTarget) onKillEvent?.Invoke(s);

                    if (hitMarker != null && gameObject.activeSelf)
                    {
                        StartCoroutine(AnimateHitMarker());
                    }

                    onHitEvent?.Invoke();

                    return true;
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            }

            return false;
        }

        private IEnumerator AnimateHitMarker()
        {
            hitMarker.color = new Color(hitMarker.color.r, hitMarker.color.g, hitMarker.color.b, 0.0f);
            yield return null;
            hitMarker.color = new Color(hitMarker.color.r, hitMarker.color.g, hitMarker.color.b, 255.0f);
            yield return null;

            while (hitMarker.color.a > 10)
            {
                hitMarker.color = Color.Lerp(hitMarker.color, 
                    new Color(hitMarker.color.r, hitMarker.color.g, hitMarker.color.b, 0.0f),
                    hitMarkerDecaySpeed * Time.deltaTime);
                yield return null;
            }

            hitMarker.color = new Color(hitMarker.color.r, hitMarker.color.g, hitMarker.color.b, 0.0f);
            yield return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 10);
        }
    }
}