using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Afterrust
{
    [System.Serializable]
    public struct LootEntry
    {
        public string itemId;
        [Range(0f, 1f)] public float chance;
        public int min;
        public int max;

        public LootEntry(string itemId, float chance, int min, int max)
        {
            this.itemId = itemId;
            this.chance = chance;
            this.min = min;
            this.max = max;
        }
    }

    /// <summary>
    /// Searchable container. Contents are rolled once on the first search; whatever does not
    /// fit in the backpack stays inside for later.
    /// </summary>
    public sealed class LootContainer : MonoBehaviour, IInteractable
    {
        public string displayName = "Kutu";
        [Min(0f)] public float searchSeconds = 1.2f;
        public LootEntry[] table = new LootEntry[0];
        public int creditsMin, creditsMax;

        [SerializeField] private bool rolled;
        [SerializeField] private List<string> contents = new List<string>();

        public bool IsEmpty => rolled && contents.Count == 0;
        public bool Rolled => rolled;
        public List<string> Contents => contents;

        public void Restore(bool wasRolled, List<string> savedContents)
        {
            rolled = wasRolled;
            contents = savedContents != null ? new List<string>(savedContents) : new List<string>();
        }
        public float HoldSeconds => rolled ? 0f : searchSeconds;

        public string GetPrompt(AfterrustInteraction player)
        {
            if (!rolled) return "E (basılı tut) - Ara: " + displayName;
            return contents.Count == 0 ? displayName + " (boş)" : "E - Kalanları al: " + displayName;
        }

        public string Interact(AfterrustInteraction player)
        {
            AfterrustInventory inv = player != null ? player.inventory : null;
            if (!rolled) Roll(inv);
            if (contents.Count == 0) return displayName + ": boş.";
            if (inv == null) return "Envanter yok.";

            Dictionary<string, int> taken = new Dictionary<string, int>();
            for (int i = contents.Count - 1; i >= 0; i--)
            {
                if (inv.TryAddItem(contents[i]) == 0) continue;
                taken.TryGetValue(contents[i], out int n);
                taken[contents[i]] = n + 1;
                contents.RemoveAt(i);
            }

            if (taken.Count == 0) return "Çanta dolu, " + displayName + " içinde eşya kaldı.";
            StringBuilder sb = new StringBuilder("Bulundu: ");
            bool first = true;
            foreach (KeyValuePair<string, int> kv in taken)
            {
                if (!first) sb.Append(", ");
                first = false;
                ItemDef def = ItemDatabase.Get(kv.Key);
                sb.Append(def != null ? def.name : kv.Key);
                if (kv.Value > 1) sb.Append(" x").Append(kv.Value);
            }
            if (contents.Count > 0) sb.Append(" (çanta doldu, kalanlar içeride)");
            return sb.ToString();
        }

        private void Roll(AfterrustInventory inv)
        {
            rolled = true;
            contents.Clear();
            foreach (LootEntry e in table)
            {
                if (Random.value > e.chance) continue;
                int n = Random.Range(Mathf.Max(1, e.min), Mathf.Max(e.min, e.max) + 1);
                for (int i = 0; i < n; i++) contents.Add(e.itemId);
            }
            if (creditsMax > 0 && inv != null) inv.credits += Random.Range(creditsMin, creditsMax + 1);
        }
    }
}
