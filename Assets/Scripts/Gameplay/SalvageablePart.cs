using UnityEngine;

namespace Afterrust
{
    public enum SalvageTool
    {
        None,
        Wrench,
        Screwdriver,
        WireCutter
    }

    public enum SalvageResult
    {
        Success,
        AlreadyTaken,
        MissingTool,
        Blocked,
        InventoryFull,
        NoInventory
    }

    /// <summary>
    /// A single removable component of a machine. Dependencies are other components
    /// in the same scene which must be removed first.
    /// </summary>
    public sealed class SalvageablePart : MonoBehaviour
    {
        public string partId = "scrap-metal";
        public string displayName = "Pasli Metal";
        [Range(0, 100)] public int condition = 65;
        [Min(0.01f)] public float massKg = 2f;
        [Min(0)] public int baseValue = 10;

        [Tooltip("Legacy flag retained for old prototype objects.")]
        public bool requiresWrench;
        public SalvageTool requiredTool = SalvageTool.None;
        public SalvageablePart[] removeFirst;
        public bool wasSalvaged;

        public int SaleValue => SalvageValue.Calculate(baseValue, condition);

        public SalvageTool EffectiveTool =>
            requiredTool == SalvageTool.None && requiresWrench
                ? SalvageTool.Wrench : requiredTool;

        public bool PrerequisitesComplete
        {
            get
            {
                if (removeFirst == null) return true;
                foreach (SalvageablePart part in removeFirst)
                {
                    if (part != null && !part.wasSalvaged) return false;
                }
                return true;
            }
        }

        public SalvageResult TrySalvageDetailed(AfterrustInventory inventory)
        {
            if (wasSalvaged) return SalvageResult.AlreadyTaken;
            if (inventory == null) return SalvageResult.NoInventory;
            if (!PrerequisitesComplete) return SalvageResult.Blocked;
            if (!inventory.HasTool(EffectiveTool)) return SalvageResult.MissingTool;
            if (!inventory.TryAdd(partId, displayName, massKg, SaleValue, condition))
                return SalvageResult.InventoryFull;

            wasSalvaged = true;
            gameObject.SetActive(false);
            return SalvageResult.Success;
        }

        public bool TrySalvage(AfterrustInventory inventory)
        {
            return TrySalvageDetailed(inventory) == SalvageResult.Success;
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
