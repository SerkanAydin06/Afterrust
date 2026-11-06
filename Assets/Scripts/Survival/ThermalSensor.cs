using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Works out how warm the player's surroundings are (night, rain exposure, roof overhead,
    /// nearby lit stoves) and feeds it to SurvivorVitals.warmthTarget twice a second.
    /// </summary>
    public sealed class ThermalSensor : MonoBehaviour
    {
        public float baseDay = 55f;
        public float nightPenalty = 18f;
        public float rainPenalty = 22f;
        public float shelterBonus = 12f;
        public float stoveBonus = 45f;
        public float roofCheckHeight = 10f;

        private SurvivorVitals vitals;
        private float next;

        private void Awake()
        {
            vitals = GetComponent<SurvivorVitals>();
        }

        private void Update()
        {
            if (vitals == null || Time.time < next) return;
            next = Time.time + 0.5f;

            Vector3 head = transform.position + Vector3.up * 1.7f;
            bool sheltered = Physics.Raycast(head, Vector3.up, roofCheckHeight, ~0, QueryTriggerInteraction.Ignore);
            DayNightWeather world = DayNightWeather.Instance;
            float night = world != null ? Mathf.Clamp01((world.Darkness - 0.3f) / 0.5f) : 0f;
            bool raining = world != null && world.IsRaining;

            float stove = 0f;
            foreach (Stove s in Stove.All) stove = Mathf.Max(stove, s.HeatAt(transform.position));

            vitals.sheltered = sheltered;
            vitals.warmthTarget = Evaluate(baseDay, night, raining && !sheltered, sheltered, stove);
        }

        /// <summary>Pure rule, unit-tested.</summary>
        public static float Evaluate(float baseDay, float night01, bool exposedToRain, bool sheltered, float stoveHeat01,
            float nightPenalty = 18f, float rainPenalty = 22f, float shelterBonus = 12f, float stoveBonus = 45f)
        {
            float t = baseDay - nightPenalty * night01 - (exposedToRain ? rainPenalty : 0f) +
                      (sheltered ? shelterBonus : 0f) + stoveBonus * stoveHeat01;
            return Mathf.Clamp(t, 0f, 100f);
        }
    }
}
