using System.Collections.Generic;
using Game.Events;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.Targets
{
    internal class ObstacleScore : MonoBehaviour
    {
        private static readonly List<ObstacleScore> Scores = new();
        internal static ObstacleScore Current => Scores.Count > 0 ? Scores[0] : null;

        [SerializeField] private int order;
        [SerializeField] private GameObject marker;

        internal void MarkDone()
        {
            if (Current == this)
            {
                Scores.Remove(this);
                ServiceProvider.GetService<EventSystem>().Raise<RingPassedEvent>(transform.position, Scores.Count);
            }
            
            if (Scores.Count <= 0)
                ServiceProvider.GetService<EventSystem>().Raise<GameOverEvent>();
        }

        internal static void SortScores()
        {
            Scores.Sort((a, b) => a.order.CompareTo(b.order));
        }
        
        internal void SetOrder(int order)
        {
            this.order = order;
        }
        
        internal static void ResetScores()
        {
            Scores.Clear();
        }
        
        internal void AddScore()
        {
            Scores.Add(this);
        }

        private void OnDestroy()
        {
            Scores.Remove(this);
        }

        private void Update()
        {
            marker.SetActive(Current == this);
        }
    }
}
