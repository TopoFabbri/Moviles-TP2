using UnityEngine;

namespace Game.Audio
{
    public class GameAudioService : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private static GameAudioService _instance;
        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private AudioClip _pickupClip;
        private AudioClip _milestoneClip;
        private AudioClip _crashClip;
        private AudioClip _uiClickClip;
        private AudioClip _ambientClip;

        public static float MusicVolume { get; private set; } = 0.8f;
        public static float SfxVolume { get; private set; } = 1.0f;
        public static bool IsPausedDucking { get; private set; }

        public static float EffectiveMusicVolume => MusicVolume * (IsPausedDucking ? 0.5f : 1f);
        public static float EffectiveSfxVolume => SfxVolume * (IsPausedDucking ? 0.5f : 1f);

        public static GameAudioService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(GameAudioService));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<GameAudioService>();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInitialized()
        {
            _ = Instance;
        }

        private void Initialize()
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            _pickupClip = CreatePickupBlipClip();
            _milestoneClip = CreateMilestoneStingClip();
            _crashClip = CreateCrashClip();
            _uiClickClip = CreateUiClickClip();
            _ambientClip = CreateAmbientBedClip();

            _musicSource.clip = _ambientClip;
            _musicSource.volume = EffectiveMusicVolume;
            _musicSource.Play();
        }

        public static void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            if (_instance != null && _instance._musicSource != null)
                _instance._musicSource.volume = EffectiveMusicVolume;
        }

        public static void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
        }

        public static void SetPausedDucking(bool paused)
        {
            IsPausedDucking = paused;
            if (_instance != null && _instance._musicSource != null)
                _instance._musicSource.volume = EffectiveMusicVolume;
        }

        public static void PlayPickupBlip()
        {
            var inst = Instance;
            inst._sfxSource.pitch = Random.Range(0.92f, 1.08f);
            inst._sfxSource.PlayOneShot(inst._pickupClip, EffectiveSfxVolume);
        }

        public static void PlayMilestoneSting()
        {
            var inst = Instance;
            inst._sfxSource.pitch = 1f;
            inst._sfxSource.PlayOneShot(inst._milestoneClip, EffectiveSfxVolume);
        }

        public static void PlayCrashSound()
        {
            var inst = Instance;
            inst._sfxSource.pitch = 1f;
            inst._sfxSource.PlayOneShot(inst._crashClip, EffectiveSfxVolume);
        }

        public static void PlayUiClick()
        {
            var inst = Instance;
            inst._sfxSource.pitch = 1f;
            inst._sfxSource.PlayOneShot(inst._uiClickClip, EffectiveSfxVolume * 0.75f);
        }

        public static AudioClip CreatePickupBlipClip()
        {
            int samples = Mathf.RoundToInt(SampleRate * 0.09f);
            float[] data = new float[samples];
            double phase = 0.0;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(880f, 1320f, t);
                phase += 2.0 * Mathf.PI * freq / SampleRate;
                data[i] = Mathf.Sin((float)phase) * 0.45f * (1f - t * 0.5f);
            }

            ApplyFadeEnvelope(data, 0.005f);
            return BuildClip("PickupBlip", data);
        }

        public static AudioClip CreateMilestoneStingClip()
        {
            float[] freqs = { 523.25f, 659.25f, 783.99f };
            int noteSamples = Mathf.RoundToInt(SampleRate * 0.12f);
            float[] data = new float[noteSamples * freqs.Length];

            for (int n = 0; n < freqs.Length; n++)
            {
                double phase = 0.0;
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / noteSamples;
                    phase += 2.0 * Mathf.PI * freqs[n] / SampleRate;
                    float env = Mathf.Sin(t * Mathf.PI);
                    data[n * noteSamples + i] = Mathf.Sin((float)phase) * 0.4f * env;
                }
            }

            ApplyFadeEnvelope(data, 0.005f);
            return BuildClip("MilestoneSting", data);
        }

        public static AudioClip CreateCrashClip()
        {
            int samples = Mathf.RoundToInt(SampleRate * 0.18f);
            float[] data = new float[samples];
            double phase = 0.0;
            float lp = 0f;
            var rng = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(180f, 55f, t);
                phase += 2.0 * Mathf.PI * freq / SampleRate;
                float tri = 2f * Mathf.Abs(2f * (float)((phase / (2.0 * Mathf.PI)) % 1.0) - 1f) - 1f;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp = Mathf.Lerp(lp, noise, 0.15f);
                data[i] = (tri * 0.4f + lp * 0.45f) * (1f - t);
            }

            ApplyFadeEnvelope(data, 0.005f);
            return BuildClip("CrashSound", data);
        }

        public static AudioClip CreateUiClickClip()
        {
            int samples = Mathf.RoundToInt(SampleRate * 0.02f);
            float[] data = new float[samples];
            float lp = 0f;
            var rng = new System.Random(7);
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp = Mathf.Lerp(lp, noise, 0.35f);
                data[i] = lp * (1f - t) * 0.5f;
            }

            ApplyFadeEnvelope(data, 0.005f);
            return BuildClip("UiClick", data);
        }

        public static AudioClip CreateAmbientBedClip()
        {
            int samples = SampleRate * 6;
            float[] data = new float[samples];
            float brown = 0f;
            var rng = new System.Random(101);
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                brown = Mathf.Clamp(brown + white * 0.02f, -1f, 1f);
                float swell = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;
                float drone = Mathf.Sin(2f * Mathf.PI * 110f * i / SampleRate) * 0.05f * swell;
                data[i] = brown * 0.08f + drone;
            }

            ApplyFadeEnvelope(data, 0.005f);
            return BuildClip("AmbientBed", data);
        }

        private static void ApplyFadeEnvelope(float[] data, float fadeSeconds)
        {
            int fadeSamples = Mathf.Min(data.Length / 2, Mathf.RoundToInt(SampleRate * fadeSeconds));
            for (int i = 0; i < fadeSamples; i++)
            {
                float gain = (float)i / fadeSamples;
                data[i] *= gain;
                data[data.Length - 1 - i] *= gain;
            }
        }

        private static AudioClip BuildClip(string clipName, float[] data)
        {
            AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
