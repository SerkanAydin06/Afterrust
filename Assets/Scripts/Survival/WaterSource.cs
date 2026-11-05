using UnityEngine;

namespace Afterrust
{
    /// <summary>Rain barrel: gives dirty water, refills after a cooldown.</summary>
    public sealed class WaterSource : MonoBehaviour, IInteractable
    {
        public string displayName = "Yağmur Varili";
        public string itemId = "water_dirty";
        [Min(1)] public int maxUnits = 3;
        [Tooltip("Seconds for one unit to refill.")]
        public float refillSeconds = 45f;

        private int units = -1;
        private float nextRefill;

        public float HoldSeconds => 0.6f;

        private void Refill()
        {
            if (units < 0) units = maxUnits;
            while (units < maxUnits && Time.time >= nextRefill)
            {
                units++;
                nextRefill += refillSeconds;
            }
            if (units >= maxUnits) nextRefill = Time.time + refillSeconds;
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            Refill();
            return units > 0 ? "E - Kirli su doldur (" + units + "/" + maxUnits + "): " + displayName
                : displayName + " boş, yağmur bekleniyor";
        }

        public string Interact(AfterrustInteraction player)
        {
            Refill();
            if (units <= 0) return displayName + " boş.";
            if (player == null || player.inventory == null || player.inventory.TryAddItem(itemId) == 0)
                return "Çanta dolu.";
            if (units == maxUnits) nextRefill = Time.time + refillSeconds;
            units--;
            return "Kirli Su dolduruldu. Süzmeden içmek riskli.";
        }
    }
}
