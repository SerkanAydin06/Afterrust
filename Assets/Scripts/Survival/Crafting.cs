using System;
using System.Collections.Generic;
using System.Text;

namespace Afterrust
{
    public struct Ingredient
    {
        public string id;
        public int count;
        public Ingredient(string id, int count) { this.id = id; this.count = count; }
    }

    public enum CraftStation { Workbench, Stove }

    public sealed class Recipe
    {
        public CraftStation station = CraftStation.Workbench;
        public string id;
        public string name;
        public string description;
        public Ingredient[] inputs;
        public string outputItem;      // catalogue item, or null for upgrades/tools
        public int outputCount = 1;
        public Func<AfterrustInventory, bool> isAvailable;   // e.g. one-time upgrades
        public Action<AfterrustInventory> onCrafted;         // tools / upgrades
    }

    /// <summary>Workbench recipes and the craft rule (all-or-nothing, respects carry weight).</summary>
    public static class Crafting
    {
        public const float LargeBackpackBonusKg = 15f;

        public static readonly List<Recipe> Recipes = new List<Recipe>
        {
            new Recipe
            {
                id = "bandage", name = "Bandaj", description = "Sağlığı 25 artırır.",
                inputs = new[] { new Ingredient("cloth", 2) }, outputItem = "bandage"
            },
            new Recipe
            {
                id = "filter_water", name = "Suyu süz", description = "Kirli suyu kömür ve bezle içilebilir yapar.",
                inputs = new[] { new Ingredient("water_dirty", 1), new Ingredient("charcoal", 1), new Ingredient("cloth", 1) },
                outputItem = "water_clean"
            },
            new Recipe
            {
                id = "wire_cutter", name = "Kablo Kesici", description = "Kabloları sökmeni sağlar.",
                inputs = new[] { new Ingredient("scrap", 1), new Ingredient("duct_tape", 1) },
                isAvailable = inv => !inv.hasWireCutter,
                onCrafted = inv => inv.hasWireCutter = true
            },
            new Recipe
            {
                id = "large_backpack", name = "Büyük Sırt Çantası", description = "Taşıma kapasitesi +15 kg.",
                inputs = new[] { new Ingredient("cloth", 4), new Ingredient("duct_tape", 2), new Ingredient("wire", 1) },
                isAvailable = inv => !inv.hasLargeBackpack,
                onCrafted = inv => { inv.hasLargeBackpack = true; inv.capacityKg += LargeBackpackBonusKg; }
            },
            new Recipe
            {
                id = "recycle_alternator", name = "Alternatörü geri dönüştür", description = "Alternatörden 3 bakır tel çıkarır.",
                inputs = new[] { new Ingredient("alternator", 1) }, outputItem = "wire", outputCount = 3
            },
            // ---- stove (needs a lit fire)
            new Recipe
            {
                station = CraftStation.Stove, id = "boil_water", name = "Suyu kaynat", description = "Kirli suyu güvenle içilir yapar.",
                inputs = new[] { new Ingredient("water_dirty", 1) }, outputItem = "water_clean"
            },
            new Recipe
            {
                station = CraftStation.Stove, id = "hot_meal", name = "Konserveyi ısıt", description = "Sıcak yemek: daha tok tutar ve ısıtır.",
                inputs = new[] { new Ingredient("canned_food", 1) }, outputItem = "hot_meal"
            },
            new Recipe
            {
                station = CraftStation.Stove, id = "make_charcoal", name = "Kömür yap", description = "2 odundan 1 kömür (süzgeç için).",
                inputs = new[] { new Ingredient("wood", 2) }, outputItem = "charcoal"
            }
        };

        public static IEnumerable<Recipe> For(CraftStation station)
        {
            foreach (Recipe r in Recipes)
                if (r.station == station) yield return r;
        }

        public static bool IsAvailable(Recipe r, AfterrustInventory inv)
        {
            return r.isAvailable == null || r.isAvailable(inv);
        }

        public static bool HasIngredients(Recipe r, AfterrustInventory inv)
        {
            foreach (Ingredient i in r.inputs)
                if (inv.CountOf(i.id) < i.count) return false;
            return true;
        }

        /// <summary>Mass change if crafted (outputs minus consumed inputs).</summary>
        public static float MassDelta(Recipe r, AfterrustInventory inv)
        {
            float delta = 0f;
            foreach (Ingredient i in r.inputs)
            {
                // Inventory.TryRemove removes matching units from the END of the list.
                // Use the same order; salvage of the same type can have different weights.
                int left = i.count;
                for (int j = inv.items.Count - 1; j >= 0 && left > 0; j--)
                {
                    AfterrustInventory.ItemStack item = inv.items[j];
                    if (item.id != i.id) continue;
                    delta -= item.massKg;
                    left--;
                }
            }
            ItemDef output = ItemDatabase.Get(r.outputItem);
            if (output != null) delta += output.massKg * r.outputCount;
            return delta;
        }

        public static bool CanCraft(Recipe r, AfterrustInventory inv)
        {
            return inv != null && r != null && (r.outputItem == null || ItemDatabase.Get(r.outputItem) != null) &&
                   IsAvailable(r, inv) && HasIngredients(r, inv) &&
                   inv.CurrentMassKg + MassDelta(r, inv) <= inv.capacityKg + 0.0001f;
        }

        public static string Craft(Recipe r, AfterrustInventory inv)
        {
            if (inv == null) return "Envanter yok.";
            if (r == null || (r.outputItem != null && ItemDatabase.Get(r.outputItem) == null))
                return "Tarif hatalı.";
            if (!IsAvailable(r, inv)) return r.name + " zaten var.";
            if (!HasIngredients(r, inv)) return "Malzeme eksik: " + Describe(r, inv);
            if (!CanCraft(r, inv)) return "Çanta dolu.";
            // Snapshot preserves individual salvage condition, mass and price on rollback.
            List<AfterrustInventory.ItemStack> original = new List<AfterrustInventory.ItemStack>(inv.items);
            foreach (Ingredient i in r.inputs) inv.TryRemove(i.id, i.count);
            if (r.outputItem != null && inv.TryAddItem(r.outputItem, r.outputCount) != r.outputCount)
            {
                inv.items = original;
                return "Üretim tamamlanamadı; malzemeler geri verildi.";
            }
            r.onCrafted?.Invoke(inv);
            return r.name + " üretildi.";
        }

        /// <summary>"Bez 1/2, Bant 0/1"</summary>
        public static string Describe(Recipe r, AfterrustInventory inv)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Ingredient i in r.inputs)
            {
                if (sb.Length > 0) sb.Append(", ");
                ItemDef def = ItemDatabase.Get(i.id);
                sb.Append(def != null ? def.name : i.id).Append(' ')
                  .Append(inv != null ? inv.CountOf(i.id) : 0).Append('/').Append(i.count);
            }
            return sb.ToString();
        }
    }
}
