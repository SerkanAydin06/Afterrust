using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class SurvivalRulesTests
    {
        private GameObject host;
        private AfterrustInventory inv;
        private SurvivorVitals vitals;

        [SetUp]
        public void Setup()
        {
            host = new GameObject("Survivor");
            inv = host.AddComponent<AfterrustInventory>();
            vitals = host.AddComponent<SurvivorVitals>();
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        private static Recipe RecipeById(string id)
        {
            return Crafting.Recipes.Find(r => r.id == id);
        }

        [Test]
        public void Vitals_DecayOverTime_AndStarvationKills()
        {
            vitals.Tick(60f);
            Assert.Less(vitals.hunger, 100f);
            Assert.Less(vitals.thirst, vitals.hunger, "thirst drops faster than hunger");

            bool died = false;
            vitals.Died += () => died = true;
            vitals.hunger = 0f;
            vitals.thirst = 0f;
            for (int i = 0; i < 400 && !vitals.IsDead; i++) vitals.Tick(1f);
            Assert.IsTrue(vitals.IsDead);
            Assert.IsTrue(died);
        }

        [Test]
        public void Vitals_RegenerateWhenFedAndWatered()
        {
            vitals.health = 50f;
            vitals.Tick(10f);
            Assert.Greater(vitals.health, 50f);
        }

        [Test]
        public void Consume_RestoresNeeds_AndRemovesItem()
        {
            vitals.hunger = 20f;
            Assert.AreEqual(1, inv.TryAddItem("canned_food"));
            Assert.IsNotNull(inv.Consume("canned_food", vitals));
            Assert.AreEqual(55f, vitals.hunger, 0.01f);
            Assert.AreEqual(0, inv.CountOf("canned_food"));
            Assert.IsNull(inv.Consume("cloth", vitals), "materials are not consumable");
        }

        [Test]
        public void DirtyWater_QuenchesButHurts()
        {
            vitals.thirst = 10f;
            inv.TryAddItem("water_dirty");
            inv.Consume("water_dirty", vitals);
            Assert.AreEqual(35f, vitals.thirst, 0.01f);
            Assert.AreEqual(90f, vitals.health, 0.01f);
        }

        [Test]
        public void Craft_Bandage_ConsumesCloth()
        {
            Recipe bandage = RecipeById("bandage");
            inv.TryAddItem("cloth", 1);
            Assert.IsFalse(Crafting.CanCraft(bandage, inv));
            inv.TryAddItem("cloth", 1);
            Assert.IsTrue(Crafting.CanCraft(bandage, inv));
            Crafting.Craft(bandage, inv);
            Assert.AreEqual(0, inv.CountOf("cloth"));
            Assert.AreEqual(1, inv.CountOf("bandage"));
        }

        [Test]
        public void Craft_LargeBackpack_OnlyOnce_AndRaisesCapacity()
        {
            Recipe pack = RecipeById("large_backpack");
            inv.TryAddItem("cloth", 8);
            inv.TryAddItem("duct_tape", 4);
            inv.TryAddItem("wire", 2);
            float before = inv.capacityKg;
            Crafting.Craft(pack, inv);
            Assert.AreEqual(before + Crafting.LargeBackpackBonusKg, inv.capacityKg, 0.001f);
            Assert.IsFalse(Crafting.CanCraft(pack, inv), "one-time upgrade");
        }

        [Test]
        public void Craft_WireCutter_EnablesTool()
        {
            Recipe cutter = RecipeById("wire_cutter");
            inv.TryAddItem("scrap");
            inv.TryAddItem("duct_tape");
            Crafting.Craft(cutter, inv);
            Assert.IsTrue(inv.hasWireCutter);
            Assert.IsTrue(inv.HasTool(SalvageTool.WireCutter));
        }

        [Test]
        public void Loot_RespectsCarryWeight_AndKeepsLeftovers()
        {
            GameObject boxGo = new GameObject("Box");
            try
            {
                LootContainer box = boxGo.AddComponent<LootContainer>();
                box.table = new[] { new LootEntry("scrap", 1f, 3, 3) };   // 3 x 3 kg
                inv.capacityKg = 7f;
                AfterrustInteraction player = host.AddComponent<AfterrustInteraction>();
                player.inventory = inv;

                box.Interact(player);
                Assert.AreEqual(2, inv.CountOf("scrap"));
                Assert.IsFalse(box.IsEmpty);

                inv.TryRemove("scrap", 2);
                box.Interact(player);
                Assert.AreEqual(1, inv.CountOf("scrap"));
                Assert.IsTrue(box.IsEmpty);
            }
            finally
            {
                Object.DestroyImmediate(boxGo);
            }
        }

        [Test]
        public void Trader_SellsOnlySalvage()
        {
            inv.TryAddItem("canned_food");
            inv.TryAdd("battery", "Akü", 14f, 80, 50);
            int value = inv.SellSalvage();
            Assert.AreEqual(80, value);
            Assert.AreEqual(1, inv.CountOf("canned_food"));
            Assert.AreEqual(0, inv.CountOf("battery"));
        }

        [Test]
        public void Craft_MassDeltaMatchesActualLastStackRemoval()
        {
            Recipe recycle = RecipeById("recycle_alternator");
            // Same id can have different mass; TryRemove removes the LAST matching stack.
            Assert.IsTrue(inv.TryAdd("alternator", "Heavy alternator", 18f, 100));
            Assert.IsTrue(inv.TryAdd("alternator", "Light alternator", 1.5f, 100));
            float outputMass = ItemDatabase.Get("wire").massKg * recycle.outputCount;
            Assert.AreEqual(outputMass - 1.5f, Crafting.MassDelta(recycle, inv), 0.0001f);

            Assert.IsTrue(Crafting.CanCraft(recycle, inv));
            Crafting.Craft(recycle, inv);
            Assert.AreEqual(1, inv.CountOf("alternator"));
            Assert.AreEqual(18f + outputMass, inv.CurrentMassKg, 0.0001f);
        }

        [Test]
        public void Craft_UnknownOutputDoesNotConsumeInputs()
        {
            inv.TryAddItem("cloth");
            Recipe invalid = new Recipe
            {
                id = "invalid", name = "Invalid",
                inputs = new[] { new Ingredient("cloth", 1) }, outputItem = "not_in_database"
            };
            Assert.IsFalse(Crafting.CanCraft(invalid, inv));
            Assert.AreEqual("Tarif hatalı.", Crafting.Craft(invalid, inv));
            Assert.AreEqual(1, inv.CountOf("cloth"));
        }

    }
}
