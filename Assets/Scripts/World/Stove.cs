using System.Collections.Generic;
using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Drum stove: burns wood/charcoal, warms everything nearby (ThermalSensor), glows, smokes
    /// from the flue and enables the cooking recipes. E opens the stove panel.
    /// </summary>
    public sealed class Stove : MonoBehaviour, IInteractable
    {
        public static readonly List<Stove> All = new List<Stove>();

        public string displayName = "Soba";
        [Tooltip("Real seconds of fire per unit of fuel.")]
        public float woodSeconds = 240f;
        public float charcoalSeconds = 360f;
        public float maxBurnSeconds = 900f;
        [Tooltip("Metres within which the fire warms you.")]
        public float heatRadius = 5f;

        public float burnSecondsLeft;

        [Header("Visuals (assigned by the scene setup)")]
        public Light fireLight;
        public Renderer glowRenderer;
        public Material glowOn;
        public Material glowOff;
        public ParticleSystem smoke;

        private float baseLightIntensity = 1.6f;
        private bool shownLit = true;

        public bool IsLit => burnSecondsLeft > 0f;
        public float HoldSeconds => 0f;

        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() { All.Remove(this); }

        private void Awake()
        {
            if (fireLight != null) baseLightIntensity = Mathf.Max(fireLight.intensity, baseLightIntensity);
            RefreshVisuals(true);
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            return IsLit ? "E - Soba (yanıyor " + TimeLeftText + ")" : "E - Soba (sönük): ateş yak";
        }

        public string Interact(AfterrustInteraction player)
        {
            if (player != null && player.hud != null) player.hud.OpenStation(CraftStation.Stove, this);
            return null;
        }

        public string TimeLeftText
        {
            get
            {
                int s = Mathf.CeilToInt(burnSecondsLeft);
                return (s / 60) + ":" + (s % 60).ToString("00");
            }
        }

        /// <summary>Adds one unit of fuel from the inventory ("wood" or "charcoal").</summary>
        public string AddFuel(AfterrustInventory inv, string fuelId)
        {
            if (inv == null) return "Envanter yok.";
            float seconds = fuelId == "charcoal" ? charcoalSeconds : fuelId == "wood" ? woodSeconds : 0f;
            if (seconds <= 0f) return "Bu yakıt olmaz.";
            if (burnSecondsLeft + seconds * 0.5f > maxBurnSeconds) return "Soba dolu.";
            if (!inv.TryRemove(fuelId)) return fuelId == "wood" ? "Odun yok." : "Kömür yok.";
            bool wasLit = IsLit;
            burnSecondsLeft = Mathf.Min(maxBurnSeconds, burnSecondsLeft + seconds);
            RefreshVisuals(false);
            return wasLit ? "Ateşe yakıt eklendi." : "Ateş yandı.";
        }

        /// <summary>Burns fuel for a span of real seconds (also used when sleeping).</summary>
        public void Burn(float seconds)
        {
            if (!IsLit) return;
            burnSecondsLeft = Mathf.Max(0f, burnSecondsLeft - seconds);
            if (!IsLit) RefreshVisuals(false);
        }

        /// <summary>0..1 heat at a world position (1 = next to the stove).</summary>
        public float HeatAt(Vector3 position)
        {
            if (!IsLit) return 0f;
            float d = Vector3.Distance(position, transform.position);
            return Mathf.Clamp01(1f - d / heatRadius);
        }

        public void Restore(float seconds)
        {
            burnSecondsLeft = Mathf.Clamp(seconds, 0f, maxBurnSeconds);
            RefreshVisuals(false);
        }

        private void Update()
        {
            Burn(Time.deltaTime);
            if (IsLit && fireLight != null)
                fireLight.intensity = baseLightIntensity * (0.85f + 0.3f * Mathf.PerlinNoise(Time.time * 6f, 0.37f));
        }

        private void RefreshVisuals(bool force)
        {
            bool lit = IsLit;
            if (!force && lit == shownLit) return;
            shownLit = lit;
            if (fireLight != null) fireLight.enabled = lit;
            if (glowRenderer != null && glowOn != null && glowOff != null)
                glowRenderer.sharedMaterial = lit ? glowOn : glowOff;
            if (smoke != null)
            {
                ParticleSystem.EmissionModule em = smoke.emission;
                em.enabled = lit;
            }
        }
    }
}
