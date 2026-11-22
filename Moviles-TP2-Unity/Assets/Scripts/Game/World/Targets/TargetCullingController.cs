using System.Collections.Generic;
using UnityEngine;

namespace Game.World.Targets
{
    /// <summary>
    /// Manages runtime frustum and distance culling for dynamically pooled <see cref="Target"/> instances
    /// using Unity's <see cref="CullingGroup"/> API while keeping physics colliders active.
    /// </summary>
    public sealed class TargetCullingController : MonoBehaviour
    {
        [SerializeField] private float maxRenderDistance = 260f;

        private CullingGroup cullingGroup;
        private BoundingSphere[] boundingSpheres;
        private readonly List<Target> trackedTargets = new();
        private Camera targetCamera;

        public float MaxRenderDistance
        {
            get => maxRenderDistance;
            set => maxRenderDistance = Mathf.Max(10f, value);
        }

        public int TrackedCount => trackedTargets.Count;

        public void Initialize(IReadOnlyList<Target> targets, Camera cam = null)
        {
            DisposeCullingGroup();
            trackedTargets.Clear();

            if (targets == null || targets.Count == 0)
                return;

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                    trackedTargets.Add(targets[i]);
            }

            if (trackedTargets.Count == 0)
                return;

            targetCamera = cam != null ? cam : Camera.main;
            boundingSpheres = new BoundingSphere[trackedTargets.Count];
            for (int i = 0; i < trackedTargets.Count; i++)
            {
                Target t = trackedTargets[i];
                boundingSpheres[i] = new BoundingSphere(t.transform.position, t.GetBoundingRadius());
            }

            if (targetCamera != null)
                SetupCullingGroup();

            EvaluateManualFallback(targetCamera);
        }

        private void SetupCullingGroup()
        {
            DisposeCullingGroup();
            cullingGroup = new CullingGroup
            {
                targetCamera = targetCamera
            };
            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingSphereCount(trackedTargets.Count);
            cullingGroup.SetBoundingDistances(new[] { maxRenderDistance });
            cullingGroup.SetDistanceReferencePoint(targetCamera.transform);
            cullingGroup.onStateChanged = OnCullingStateChanged;
        }

        private void OnCullingStateChanged(CullingGroupEvent evt)
        {
            if (evt.index < 0 || evt.index >= trackedTargets.Count)
                return;

            Target target = trackedTargets[evt.index];
            if (target == null)
                return;

            bool visible = evt.isVisible && evt.currentDistance == 0;
            target.SetRenderVisible(visible);
        }

        public void EvaluateManualFallback(Camera cam = null)
        {
            Camera activeCam = cam != null ? cam : (targetCamera != null ? targetCamera : Camera.main);
            if (activeCam == null || trackedTargets.Count == 0)
                return;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(activeCam);
            Vector3 camPos = activeCam.transform.position;

            for (int i = 0; i < trackedTargets.Count; i++)
            {
                Target t = trackedTargets[i];
                if (t == null) continue;

                float radius = boundingSpheres != null && i < boundingSpheres.Length
                    ? boundingSpheres[i].radius
                    : t.GetBoundingRadius();

                Vector3 pos = t.transform.position;
                float dist = Vector3.Distance(camPos, pos) - radius;
                bool withinDistance = dist <= maxRenderDistance;

                Bounds sphereBounds = new Bounds(pos, Vector3.one * (radius * 2f));
                bool inFrustum = GeometryUtility.TestPlanesAABB(planes, sphereBounds);

                t.SetRenderVisible(withinDistance && inFrustum);
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null && trackedTargets.Count > 0)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    targetCamera = mainCam;
                    SetupCullingGroup();
                    EvaluateManualFallback(targetCamera);
                }
            }
        }

        public void Clear()
        {
            DisposeCullingGroup();
            for (int i = 0; i < trackedTargets.Count; i++)
            {
                if (trackedTargets[i] != null)
                    trackedTargets[i].SetRenderVisible(true);
            }

            trackedTargets.Clear();
            boundingSpheres = null;
        }

        private void DisposeCullingGroup()
        {
            if (cullingGroup != null)
            {
                cullingGroup.Dispose();
                cullingGroup = null;
            }
        }

        private void OnDisable()
        {
            DisposeCullingGroup();
        }

        private void OnDestroy()
        {
            DisposeCullingGroup();
        }
    }
}
