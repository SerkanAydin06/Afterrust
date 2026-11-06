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
        private float nextStep;

        private void Awake()
        {
            mover = GetComponent<CharacterController>();
            vitals = GetComponent<SurvivorVitals>();

            if (windClip == null) windClip = Generate("wind", 2.8f, 154);
            if (rainClip == null) rainClip = Generate("rain", 2.3f, 317);
            if (footstepClip == null) footstepClip = Generate("footstep", 0.19f, 615);
            if (interactionClip == null) interactionClip = Generate("interaction", 0.16f, 413);
            if (craftingClip == null) craftingClip = Generate("crafting", 0.37f, 821);
            if (dangerClip == null) dangerClip = Generate("danger", 0.32f, 917);

            wind = MakeSource("Wind ambience", true);
            rain = MakeSource("Rain ambience", true);
            effects = MakeSource("Survivor effects", false);
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
            source.volume = 0f;
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
            float rainTarget = rainy ? mix * (indoors ? 0.18f : 0.95f) : 0f;
            if (wind != null) wind.volume = Mathf.MoveTowards(wind.volume, windTarget, dt * 0.12f);
            if (rain != null) rain.volume = Mathf.MoveTowards(rain.volume, rainTarget, dt * 0.15f);

            if (mover == null || !mover.enabled || !mover.isGrounded ||
                (vitals != null && vitals.IsDead))
                return;

            Vector3 velocity = mover.velocity;
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            if (speed < 1.1f || Time.time < nextStep) return;
            bool running = speed > 4.7f;
            nextStep = Time.time + (running ? 0.32f : 0.50f);
            if (effects != null && footstepClip != null)
            {
                effects.pitch = running ? 1.08f : 0.92f;
                effects.PlayOneShot(footstepClip, masterVolume * effectsVolume * (running ? 0.7f : 0.48f));
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
                // Short crossfade near the loop boundary avoids harsh ambient clicks.
                if (kind == "wind" || kind == "rain")
                {
                    float edge = Mathf.Min(t, seconds - t);
                    sample *= Mathf.Clamp01(edge / 0.08f);
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
