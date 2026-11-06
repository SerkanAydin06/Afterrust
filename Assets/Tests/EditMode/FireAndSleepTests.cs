using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class FireAndSleepTests
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

        [Test]
        public void Warmth_FollowsEnvironment_AndColdHurts()
        {
            float exposedNightRain = ThermalSensor.Evaluate(55f, 1f, true, false, 0f);
            float byTheFire = ThermalSensor.Evaluate(55f, 1f, false, true, 1f);
            Assert.Less(exposedNightRain, 20f);
            Assert.Greater(byTheFire, 80f);

            vitals.warmthTarget = 0f;
            vitals.warmth = 10f;
            float before = vitals.health;
            vitals.Tick(5f);
            Assert.Less(vitals.health, before, "freezing costs health");
            Assert.Less(vitals.warmth, 10f);
        }

        [Test]
        public void Energy_DrainsAwake_AndSleepRestores()
        {
            vitals.Tick(60f);
            Assert.Less(vitals.energy, 100f);

            vitals.energy = 10f;
            vitals.hunger = 80f;
            vitals.thirst = 80f;
            SleepSystem.ApplySleep(vitals, 8f);
            Assert.AreEqual(100f, vitals.energy, 0.01f);
            Assert.Less(vitals.hunger, 80f);
            Assert.Less(vitals.thirst, 80f);
            Assert.AreEqual(12f, SleepSystem.HoursUntil(19f, 7f), 0.001f);
            Assert.AreEqual(2f, SleepSystem.HoursUntil(5f, 7f), 0.001f);
        }

        [Test]
        public void Stove_BurnsFuel_AndHeatsNearby()
        {
            GameObject go = new GameObject("Stove");
            try
            {
                Stove stove = go.AddComponent<Stove>();
                Assert.IsFalse(stove.IsLit);
                Assert.AreEqual("Odun yok.", stove.AddFuel(inv, "wood"));
                inv.TryAddItem("wood", 2);
                stove.AddFuel(inv, "wood");
                Assert.IsTrue(stove.IsLit);
                Assert.AreEqual(1, inv.CountOf("wood"));
                Assert.Greater(stove.HeatAt(go.transform.position + Vector3.right), 0.5f);
                Assert.AreEqual(0f, stove.HeatAt(go.transform.position + Vector3.right * 20f));
                stove.Burn(10000f);
                Assert.IsFalse(stove.IsLit);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Cooking_BoilWater_AndHotMealWarms()
        {
            Recipe boil = Crafting.Recipes.Find(r => r.id == "boil_water");
            Assert.AreEqual(CraftStation.Stove, boil.station);
            inv.TryAddItem("water_dirty");
            Crafting.Craft(boil, inv);
            Assert.AreEqual(1, inv.CountOf("water_clean"));

            vitals.warmth = 30f;
            inv.TryAddItem("hot_meal");
            inv.Consume("hot_meal", vitals);
            Assert.AreEqual(50f, vitals.warmth, 0.01f);
        }

        [Test]
        public void World_AdvanceHours_WrapsDay()
        {
            GameObject go = new GameObject("World");
            try
            {
                DayNightWeather w = go.AddComponent<DayNightWeather>();
                w.Restore(22f, 1, false, 100000f);
                float real = w.AdvanceHours(9f);
                Assert.AreEqual(7f, w.timeOfDay, 0.001f);
                Assert.AreEqual(2, w.day);
                Assert.AreEqual(9f * w.dayLengthMinutes * 60f / 24f, real, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
