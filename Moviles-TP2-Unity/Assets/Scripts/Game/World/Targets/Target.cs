using Tools.Pool;
using UnityEngine;

namespace Game.World.Targets
{
    public class Target : MonoBehaviour, IResetable
    {
        [SerializeField] private ObstacleScore obstacleScore;
        
        public void Reset()
        {
            gameObject.SetActive(false);
        }

        public void Assign(params object[] parameters)
        {
            Transform parent = parameters[0] as Transform;
            Vector3 position = (Vector3)parameters[1];
            Quaternion rotation = (Quaternion)parameters[2];
            Vector3 scale = (Vector3)parameters[3];
            int order = (int)parameters[4];
            
            transform.parent = parent;
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;

            obstacleScore.SetOrder(order);
            
            gameObject.SetActive(true);
        }
    }
}
