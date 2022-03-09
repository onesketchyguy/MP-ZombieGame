using System.Collections;
using UnityEngine;

namespace FPS
{
    public class RayCaster : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image hitMarker = null;
        [SerializeField] private float hitMarkerDecaySpeed = 50.0f;

        public void Cast(float damage)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

                var damagable = hit.transform.gameObject.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    damagable.RecieveDamage(damage);

                    if (hitMarker != null)
                    {
                        StartCoroutine(OnHit());
                    }
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            }
        }

        private IEnumerator OnHit()
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