using Game.World.Targets;
using UnityEngine;

namespace Game.World.Character
{
    public class UIGyroController : MonoBehaviour
    {
        [SerializeField] private Transform gyro;
        [SerializeField] private Transform pointer;
        [SerializeField] private Transform droneTransform;
        [SerializeField] private Camera overlayCam;
        [SerializeField] private Vector2 relativePos = new(0.8f, 0.8f);

        private void Update()
        {
            gyro.rotation = Quaternion.identity;

            float posX = overlayCam.rect.x + overlayCam.rect.width * relativePos.x;
            float posY = overlayCam.rect.y + overlayCam.rect.height * relativePos.y;
            gyro.position = overlayCam.ViewportToWorldPoint(new Vector3(posX, posY, 10f));
            
            if (ObstacleScore.Current)
                pointer.rotation = Quaternion.LookRotation(ObstacleScore.Current.transform.position - droneTransform.position, droneTransform.up);
        }
    }
}