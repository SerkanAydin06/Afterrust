using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// A clearly marked local environmental danger. Polls the survivor's XZ distance
    /// rather than relying on trigger callbacks (CharacterController-safe).
    /// </summary>
    public sealed class ToxicExposure : MonoBehaviour
    {
        [Min(0.25f)] public float radius = 3.45f;
        [Min(0f)] public float damagePerSecond = 0.75f;
        [Min(1f)] public float warningIntervalSeconds = 8f;

        private SurvivorVitals survivor;
        private AfterrustInteraction interaction;
        private float nextWarning;
        private bool inside;

        private void Start()
        {
            survivor = FindFirstObjectByType<SurvivorVitals>();
            if (survivor != null) interaction = survivor.GetComponent<AfterrustInteraction>();
        }

        private void Update()
        {
            if (survivor == null || survivor.IsDead) return;
            Vector3 delta = survivor.transform.position - transform.position;
            float distance = new Vector2(delta.x, delta.z).magnitude;
            float intensity = ExposureIntensity(distance, radius);
            if (intensity <= 0f)
            {
                inside = false;
                return;
            }

            survivor.Apply(0f, 0f, -damagePerSecond * intensity * Time.deltaTime);
            if (!inside || Time.time >= nextWarning)
            {
                interaction?.Notify("Kimyasal sızıntı! Buradan uzaklaş.");
                nextWarning = Time.time + warningIntervalSeconds;
                AfterrustAudio audio = survivor.GetComponent<AfterrustAudio>();
                if (audio != null) audio.PlayDanger();
            }
            inside = true;
        }

        /// <summary>A soft edge gives a readable risk gradient; zero outside the radius.</summary>
        public static float ExposureIntensity(float distance, float radius)
        {
            if (radius <= 0f || distance >= radius) return 0f;
            return Mathf.Clamp01((radius - Mathf.Max(0f, distance)) /
                                 Mathf.Max(0.1f, radius * 0.45f));
        }
    }
}
