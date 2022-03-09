using UnityEngine;

namespace FPS
{
    public class BulletCounter : MonoBehaviour
    {
        [SerializeField] private FPSArmsController armsController = null;

        [SerializeField] private UnityEngine.UI.Image bulletImage = null;
        [SerializeField] private int width = 100;
        [SerializeField] private int maxWidth = 800;

        [Header("Images")]
        [SerializeField] private Sprite bulletSpr = null;
        [SerializeField] private Sprite reloadSpr = null;
        [SerializeField] private Sprite reloadingSpr = null;

        private Vector2 startPivot;

        private void Start()
        {
            startPivot = bulletImage.rectTransform.pivot;
        }

        private void Update()
        {
            if (armsController.GetReloading())
            {
                if (bulletImage.rectTransform.pivot == startPivot)
                {
                    var rect = bulletImage.rectTransform.sizeDelta;
                    bulletImage.rectTransform.sizeDelta = new Vector2(width, rect.y);
                    bulletImage.rectTransform.pivot = Vector2.one * 0.5f;
                    bulletImage.sprite = reloadingSpr;
                }

                bulletImage.rectTransform.Rotate(bulletImage.rectTransform.forward, 100.0f * Time.deltaTime);
            }
            else
            {
                if (armsController.GetBulletCount() > 0)
                {
                    bulletImage.rectTransform.pivot = startPivot;

                    var rect = bulletImage.rectTransform.sizeDelta;
                    float w = width * armsController.GetBulletCount() % maxWidth;
                    float h = width * armsController.GetBulletCount() / maxWidth;
                    h += width;

                    bulletImage.rectTransform.sizeDelta = new Vector2(w, h);
                    bulletImage.rectTransform.rotation = Quaternion.identity;

                    bulletImage.sprite = bulletSpr;
                }
                else
                {
                    bulletImage.rectTransform.pivot = startPivot;

                    var rect = bulletImage.rectTransform.sizeDelta;
                    bulletImage.rectTransform.sizeDelta = new Vector2(width, rect.y);
                    bulletImage.sprite = reloadSpr;
                }
            }
        }
    }
}
