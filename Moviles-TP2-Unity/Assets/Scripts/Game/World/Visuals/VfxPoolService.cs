using System.Collections;
using TMPro;
using Tools.Tween;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Game.World.Visuals
{
    public class VfxPoolService : MonoBehaviour
    {
        private static VfxPoolService _instance;
        private static Material _particleMat;

        private ObjectPool<ParticleSystem> _ringBurstPool;
        private ObjectPool<ParticleSystem> _crashBurstPool;
        private ObjectPool<ParticleSystem> _confettiPool;
        private ObjectPool<TextMeshPro> _floatingTextPool;
        private ParticleSystem _ambientMotes;

        public static VfxPoolService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(VfxPoolService));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<VfxPoolService>();
                    _instance.InitializePools();
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HookSceneLoad()
        {
            SceneManager.sceneLoaded += (_, _) => Instance.EnsureAmbientMotes();
            Instance.EnsureAmbientMotes();
        }

        private static Material GetParticleMaterial()
        {
            if (_particleMat == null)
            {
                Shader s = Shader.Find("Sprites/Default");
                if (s != null)
                {
                    _particleMat = new Material(s)
                    {
                        name = "VfxPool_SpritesDefault"
                    };
                }
            }

            return _particleMat;
        }

        private void InitializePools()
        {
            _ringBurstPool = new ObjectPool<ParticleSystem>(
                () => CreateBurstSystem("RingBurst", 12, 0.55f, 3.5f, ParticleSystemShapeType.Sphere, Palette.Accent, Palette.RingSecondary, 0f),
                ps => ps.gameObject.SetActive(true),
                ps => ps.gameObject.SetActive(false),
                ps => { if (ps != null) Destroy(ps.gameObject); },
                false, 4, 16);

            _crashBurstPool = new ObjectPool<ParticleSystem>(
                () => CreateBurstSystem("CrashBurst", 16, 0.45f, 5.0f, ParticleSystemShapeType.Sphere, Palette.RingPrimary, Palette.Accent, 0.2f),
                ps => ps.gameObject.SetActive(true),
                ps => ps.gameObject.SetActive(false),
                ps => { if (ps != null) Destroy(ps.gameObject); },
                false, 2, 8);

            _confettiPool = new ObjectPool<ParticleSystem>(
                () => CreateConfettiSystem(),
                ps => ps.gameObject.SetActive(true),
                ps => ps.gameObject.SetActive(false),
                ps => { if (ps != null) Destroy(ps.gameObject); },
                false, 2, 6);

            _floatingTextPool = new ObjectPool<TextMeshPro>(
                CreateFloatingText,
                tmp => tmp.gameObject.SetActive(true),
                tmp => tmp.gameObject.SetActive(false),
                tmp => { if (tmp != null) Destroy(tmp.gameObject); },
                false, 4, 16);
        }

        private ParticleSystem CreateBurstSystem(
            string goName,
            int count,
            float lifetime,
            float speed,
            ParticleSystemShapeType shapeType,
            Color startColor,
            Color endColor,
            float gravity)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = 0.28f;
            main.gravityModifier = gravity;
            main.playOnAwake = false;
            main.startColor = startColor;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = 0.35f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = GetParticleMaterial();

            return ps;
        }

        private ParticleSystem CreateConfettiSystem()
        {
            var go = new GameObject("MilestoneConfetti");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 1.2f;
            main.loop = false;
            main.startLifetime = 1.2f;
            main.startSpeed = 7.5f;
            main.startSize = 0.22f;
            main.gravityModifier = 0.8f;
            main.playOnAwake = false;
            main.startColor = new ParticleSystem.MinMaxGradient(Palette.Accent, Palette.CharacterPrimary);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 32f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(Palette.Accent, 0f),
                    new GradientColorKey(Palette.CharacterPrimary, 0.5f),
                    new GradientColorKey(Palette.RingPrimary, 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = GetParticleMaterial();

            return ps;
        }

        private TextMeshPro CreateFloatingText()
        {
            var go = new GameObject("FloatingRingText");
            go.transform.SetParent(transform, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = "+RING";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Palette.Accent;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = Palette.Shadow;
            return tmp;
        }

        public void EnsureAmbientMotes()
        {
            if (_ambientMotes != null) return;

            var go = new GameObject("AmbientMotes");
            go.transform.SetParent(transform, false);
            _ambientMotes = go.AddComponent<ParticleSystem>();
            var main = _ambientMotes.main;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.4f;
            main.startSize = 0.14f;
            main.maxParticles = 16;
            Color moteCol = Palette.KeyLight;
            moteCol.a = 0.35f;
            main.startColor = moteCol;

            var emission = _ambientMotes.emission;
            emission.rateOverTime = 3f;

            var shape = _ambientMotes.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(45f, 18f, 45f);

            var rend = _ambientMotes.GetComponent<ParticleSystemRenderer>();
            rend.material = GetParticleMaterial();
            _ambientMotes.Play();
        }

        public void PlayRingPassBurst(Vector3 pos)
        {
            ParticleSystem ps = _ringBurstPool.Get();
            ps.transform.position = pos;
            ps.Play();
            StartCoroutine(ReleaseAfter(ps, _ringBurstPool, 0.65f));
        }

        public void PlayCrashBurst(Vector3 pos)
        {
            ParticleSystem ps = _crashBurstPool.Get();
            ps.transform.position = pos;
            ps.Play();
            StartCoroutine(ReleaseAfter(ps, _crashBurstPool, 0.55f));
        }

        public void PlayMilestoneConfetti(Vector3 pos)
        {
            ParticleSystem ps = _confettiPool.Get();
            ps.transform.position = pos;
            ps.Play();
            StartCoroutine(ReleaseAfter(ps, _confettiPool, 1.35f));
        }

        public void SpawnFloatingText(Vector3 pos, string text = "+RING", Transform followTarget = null)
        {
            TextMeshPro tmp = _floatingTextPool.Get();
            tmp.text = text;
            tmp.transform.position = (followTarget != null ? followTarget.position : pos) + Vector3.up * 0.8f;
            if (Camera.main != null)
                tmp.transform.rotation = Camera.main.transform.rotation;
            StartCoroutine(AnimateFloatingText(tmp, followTarget));
        }

        private IEnumerator ReleaseAfter(ParticleSystem ps, ObjectPool<ParticleSystem> pool, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pool.Release(ps);
            }
        }

        private IEnumerator AnimateFloatingText(TextMeshPro tmp, Transform followTarget)
        {
            Vector3 startPos = tmp.transform.position;
            Vector3 endPos = startPos + Vector3.up * 1.2f;
            Color c = Palette.Accent;

            yield return Ease.Tween(0.6f, t =>
            {
                if (tmp == null) return;
                if (followTarget != null)
                {
                    tmp.transform.position = followTarget.position + Vector3.up * Mathf.Lerp(0.8f, 2.0f, t);
                    if (Camera.main != null)
                        tmp.transform.rotation = Camera.main.transform.rotation;
                }
                else
                {
                    tmp.transform.position = Vector3.Lerp(startPos, endPos, t);
                }
                c.a = 1f - t;
                tmp.color = c;
            }, Ease.OutCubic, unscaled: false);

            if (tmp != null)
                _floatingTextPool.Release(tmp);
        }
    }
}
