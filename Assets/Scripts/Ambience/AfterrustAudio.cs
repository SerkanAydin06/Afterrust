using System.Collections.Generic;
using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Lightweight 2D player soundscape: footfalls, interaction/crafting, wind and rain.
    /// Optional imported clips override the built-in original procedural prototypes.
    /// No per-frame sample generation and no external audio dependency.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class AfterrustAudio : MonoBehaviour
    {
        [Header("Mix")]
        [Range(0f, 1f)] public float masterVolume = 0.65f;
        [Range(0f, 1f)] public float ambienceVolume = 0.18f;
        [Range(0f, 1f)] public float effectsVolume = 0.32f;

        [Header("Optional replacement clips")]
        public AudioClip windClip;
        public AudioClip rainClip;
        public AudioClip footstepClip;
        public AudioClip interactionClip;
        public AudioClip craftingClip;
        public AudioClip dangerClip;

        private readonly List<AudioClip> generated = new List<AudioClip>();
        private CharacterController mover;
        private SurvivorVitals vitals;
        private AudioSource wind;
        private AudioSource rain;
        private AudioSource effects;
        private AudioLowPassFilter windFilter;
        private AudioLowPassFilter rainFilter;
        private AudioClip[] footstepVariations;
        private float nextStep;

        private void Awake()
        {
            mover = GetComponent<CharacterController>();
            vitals = GetComponent<SurvivorVitals>();

            // These two streams keep their noise-filter state across every AudioClip
            // buffer request. There is no 2-3 second loop or fade-to-silence seam.
            if (windClip == null) windClip = GenerateAmbient(false, 154);
            if (rainClip == null) rainClip = GenerateAmbient(true, 317);
            if (footstepClip == null)
            {
                footstepClip = Generate("footstep", 0.19f, 615);
                footstepVariations = new[]
                {
                    footstepClip,
                    Generate("footstep", 0.17f, 624),
                    Generate("footstep", 0.21f, 755),
                    Generate("footstep", 0.19f, 812)
                };
            }
            else footstepVariations = new[] { footstepClip };
            if (interactionClip == null) interactionClip = Generate("interaction", 0.16f, 413);
            if (craftingClip == null) craftingClip = Generate("crafting", 0.37f, 821);
            if (dangerClip == null) dangerClip = Generate("danger", 0.32f, 917);

            wind = MakeSource("Wind ambience", true);
            rain = MakeSource("Rain ambience", true);
            effects = MakeSource("Survivor effects", false);
            windFilter = wind.gameObject.AddComponent<AudioLowPassFilter>();
            rainFilter = rain.gameObject.AddComponent<AudioLowPassFilter>();
            windFilter.cutoffFrequency = 12000f;
            rainFilter.cutoffFrequency = 12000f;
            wind.clip = windClip;
            rain.clip = rainClip;
            if (wind.clip != null) wind.Play();
            if (rain.clip != null) rain.Play();
        }

        private AudioSource MakeSource(string objectName, bool loop)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.volume = loop ? 0f : 1f;
            return source;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            DayNightWeather world = DayNightWeather.Instance;
            bool rainy = world != null && world.IsRaining;
            bool indoors = vitals != null && vitals.sheltered;
            float mix = masterVolume * ambienceVolume;

            float windTarget = mix * (rainy ? 0.66f : 0.36f) * (indoors ? 0.25f : 1f);
            float rainTarget = rainy ? mix * (indoors ? 0.12f : 0.95f) : 0f;
            if (wind != null) wind.volume = Mathf.MoveTowards(wind.volume, windTarget, dt * 0.12f);
            if (rain != null) rain.volume = Mathf.MoveTowards(rain.volume, rainTarget, dt * 0.15f);
            // Rain can still be heard outside the shelter, but high-frequency drops
            // are muffled by a roof rather than sounding as if they hit the floor.
            if (windFilter != null)
                windFilter.cutoffFrequency = Mathf.MoveTowards(windFilter.cutoffFrequency,
                    indoors ? 850f : 12000f, dt * 6000f);
            if (rainFilter != null)
                rainFilter.cutoffFrequency = Mathf.MoveTowards(rainFilter.cutoffFrequency,
                    indoors ? 600f : 12000f, dt * 6000f);

            if (mover == null || !mover.enabled || !mover.isGrounded ||
                (vitals != null && vitals.IsDead))
                return;

            Vector3 velocity = mover.velocity;
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            if (speed < 1.1f || Time.time < nextStep) return;
            bool running = speed > 4.7f;
            nextStep = Time.time + (running ? 0.32f : 0.50f);
            if (effects != null && footstepVariations != null && footstepVariations.Length > 0)
            {
                AudioClip step = footstepVariations[Random.Range(0, footstepVariations.Length)];
                effects.pitch = (running ? 1.08f : 0.92f) * Random.Range(0.94f, 1.06f);
                effects.PlayOneShot(step, masterVolume * effectsVolume *
                    (running ? 0.7f : 0.48f) * Random.Range(0.9f, 1.08f));
            }
        }

        public void PlayInteraction()
        {
            PlayEffect(interactionClip, 0.65f);
        }

        public void PlayCrafting()
        {
            PlayEffect(craftingClip, 0.85f);
        }

        public void PlayDanger()
        {
            PlayEffect(dangerClip, 0.65f);
        }

        private void PlayEffect(AudioClip clip, float gain)
        {
            if (effects == null || clip == null) return;
            effects.pitch = 1f;
            effects.PlayOneShot(clip, Mathf.Clamp01(masterVolume * effectsVolume * gain));
        }

        private AudioClip GenerateAmbient(bool isRain, int seed)
        {
            const int sampleRate = 22050;
            ContinuousAmbientNoise noise = new ContinuousAmbientNoise(isRain, seed, sampleRate);
            // Streamed PCM callback regenerates NEW noise on every buffer fill. The
            // state is never reset at AudioSource loop boundaries.
            AudioClip clip = AudioClip.Create(
                isRain ? "Afterrust continuous rain" : "Afterrust continuous wind",
                sampleRate * 2, 1, sampleRate, true, noise.Fill);
            generated.Add(clip);
            return clip;
        }

        /// <summary>
        /// Endless colored noise without an audible loop or silent seam.
        /// PCM callback may run on Unity's audio thread: only plain .NET math is used.
        /// The exposed sample generator is covered by EditMode regression tests.
        /// </summary>
        public sealed class ContinuousAmbientNoise
        {
            private readonly System.Random rng;
            private readonly bool isRain;
            private readonly double samplePeriod;
            private double elapsed;
            private float slow;
            private float mid;

            public ContinuousAmbientNoise(bool isRain, int seed, int sampleRate)
            {
                this.isRain = isRain;
                rng = new System.Random(seed);
                samplePeriod = 1.0 / System.Math.Max(1, sampleRate);
            }

            public void Fill(float[] samples)
            {
                if (samples == null) return;
                for (int i = 0; i < samples.Length; i++)
                {
                    float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                    mid += (noise - mid) * (isRain ? 0.065f : 0.016f);
                    slow += (noise - slow) * (isRain ? 0.0018f : 0.0005f);

                    // Low, incommensurate modulation rates prevent a recognizable
                    // repeated gust or rain pattern. No fade at the clip boundary.
                    double swell = isRain
                        ? 0.83 + 0.11 * System.Math.Sin(elapsed * 0.47) +
                          0.06 * System.Math.Sin(elapsed * 0.137)
                        : 0.76 + 0.18 * System.Math.Sin(elapsed * 0.63) +
                          0.12 * System.Math.Sin(elapsed * 0.193);
                    float sample = isRain
                        ? (noise * 0.13f + mid * 0.64f + slow * 0.24f) * (float)swell
                        : (mid * 0.9f + slow * 2.5f + noise * 0.015f) * (float)swell;
                    samples[i] = sample > 1f ? 1f : sample < -1f ? -1f : sample;
                    elapsed += samplePeriod;
                }
            }
        }

        private AudioClip Generate(string kind, float seconds, int seed)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(seconds * sampleRate);
            float[] data = new float[count];
            System.Random random = new System.Random(seed);
            float low = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                low += (white - low) * (kind == "wind" ? 0.008f : 0.06f);
                float sample;
                switch (kind)
                {
                    case "wind":
                        sample = low * (1.6f + 0.55f * Mathf.Sin(t * 1.3f));
                        break;
                    case "rain":
                        sample = white * 0.13f + low * 0.62f;
                        break;
                    case "footstep":
                        sample = (Mathf.Sin(t * 2f * Mathf.PI * 76f) * 0.25f +
                                  low * 1.3f) * Mathf.Exp(-t * 25f);
                        break;
                    case "interaction":
                        sample = (Mathf.Sin(t * 2f * Mathf.PI * 520f) * 0.26f +
                                  white * 0.15f) * Mathf.Exp(-t * 38f);
                        break;
                    case "crafting":
                        float pulse = Mathf.Exp(-t * 35f) +
                                      (t > 0.17f ? 0.75f * Mathf.Exp(-(t - 0.17f) * 32f) : 0f);
                        sample = (low * 1.05f + Mathf.Sin(t * 2f * Mathf.PI * 195f) * 0.16f) * pulse;
                        break;
                    default:
                        sample = Mathf.Sin(t * 2f * Mathf.PI * (400f + t * 480f)) *
                                 Mathf.Sin(Mathf.PI * t / seconds) * 0.2f;
                        break;
                }
                data[i] = Mathf.Clamp(sample, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Afterrust " + kind + " prototype", count, 1, sampleRate, false);
            clip.SetData(data, 0);
            generated.Add(clip);
            return clip;
        }

        private void OnDestroy()
        {
            foreach (AudioClip clip in generated)
                if (clip != null) Destroy(clip);
            generated.Clear();
        }
    }
}
