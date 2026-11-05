using UnityEngine;

namespace Afterrust
{
    /// <summary>Subtle, irregular brightness flicker for old bulbs (no allocations, Perlin based).</summary>
    [RequireComponent(typeof(Light))]
    public sealed class LampFlicker : MonoBehaviour
    {
        [Range(0f, 1f)] public float depth = 0.12f;
        public float speed = 3.5f;
        [Tooltip("Chance per second of a short brown-out dip.")]
        [Range(0f, 1f)] public float dipChance = 0.08f;

        private Light lamp;
        private float baseIntensity;
        private float seed;
        private float dipUntil;

        private void Awake()
        {
            lamp = GetComponent<Light>();
            baseIntensity = lamp.intensity;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            float t = Time.time;
            float noise = Mathf.PerlinNoise(seed, t * speed) - 0.5f;
            if (t > dipUntil && Random.value < dipChance * Time.deltaTime) dipUntil = t + Random.Range(0.05f, 0.18f);
            float dip = t < dipUntil ? 0.45f : 1f;
            lamp.intensity = baseIntensity * dip * (1f + noise * 2f * depth);
        }
    }
}
