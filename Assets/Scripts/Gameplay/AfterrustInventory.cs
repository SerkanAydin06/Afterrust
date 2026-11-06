using System.Collections.Generic;
using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Weight-limited backpack. Every entry is one unit of an item (counts are derived), so
    /// salvage parts can keep their individual condition and value.
    /// </summary>
    public sealed class AfterrustInventory : MonoBehaviour
    {
        [System.Serializable]
        public class ItemStack
        {
            public string id;
            public string displayName;
            public float massKg;
            public int saleValue;
            public int condition;
        }

        /// <summary>Grouped view for UI: one row per item id.</summary>
        public struct Group
        {
            public string id;
            public string displayName;
            public int count;
            public float massKg;
        }

        [Min(1f)] public float capacityKg = 35f;
        public bool hasWrench = true;
        public bool hasScrewdriver = true;
        public bool hasWireCutter = false;
        public bool hasLargeBackpack = false;
        public int credits = 0;
        public List<ItemStack> items = new List<ItemStack>();

        public bool HasTool(SalvageTool tool)
        {
            switch (tool)
            {
                case SalvageTool.None: return true;
                case SalvageTool.Wrench: return hasWrench;
                case SalvageTool.Screwdriver: return hasScrewdriver;
                case SalvageTool.WireCutter: return hasWireCutter;
                default: return false;
            }
        }

        public float CurrentMassKg
        {
            get
            {
                float sum = 0f;
                foreach (ItemStack item in items) sum += item.massKg;
                return sum;
            }
        }

        public float FreeKg => Mathf.Max(0f, capacityKg - CurrentMassKg);

        public bool TryAdd(string id, string displayName, float massKg, int saleValue)
        {
            return TryAdd(id, displayName, massKg, saleValue, 100);
        }

        public bool TryAdd(string id, string displayName, float massKg, int saleValue, int condition)
        {
            if (massKg < 0f || CurrentMassKg + massKg > capacityKg + 0.0001f) return false;
            items.Add(new ItemStack
            {
                id = id,
                displayName = displayName,
                massKg = massKg,
                saleValue = saleValue,
                condition = Mathf.Clamp(condition, 0, 100)
            });
            return true;
        }

        /// <summary>Adds up to <paramref name="count"/> units of a catalogue item; returns how many fit.</summary>
        public int TryAddItem(string id, int count = 1)
        {
            ItemDef def = ItemDatabase.Get(id);
            if (def == null) return 0;
            int added = 0;
            for (int i = 0; i < count; i++)
            {
                if (!TryAdd(def.id, def.name, def.massKg, def.value, 100)) break;
                added++;
            }
            return added;
        }

        public int CountOf(string id)
        {
            int n = 0;
            foreach (ItemStack item in items)
                if (item.id == id) n++;
            return n;
        }

        public bool TryRemove(string id, int count = 1)
        {
            if (CountOf(id) < count) return false;
            for (int i = items.Count - 1; i >= 0 && count > 0; i--)
            {
                if (items[i].id != id) continue;
                items.RemoveAt(i);
                count--;
            }
            return true;
        }

        public List<Group> Grouped()
        {
            List<Group> result = new List<Group>();
            foreach (ItemStack item in items)
            {
                int index = result.FindIndex(g => g.id == item.id);
                if (index < 0)
                {
                    result.Add(new Group { id = item.id, displayName = item.displayName, count = 1, massKg = item.massKg });
                }
                else
                {
                    Group g = result[index];
                    g.count++;
                    g.massKg += item.massKg;
                    result[index] = g;
                }
            }
            return result;
        }

        /// <summary>Eats/drinks/uses one unit; returns feedback or null if not possible.</summary>
        public string Consume(string id, SurvivorVitals vitals)
        {
            ItemDef def = ItemDatabase.Get(id);
            if (def == null || !def.IsConsumable || vitals == null || vitals.IsDead) return null;
            if (!TryRemove(id)) return null;
            vitals.Apply(def.hunger, def.thirst, def.health);
            if (def.warmth != 0f) vitals.ApplyWarmth(def.warmth);
            if (def.id == "water_dirty") return "Kirli su içtin. Midesi bulanıyor (-10 sağlık).";
            return def.name + " kullanıldı.";
        }

        /// <summary>Uses the best available item of a category (e.g. quick key "1 = eat").</summary>
        public string ConsumeBest(ItemCategory category, SurvivorVitals vitals)
        {
            string[] order;
            switch (category)
            {
                case ItemCategory.Food: order = new[] { "hot_meal", "canned_food", "crackers" }; break;
                case ItemCategory.Drink: order = new[] { "water_clean", "water_dirty" }; break;
                case ItemCategory.Medical: order = new[] { "bandage" }; break;
                default: return null;
            }
            foreach (string id in order)
                if (CountOf(id) > 0) return Consume(id, vitals);
            return category == ItemCategory.Food ? "Yiyecek yok."
                : category == ItemCategory.Drink ? "İçecek yok." : "Bandaj yok.";
        }

        /// <summary>Sells salvage only (food, water and materials are kept).</summary>
        public int SellSalvage()
        {
            int value = 0;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (ItemDatabase.CategoryOf(items[i].id) != ItemCategory.Salvage) continue;
                value += items[i].saleValue;
                items.RemoveAt(i);
            }
            credits += value;
            return value;
        }

        /// <summary>Legacy: sells everything.</summary>
        public int SellAll()
        {
            int value = 0;
            foreach (ItemStack item in items) value += item.saleValue;
            credits += value;
            items.Clear();
            return value;
        }
    }
}
