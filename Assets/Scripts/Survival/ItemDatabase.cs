using System.Collections.Generic;

namespace Afterrust
{
    public enum ItemCategory { Food, Drink, Medical, Material, Salvage }

    /// <summary>Static definition of an item type. Inventory entries reference it by id.</summary>
    public sealed class ItemDef
    {
        public readonly string id;
        public readonly string name;
        public readonly float massKg;
        public readonly int value;
        public readonly ItemCategory category;
        public readonly float hunger, thirst, health, warmth;

        public ItemDef(string id, string name, float massKg, int value, ItemCategory category,
            float hunger = 0f, float thirst = 0f, float health = 0f, float warmth = 0f)
        {
            this.warmth = warmth;
            this.id = id;
            this.name = name;
            this.massKg = massKg;
            this.value = value;
            this.category = category;
            this.hunger = hunger;
            this.thirst = thirst;
            this.health = health;
        }

        public bool IsConsumable =>
            category == ItemCategory.Food || category == ItemCategory.Drink || category == ItemCategory.Medical;
    }

    /// <summary>Prototype item catalogue (code-defined; moves to data assets later).</summary>
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDef> Defs = new Dictionary<string, ItemDef>();

        static ItemDatabase()
        {
            Add(new ItemDef("canned_food", "Konserve Yemek", 0.4f, 12, ItemCategory.Food, hunger: 35f));
            Add(new ItemDef("crackers", "Kraker", 0.15f, 5, ItemCategory.Food, hunger: 12f, thirst: -3f));
            Add(new ItemDef("hot_meal", "Sıcak Yemek", 0.45f, 16, ItemCategory.Food, hunger: 45f, warmth: 20f));
            Add(new ItemDef("water_clean", "Temiz Su", 0.5f, 8, ItemCategory.Drink, thirst: 40f));
            Add(new ItemDef("water_dirty", "Kirli Su", 0.5f, 1, ItemCategory.Drink, thirst: 25f, health: -10f));
            Add(new ItemDef("bandage", "Bandaj", 0.05f, 6, ItemCategory.Medical, health: 25f));
            Add(new ItemDef("cloth", "Bez", 0.15f, 2, ItemCategory.Material));
            Add(new ItemDef("duct_tape", "Bant", 0.2f, 4, ItemCategory.Material));
            Add(new ItemDef("wire", "Bakır Tel", 0.2f, 5, ItemCategory.Material));
            Add(new ItemDef("charcoal", "Kömür", 0.3f, 2, ItemCategory.Material));
            Add(new ItemDef("wood", "Odun", 0.8f, 1, ItemCategory.Material));
            Add(new ItemDef("scrap", "Metal Hurda", 3f, 15, ItemCategory.Salvage));
            Add(new ItemDef("battery", "Araç Aküsü", 14f, 80, ItemCategory.Salvage));
            Add(new ItemDef("battery-clamp", "Akü Bağlantısı", 0.5f, 15, ItemCategory.Salvage));
            Add(new ItemDef("alternator", "Alternatör", 6f, 170, ItemCategory.Salvage));
            Add(new ItemDef("alternator-wire", "Alternatör Kablosu", 0.3f, 20, ItemCategory.Salvage));
        }

        private static void Add(ItemDef def) { Defs[def.id] = def; }

        public static ItemDef Get(string id)
        {
            return id != null && Defs.TryGetValue(id, out ItemDef def) ? def : null;
        }

        /// <summary>Unknown ids (e.g. new salvage parts) count as salvage.</summary>
        public static ItemCategory CategoryOf(string id)
        {
            ItemDef def = Get(id);
            return def != null ? def.category : ItemCategory.Salvage;
        }
    }
}
