using UnityEngine;

namespace Afterrust
{
    public sealed class SalvageablePart : MonoBehaviour
    {
        public string partId = "scrap-metal";
        public string displayName = "Pasli Metal";
        [Range(0, 100)] public int condition = 65;
        [Min(0.01f)] public float massKg = 2f;
        [Min(0)] public int baseValue = 10;
        public bool requiresWrench;
        public bool wasSalvaged;

        public int SaleValue => SalvageValue.Calculate(baseValue, condition);

        public bool TrySalvage(AfterrustInventory inventory)
        {
            if (wasSalvaged || inventory == null) return false;
            if (requiresWrench && !inventory.hasWrench) return false;
            if (!inventory.TryAdd(partId, displayName, massKg, SaleValue)) return false;
            wasSalvaged = true;
            gameObject.SetActive(false);
            return true;
        }
    }

    public static class SalvageValue
    {
        public static int Calculate(int baseValue, int condition)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseValue) *
                Mathf.Clamp01(condition / 100f)));
        }
    }
}
