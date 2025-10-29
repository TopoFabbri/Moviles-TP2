using System.Collections.Generic;
using Game.Events;
using Game.World.Targets;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        private const int MaxCollisions = 10;

        [SerializeField] private BoxCollider box;
        [SerializeField] private LayerMask mask;

        private readonly Collider[] collisions = new Collider[MaxCollisions];
        private List<ObstacleScore> scoresInContact = new();

        private void Update()
        {
            List<ObstacleScore> actualScoresInContact = new();

            for (int i = 0;
                 i < Physics.OverlapBoxNonAlloc(box.bounds.center, box.bounds.size / 2f, collisions, transform.rotation, mask.value, QueryTriggerInteraction.Collide);
                 i++)
            {
                if (!collisions[i].enabled) return;

                if (collisions[i].TryGetComponent(out ObstacleScore obstacleScore))
                {
                    if (!actualScoresInContact.Contains(obstacleScore))
                        actualScoresInContact.Add(obstacleScore);
                }
                else
                {
                    EventSystem.Raise<DroneCollisionEvent>();
                    break;
                }
            }

            foreach (ObstacleScore score in scoresInContact)
            {
                if (!actualScoresInContact.Contains(score))
                    score.MarkDone();
            }

            scoresInContact = actualScoresInContact;
        }
    }
}