using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class SalvageRulesTests
    {
        private GameObject host;

        [SetUp]
        public void Setup()
        {
            host = new GameObject("TestInventory");
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void SaleValue_RespectsPartCondition()
        {
            Assert.AreEqual(50, SalvageValue.Calculate(100, 50));
            Assert.AreEqual(0, SalvageValue.Calculate(100, 0));
            Assert.AreEqual(100, SalvageValue.Calculate(100, 200));
        }

        [Test]
        public void Inventory_RejectsOverweightItem()
        {
            AfterrustInventory inventory = host.AddComponent<AfterrustInventory>();
            inventory.capacityKg = 5f;
            Assert.IsTrue(inventory.TryAdd("a", "A", 3f, 10, 50));
            Assert.IsFalse(inventory.TryAdd("b", "B", 3f, 10, 50));
            Assert.AreEqual(3f, inventory.CurrentMassKg, 0.001f);
        }

        [Test]
        public void Salvage_RequiresToolAndDependencies()
        {
            AfterrustInventory inventory = host.AddComponent<AfterrustInventory>();
            inventory.hasWireCutter = false;

            GameObject clampObject = new GameObject("Clamp");
            GameObject wireObject = new GameObject("Wire");
            try
            {
                SalvageablePart clamp = clampObject.AddComponent<SalvageablePart>();
                clamp.massKg = 0.1f;

                SalvageablePart wire = wireObject.AddComponent<SalvageablePart>();
                wire.massKg = 0.1f;
                wire.requiredTool = SalvageTool.WireCutter;
                wire.removeFirst = new[] { clamp };

                Assert.AreEqual(SalvageResult.Blocked, wire.TrySalvageDetailed(inventory));
                Assert.AreEqual(SalvageResult.Success, clamp.TrySalvageDetailed(inventory));
                Assert.AreEqual(SalvageResult.MissingTool, wire.TrySalvageDetailed(inventory));
                inventory.hasWireCutter = true;
                Assert.AreEqual(SalvageResult.Success, wire.TrySalvageDetailed(inventory));
            }
            finally
            {
                Object.DestroyImmediate(clampObject);
                Object.DestroyImmediate(wireObject);
            }
        }
    }
}
