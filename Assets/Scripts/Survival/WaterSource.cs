using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Open rainwater collector. Starts completely empty, fills ONLY during rain,
    /// and keeps continuous fractional volume so the visible surface rises slowly.
    /// One dirty-water item consumes exactly one of maxUnits equal portions.
    /// </summary>
    public sealed class WaterSource : MonoBehaviour, IInteractable
    {
        public string displayName = "Yağmur Varili";
        public string itemId = "water_dirty";

        [Min(1)] public int maxUnits = 3;
        [Tooltip("Real seconds of active rainfall for a completely empty barrel to fill. 900 = 15 minutes of rain.")]
        [Min(60f)] public float secondsToFill = 900f;

        [Header("Visual fill (optional child transforms)")]
        public Transform waterSurface;
        public Transform levelGauge;
        [Range(0f, 1f)] [SerializeField] private float fill01;

        private const float BottomWaterY = 0.075f;
        private const float MaxWaterHeight = 0.80f;

        public float HoldSeconds => 0.6f;
        public float Fill01 => fill01;
        public int Units => Mathf.Clamp(Mathf.FloorToInt(fill01 * Mathf.Max(1, maxUnits) + 0.0001f),
            0, Mathf.Max(1, maxUnits));

        private void Awake()
        {
            UpdateVisual();
        }

        private void Update()
        {
            bool isRaining = DayNightWeather.Instance != null && DayNightWeather.Instance.IsRaining;
            AdvanceFill(Time.deltaTime, isRaining);
        }

        /// <summary>Pure accumulation rule; dry weather never adds water.</summary>
        public static float Accumulate(float current, float deltaSeconds, bool raining, float secondsToFull)
        {
            current = Mathf.Clamp01(current);
            if (!raining || deltaSeconds <= 0f || secondsToFull <= 0f) return current;
            return Mathf.Clamp01(current + deltaSeconds / secondsToFull);
        }

        /// <summary>Controlled entry point for EditMode tests and the game update loop.</summary>
        public void AdvanceFill(float deltaSeconds, bool raining)
        {
            float next = Accumulate(fill01, deltaSeconds, raining, secondsToFill);
            if (Mathf.Approximately(next, fill01)) return;
            fill01 = next;
            UpdateVisual();
        }

        /// <summary>Restore continuous progress, not just full water portions.</summary>
        public void Restore(float savedFill01)
        {
            fill01 = Mathf.Clamp01(savedFill01);
            UpdateVisual();
        }

        /// <summary>Compatibility with old integer rain-barrel save states.</summary>
        public void Restore(int savedUnits)
        {
            Restore((float)Mathf.Clamp(savedUnits, 0, Mathf.Max(1, maxUnits)) / Mathf.Max(1, maxUnits));
        }

        private void UpdateVisual()
        {
            if (waterSurface != null)
            {
                waterSurface.localPosition = new Vector3(0f, BottomWaterY + fill01 * MaxWaterHeight, 0f);
                Renderer r = waterSurface.GetComponent<Renderer>();
                if (r != null) r.enabled = fill01 > 0.004f;
            }

            if (levelGauge != null)
            {
                float height = 0.76f * fill01;
                levelGauge.localPosition = new Vector3(0.353f, 0.10f + height * 0.5f, 0f);
                levelGauge.localScale = new Vector3(0.025f, Mathf.Max(height, 0.002f), 0.04f);
                Renderer r = levelGauge.GetComponent<Renderer>();
                if (r != null) r.enabled = fill01 > 0.004f;
            }
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            int percent = Mathf.RoundToInt(fill01 * 100f);
            if (Units > 0)
                return "E - Kirli su doldur (" + Units + "/" + maxUnits + ", %" + percent + "): " + displayName;
            return percent <= 0
                ? displayName + " boş. Yalnızca yağmur yağınca dolar."
                : displayName + " %" + percent + " dolu. Yağmurla yavaşça birikiyor.";
        }

        public string Interact(AfterrustInteraction player)
        {
            if (Units <= 0) return "Henüz bir şişelik su birikmedi.";
            if (player == null || player.inventory == null || player.inventory.TryAddItem(itemId) == 0)
                return "Çanta dolu.";

            fill01 = Mathf.Max(0f, fill01 - 1f / Mathf.Max(1, maxUnits));
            UpdateVisual();
            return "Kirli su dolduruldu. İçmeden önce süz.";
        }
    }
}
