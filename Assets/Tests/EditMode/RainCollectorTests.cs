using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class RainCollectorTests
    {
        private GameObject collector;
        private WaterSource source;

        [SetUp]
        public void SetUp()
        {
            collector = new GameObject("Rain Barrel - Open Collector");
            source = collector.AddComponent<WaterSource>();
            source.maxUnits = 3;
            source.secondsToFill = 900f;
        }

        [TearDown]
        public void TearDown()
        {
            if (collector != null) Object.DestroyImmediate(collector);
        }

        [Test]
        public void StartsEmpty_AndDryWeatherNeverFills()
        {
            Assert.AreEqual(0f, source.Fill01, 0.0001f);
            Assert.AreEqual(0, source.Units);
            source.AdvanceFill(7200f, false);
            Assert.AreEqual(0f, source.Fill01, 0.0001f);
            StringAssert.Contains("boş", source.GetPrompt(null));
        }

        [Test]
        public void OnlyRainSlowlyAccumulates_PreservingProgressAcrossDrySpells()
        {
            source.AdvanceFill(90f, true);
            Assert.AreEqual(0.10f, source.Fill01, 0.0001f);
            Assert.AreEqual(0, source.Units);

            source.AdvanceFill(10000f, false);
            Assert.AreEqual(0.10f, source.Fill01, 0.0001f);

            source.AdvanceFill(360f, true);
            Assert.AreEqual(0.50f, source.Fill01, 0.0001f);
            Assert.AreEqual(1, source.Units);

            source.AdvanceFill(10000f, true);
            Assert.AreEqual(1f, source.Fill01, 0.0001f);
            Assert.AreEqual(3, source.Units);
        }

        [Test]
        public void TakingWaterConsumesOneThird_AndDoesNotRefillWithoutRain()
        {
            GameObject playerObject = new GameObject("Test Survivor");
            try
            {
                AfterrustInventory inventory = playerObject.AddComponent<AfterrustInventory>();
                AfterrustInteraction player = playerObject.AddComponent<AfterrustInteraction>();
                player.inventory = inventory;
                source.Restore(0.66f);
                Assert.AreEqual(1, source.Units);

                StringAssert.Contains("Kirli su dolduruldu", source.Interact(player));
                Assert.AreEqual(1, inventory.CountOf("water_dirty"));
                Assert.AreEqual(0.66f - 1f / 3f, source.Fill01, 0.0001f);
                Assert.AreEqual(0, source.Units);
                Assert.AreEqual("Henüz bir şişelik su birikmedi.", source.Interact(player));
                source.AdvanceFill(300f, false);
                Assert.AreEqual(0, source.Units);
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void PartialWaterRestoresExactly_AndIsSerializable()
        {
            source.Restore(0.275f);
            Assert.AreEqual(0.275f, source.Fill01, 0.0001f);
            SaveSystem.WaterState state = new SaveSystem.WaterState
            {
                id = "Shelter/Collector", units = source.Units,
                fill01 = source.Fill01, hasContinuousFill = true
            };
            SaveSystem.WaterState loaded = JsonUtility.FromJson<SaveSystem.WaterState>(
                JsonUtility.ToJson(state));
            source.Restore(0f);
            source.Restore(loaded.fill01);
            Assert.AreEqual(0.275f, source.Fill01, 0.0001f);
            Assert.AreEqual(0, source.Units);
        }
    }
}
