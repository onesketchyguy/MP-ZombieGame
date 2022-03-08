using System.Linq;
using UnityEngine;

namespace ZombieGame
{
    public class SpectatorCam : MonoBehaviour
    {
        [SerializeField] private float lerpSpd = 5.0f;
        private Transform target = null;

        [SerializeField] private Transform _camera = null;
        [Space]
        [SerializeField] private GameObject followCam = null;
        [SerializeField] private GameObject levelCam = null;
        [SerializeField] private string levelCamTag = "LevelCam";

        private void Start()
        {
            if (_camera.parent == transform) _camera.SetParent(null);

            if (levelCam == null && followCam != null)
            {
                var list = GameObject.FindGameObjectsWithTag(levelCamTag);
                levelCam = list.FirstOrDefault();
            }
        }

        private void Update()
        {
            if (Time.frameCount % 3 == 0) return;

            if (target == null)
            {
                var players = AiBlackboard.GetPlayers();

                if (players != null && players.Length > 0)
                {
                    target = players[0].transform;
                }

                if (levelCam != null && followCam != null)
                {
                    followCam.SetActive(false);
                    levelCam.SetActive(true);
                }
            }
            else
            {
                transform.position = Vector3.Slerp(transform.position, target.position, lerpSpd * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, lerpSpd * Time.deltaTime);

                if (levelCam != null && followCam != null)
                {
                    followCam.SetActive(true);
                    levelCam.SetActive(false);
                }
            }
        }
    }
}
