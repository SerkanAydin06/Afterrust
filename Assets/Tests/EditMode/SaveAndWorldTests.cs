using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class SaveAndWorldTests
    {
        [Test]
        public void SaveData_RoundTripsThroughJson()
        {
            SaveSystem.SaveData d = new SaveSystem.SaveData
            {
                position = new Vector3(1f, 2f, 3f), yaw = 90f, health = 42f, credits = 7, wireCutter = true,
                timeOfDay = 21.5f, day = 3, raining = true
            };
            d.items.Add(new AfterrustInventory.ItemStack { id = "bandage", displayName = "Bandaj", massKg = 0.05f, saleValue = 6, condition = 100 });
            d.containers.Add(new SaveSystem.ContainerState { id = "Shelter/Box#2", rolled = true, contents = new List<string> { "cloth" } });

            SaveSystem.SaveData back = JsonUtility.FromJson<SaveSystem.SaveData>(JsonUtility.ToJson(d));
            Assert.AreEqual(SaveSystem.FormatVersion, back.version);
            Assert.AreEqual(d.position, back.position);
            Assert.AreEqual(42f, back.health);
            Assert.IsTrue(back.wireCutter);
            Assert.AreEqual("bandage", back.items[0].id);
            Assert.AreEqual("cloth", back.containers[0].contents[0]);
            Assert.AreEqual(3, back.day);
            Assert.IsTrue(back.raining);
        }

        [Test]
        public void PathOf_IsStableAndDistinguishesDuplicates()
        {
            GameObject root = new GameObject("Shelter");
            try
            {
                GameObject a = new GameObject("Box");
                GameObject b = new GameObject("Box");
                a.transform.SetParent(root.transform);
                b.transform.SetParent(root.transform);
                Assert.AreEqual("Shelter/Box#0", SaveSystem.PathOf(a.transform));
                Assert.AreEqual("Shelter/Box#1", SaveSystem.PathOf(b.transform));
                Assert.AreEqual("Shelter", SaveSystem.PathOf(root.transform));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LootContainer_RestoresSearchedState()
        {
            GameObject go = new GameObject("Crate");
            try
            {
                LootContainer c = go.AddComponent<LootContainer>();
                c.Restore(true, new List<string>());
                Assert.IsTrue(c.IsEmpty);
                Assert.AreEqual(0f, c.HoldSeconds, "searched containers open instantly");
                c.Restore(true, new List<string> { "cloth" });
                Assert.IsFalse(c.IsEmpty);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void World_RestoreClampsTimeAndShowsClock()
        {
            GameObject go = new GameObject("World");
            try
            {
                DayNightWeather w = go.AddComponent<DayNightWeather>();
                w.Restore(25.25f, 0, true, 30f);
                Assert.AreEqual(1.25f, w.timeOfDay, 0.001f);
                Assert.AreEqual(1, w.day);
                Assert.IsTrue(w.IsRaining);
                StringAssert.Contains("01:15", w.ClockText);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    
        [Test]
        public void Door_TogglesAndRestoresInstantly()
        {
            GameObject go = new GameObject("Shelter Door");
            try
            {
                Door door = go.AddComponent<Door>();
                Assert.AreEqual("E - Kapıyı aç", door.GetPrompt(null));
                door.Interact(null);
                Assert.IsTrue(door.isOpen);
                Assert.AreEqual("E - Kapıyı kapat", door.GetPrompt(null));
                door.SetOpen(true, true);
                Assert.AreEqual(door.openAngle, door.Angle, 0.001f);
                door.SetOpen(false, true);
                Assert.AreEqual(0f, door.Angle, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SaveJson_RejectsCorruptionAndOldFormats()
        {
            SaveSystem.SaveData expected = new SaveSystem.SaveData
            {
                health = 77f, warmth = 45f, energy = 80f
            };
            string json = JsonUtility.ToJson(expected);
            Assert.IsTrue(SaveSystem.TryParseSaveJson(json, out SaveSystem.SaveData parsed));
            Assert.AreEqual(77f, parsed.health, 0.01f);
            Assert.IsFalse(SaveSystem.TryParseSaveJson("{invalid JSON", out _));
            Assert.IsFalse(SaveSystem.TryParseSaveJson("{\"version\":1}", out _));
            Assert.IsFalse(SaveSystem.TryParseSaveJson("", out _));
        }

        [Test]
        public void SaveJson_RecoversFromBackupWhenPrimaryIsCorrupt()
        {
            string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "afterrust-test-" + System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(folder);
            string primary = System.IO.Path.Combine(folder, "save.json");
            string backup = primary + ".bak";
            try
            {
                System.IO.File.WriteAllText(primary, "{invalid");
                System.IO.File.WriteAllText(backup, JsonUtility.ToJson(new SaveSystem.SaveData { health = 64f }));
                Assert.IsTrue(SaveSystem.TryReadSave(primary, backup, out SaveSystem.SaveData data));
                Assert.AreEqual(64f, data.health, 0.01f);
            }
            finally
            {
                System.IO.Directory.Delete(folder, true);
            }
        }

    }
}
