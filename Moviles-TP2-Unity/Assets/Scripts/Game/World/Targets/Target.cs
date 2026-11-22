using UnityEngine;

namespace Game.World.Targets
{
    public class Target : MonoBehaviour, IGoResetable
    {
        [SerializeField] private ObstacleScore obstacleScore;

        private LODGroup lodGroup;
        private Renderer[] renderers;
        private bool renderersCached;
        private bool isRenderCulled;

        public int PrefabId { get; set; }
        public bool IsRenderCulled => isRenderCulled;

        private void Awake()
        {
            CacheVisualComponents();
        }

        private void CacheVisualComponents()
        {
            if (renderersCached) return;
            lodGroup = GetComponentInChildren<LODGroup>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            renderersCached = true;
        }

        public float GetBoundingRadius()
        {
            CacheVisualComponents();
            if (lodGroup != null)
            {
                Vector3 s = transform.lossyScale;
                float maxScale = Mathf.Max(s.x, Mathf.Max(s.y, s.z));
                return Mathf.Max(lodGroup.size * maxScale * 0.75f, 15f);
            }

            return 30f;
        }

        public void SetRenderVisible(bool visible)
        {
            CacheVisualComponents();
            isRenderCulled = !visible;

            if (lodGroup != null)
                lodGroup.enabled = visible;

            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }

        public void Restart()
        {
            SetRenderVisible(true);
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
            obstacleScore.AddScore();

            SetRenderVisible(true);
            gameObject.SetActive(true);
        }
    }
}
