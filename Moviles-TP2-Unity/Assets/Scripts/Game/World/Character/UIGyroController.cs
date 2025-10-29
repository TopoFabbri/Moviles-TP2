using Game.World.Targets;
using UnityEngine;

namespace Game.World.Character
{
    public class UIGyroController : MonoBehaviour
    {
        [SerializeField] private Transform gyro;
        [SerializeField] private Transform pointer;
        [SerializeField] private Transform droneTransform;
        
        private void Update()
        {
            gyro.rotation = Quaternion.identity;
            
            pointer.rotation = Quaternion.LookRotation(ObstacleScore.Current.transform.position - droneTransform.position, droneTransform.up);
        }
    }
}
