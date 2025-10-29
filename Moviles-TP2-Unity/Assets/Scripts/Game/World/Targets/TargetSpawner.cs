using System.Collections.Generic;
using Tools.Pool;
using UnityEngine;

namespace Game.World.Targets
{
    internal sealed class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private List<SpawnPoint> spawnPoints = new();
        private readonly GoPool<Target> pool = new();
        private readonly List<Target> targets = new();
        
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
                
                targets.Add(pool.Get(spawnPoints[i].prefab, transform, spawnPoints[i].transform.position, rot, spawnPoints[i].prefab.transform.localScale, i));
                Destroy(spawnPoints[i].gameObject);
            }
            
            spawnPoints.Clear();
        }

        private void OnDestroy()
        {
            foreach (Target target in targets)
            {
                Destroy(target.gameObject);
                pool.Release(target);
            }
            
            targets.Clear();
            spawnPoints.Clear();
        }
    }
}
