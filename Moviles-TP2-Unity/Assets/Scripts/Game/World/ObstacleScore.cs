using System.Collections.Generic;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World
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
                Scores.Remove(this);
            
            if (Scores.Count <= 0)
                EventSystem.Raise<GameOverEvent>();
        }
        
        private void Start()
        {
            Scores.Add(this);
            Scores.Sort((a, b) => a.order.CompareTo(b.order));
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
