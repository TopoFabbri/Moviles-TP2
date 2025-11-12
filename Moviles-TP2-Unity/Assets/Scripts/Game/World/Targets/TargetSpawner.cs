using System.Collections.Generic;
using Tools.Pool;
using UnityEditor;
using UnityEngine;

namespace Game.World.Targets
{
    internal sealed class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private List<SpawnPoint> spawnPoints = new();

        [Header("Debug")]
        [SerializeField] private float maxDistance = 50f;
        
        private static bool _drawPath;

        private readonly GoPool<Target> pool = new();
        private readonly List<Target> targets = new();

        private void Awake()
        {
            spawnPoints.Clear();
            GetComponentsInChildren(spawnPoints);
        }

        internal void Spawn()
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Vector3 prevPos = spawnPoints[i].transform.position;
                Vector3 nextPos = spawnPoints[i].transform.position;

                if (i > 0)
                    prevPos = spawnPoints[i - 1].transform.position;

                if (i < spawnPoints.Count - 1)
                    nextPos = spawnPoints[i + 1].transform.position;

                Vector3 lookAt = nextPos - prevPos;
                Quaternion rot = Quaternion.LookRotation(lookAt);

                targets.Add(pool.Get(spawnPoints[i].prefab, transform, spawnPoints[i].transform.position, rot, spawnPoints[i].prefab.transform.localScale, i, spawnPoints[i].prefab));
                spawnPoints[i].gameObject.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            foreach (Target target in targets)
            {
                pool.Release(target);
                Destroy(target.gameObject);
            }

            targets.Clear();
            spawnPoints.Clear();
        }

        internal void Restart()
        {
            foreach (Target target in targets)
                pool.Release(target);
            
            targets.Clear();
            ObstacleScore.ResetScores();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawPath || Application.isPlaying) return;

            for (int i = 0; i < spawnPoints.Count - 1; i++)
            {
                if (!spawnPoints[i] || !spawnPoints[i + 1]) continue;

                Vector3 curPos = spawnPoints[i].transform.position;
                Vector3 nextPos = spawnPoints[i + 1].transform.position;

                float t = Mathf.Clamp(Vector3.Distance(curPos, nextPos), 0, maxDistance) / maxDistance;
                Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                
                Gizmos.DrawLine(curPos, nextPos);
            }
        }

        [MenuItem("Tools/Toggle draw path")]
        private static void ToggleDrawPath()
        {
            if (Application.isPlaying) return;

            _drawPath = !_drawPath;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            GetComponentsInChildren(spawnPoints);
        }
#endif
    }
}