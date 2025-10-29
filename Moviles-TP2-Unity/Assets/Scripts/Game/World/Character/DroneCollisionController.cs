using Game.Events;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        [SerializeField] private BoxCollider box;
        [SerializeField] private LayerMask mask;
        
        private readonly Collider[] collisions = new Collider[10];

        private void Update()
        {
            for (int i = 0; i < Physics.OverlapBoxNonAlloc(box.bounds.center, box.bounds.size / 2f, collisions, transform.rotation, mask.value, QueryTriggerInteraction.Collide); i++)
            {
                if (!collisions[i].enabled) continue;
                
                EventSystem.Raise<DroneCollisionEvent>();
                return;
            }
        }
    }
}
