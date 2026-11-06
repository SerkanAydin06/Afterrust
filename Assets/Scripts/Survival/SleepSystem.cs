using System.Collections;
using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Sleeping in a bed: at night until 07:00, by day a 3 hour nap. The screen fades,
    /// time and the stove advance, needs drop a little, energy refills, then the game saves.
    /// </summary>
    public sealed class SleepSystem : MonoBehaviour
    {
        public float wakeHour = 7f;
        public float napHours = 3f;

        private SurvivorVitals vitals;
        private AfterrustFirstPersonController controller;
        private SaveSystem save;

        public bool Sleeping { get; private set; }
        /// <summary>0..1 black overlay for the HUD.</summary>
        public float Fade { get; private set; }

        private void Awake()
        {
            vitals = GetComponent<SurvivorVitals>();
            controller = GetComponent<AfterrustFirstPersonController>();
            save = GetComponent<SaveSystem>();
        }

        public string TrySleep(Bed bed)
        {
            if (Sleeping || vitals == null || vitals.IsDead) return null;
            if (vitals.hunger < 8f || vitals.thirst < 8f) return "Bu açlık ve susuzlukla uyuyamazsın.";
            if (vitals.warmth < 25f && vitals.warmthTarget < 40f) return "Çok soğuk. Önce sobayı yak.";

            DayNightWeather world = DayNightWeather.Instance;
            float hour = world != null ? world.timeOfDay : 12f;
            bool night = hour >= 19f || hour < 5f;
            if (!night && vitals.energy > 70f) return "Uykun yok.";

            float hours = night ? HoursUntil(hour, wakeHour) : napHours;
            StartCoroutine(SleepRoutine(hours));
            return null;
        }

        public static float HoursUntil(float now, float target)
        {
            float h = target - now;
            return h <= 0f ? h + 24f : h;
        }

        /// <summary>Effect of sleeping for <paramref name="hours"/> in-game hours (unit-tested).</summary>
        public static void ApplySleep(SurvivorVitals v, float hours)
        {
            v.hunger = Mathf.Max(0f, v.hunger - hours * 1.2f);
            v.thirst = Mathf.Max(0f, v.thirst - hours * 1.8f);
            v.ApplyRest(hours * 12.5f);
            if (v.hunger > 20f && v.thirst > 20f) v.Apply(0f, 0f, hours * 1.5f);
            v.stamina = SurvivorVitals.Max;
        }

        private IEnumerator SleepRoutine(float hours)
        {
            Sleeping = true;
            if (controller != null) controller.SetUiMode(true);
            for (float t = 0f; t < 1f; t += Time.deltaTime) { Fade = t; yield return null; }
            Fade = 1f;

            DayNightWeather world = DayNightWeather.Instance;
            float realSeconds = world != null ? world.AdvanceHours(hours) : hours * 60f;
            foreach (Stove s in Stove.All) s.Burn(realSeconds);
            ApplySleep(vitals, hours);
            vitals.warmth = Mathf.Max(vitals.warmth, vitals.warmthTarget);
            yield return new WaitForSeconds(1.2f);
            if (save != null) save.Save();

            for (float t = 1f; t > 0f; t -= Time.deltaTime) { Fade = t; yield return null; }
            Fade = 0f;
            Sleeping = false;
            if (controller != null) controller.SetUiMode(false);
        }
    }
}
