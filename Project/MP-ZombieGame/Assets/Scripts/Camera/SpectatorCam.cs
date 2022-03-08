using UnityEngine;

namespace ZombieGame
{
    public class SpectatorCam : MonoBehaviour
    {
        [SerializeField] private float lerpSpd = 5.0f;
        private Transform target = null;

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
            }
            else
            {
                transform.position = Vector3.Slerp(transform.position, target.position, lerpSpd * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, lerpSpd * Time.deltaTime);
            }
        }
    }
}
