using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class ShelterStorageTests
    {
        private GameObject player;
        private GameObject chest;
        private AfterrustInventory backpack;
        private ShelterStorage storage;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Storage Test Player");
            backpack = player.AddComponent<AfterrustInventory>();
            chest = new GameObject("Personal Storage");
            storage = chest.AddComponent<ShelterStorage>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(chest);
        }

        [Test]
        public void StoreAndTakeOne_PreservesIndividualConditionAndTotalItemCount()
        {
            Assert.IsTrue(backpack.TryAdd("alternator", "Alternatör", 6f, 170, 43));
            Assert.IsTrue(backpack.TryAdd("alternator", "Alternatör", 6f, 190, 92));
            Assert.AreEqual(2, backpack.CountOf("alternator"));

            StringAssert.Contains("depoya kondu", storage.StoreOne(backpack, "alternator"));
            Assert.AreEqual(1, backpack.CountOf("alternator"));
            Assert.AreEqual(1, storage.items.Count);
            Assert.AreEqual(43, storage.items[0].condition);
            Assert.AreEqual(170, storage.items[0].saleValue);

            StringAssert.Contains("çantaya alındı", storage.TakeOne(backpack, "alternator"));
            Assert.AreEqual(2, backpack.CountOf("alternator"));
            Assert.AreEqual(0, storage.items.Count);
            Assert.IsTrue(backpack.items.Exists(x => x.condition == 43 && x.saleValue == 170));
        }

        [Test]
        public void FullChest_CannotAcceptItem_AndDoesNotRemoveFromBackpack()
        {
            backpack.TryAdd("battery", "Akü", 14f, 80, 66);
            storage.capacityKg = 5f;
            StringAssert.Contains("yeterli yer yok", storage.StoreOne(backpack, "battery"));
            Assert.AreEqual(1, backpack.CountOf("battery"));
            Assert.AreEqual(0, storage.items.Count);
        }

        [Test]
        public void FullBackpack_CannotWithdraw_AndDoesNotDeleteStoredItem()
        {
            backpack.capacityKg = 10f;
            backpack.TryAdd("scrap", "Hurda", 10f, 5, 75);
            storage.Restore(new System.Collections.Generic.List<AfterrustInventory.ItemStack>
            {
                new AfterrustInventory.ItemStack
                {
                    id = "wire", displayName = "Kablo", massKg = 0.3f,
                    saleValue = 20, condition = 61
                }
            });

            StringAssert.Contains("yeterli yer yok", storage.TakeOne(backpack, "wire"));
            Assert.AreEqual(1, storage.items.Count);
            Assert.AreEqual(1, backpack.CountOf("scrap"));
            Assert.AreEqual(0, backpack.CountOf("wire"));
        }

        [Test]
        public void SnapshotRestore_PreservesItemData_AndNoAliasing()
        {
            backpack.TryAdd("wire", "Kablo", 0.3f, 20, 58);
            storage.StoreOne(backpack, "wire");
            var snapshot = storage.Snapshot();
            storage.items[0].condition = 1;
            Assert.AreEqual(58, snapshot[0].condition, "Saved state must be a deep copy");
            storage.Restore(snapshot);
            Assert.AreEqual(58, storage.items[0].condition);

            SaveSystem.SaveData save = new SaveSystem.SaveData();
            save.storages.Add(new SaveSystem.StorageState
            {
                id = "Shelter/Personal Storage", items = storage.Snapshot()
            });
            SaveSystem.SaveData roundtrip = JsonUtility.FromJson<SaveSystem.SaveData>(
                JsonUtility.ToJson(save));
            Assert.AreEqual(1, roundtrip.storages.Count);
            Assert.AreEqual("wire", roundtrip.storages[0].items[0].id);
            Assert.AreEqual(58, roundtrip.storages[0].items[0].condition);
        }

        [Test]
        public void OldV2SaveWithoutStoragesStillParses()
        {
            string oldJson = JsonUtility.ToJson(new SaveSystem.SaveData());
            oldJson = System.Text.RegularExpressions.Regex.Replace(oldJson,
                ",?\\\"storages\\\"\\s*:\\s*\\[\\s*\\]", "");
            Assert.IsTrue(SaveSystem.TryParseSaveJson(oldJson, out SaveSystem.SaveData result));
            Assert.NotNull(result);
        }
    }
}
