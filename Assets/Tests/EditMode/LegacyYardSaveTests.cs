using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class LegacyYardSaveTests
    {
        [Test]
        public void V9LootPathsStillMatchAfterRemovingThreeOldBarrels()
        {
            GameObject shelter = new GameObject("Shelter");
            try
            {
                GameObject yard = new GameObject("Yard Props");
                yard.transform.SetParent(shelter.transform, false);

                // The old v9 order: decorative props, 3 barrels, then saved
                // pallet/woodpile/shipping containers. v10 deletes the barrels.
                for (int i = 0; i < 6; i++)
                    new GameObject("Tire " + i).transform.SetParent(yard.transform, false);

                GameObject[] oldBarrels = new GameObject[3];
                for (int i = 0; i < 3; i++)
                {
                    oldBarrels[i] = new GameObject("Oil Drum");
                    oldBarrels[i].transform.SetParent(yard.transform, false);
                }

                GameObject pallet = new GameObject("Pallet Stack");
                pallet.transform.SetParent(yard.transform, false);
                GameObject wood = new GameObject("Woodpile");
                wood.transform.SetParent(yard.transform, false);
                GameObject container = new GameObject("Shipping Container");
                container.transform.SetParent(yard.transform, false);
                GameObject nested = new GameObject("Loot Chest");
                nested.transform.SetParent(container.transform, false);

                string oldPallet = SaveSystem.PathOf(pallet.transform);
                string oldWood = SaveSystem.PathOf(wood.transform);
                string oldContainer = SaveSystem.PathOf(nested.transform);

                for (int i = 2; i >= 0; i--) Object.DestroyImmediate(oldBarrels[i]);

                Assert.AreNotEqual(oldPallet, SaveSystem.PathOf(pallet.transform));
                Assert.AreEqual(oldPallet, SaveSystem.LegacyV9YardPathOf(pallet.transform));
                Assert.AreEqual(oldWood, SaveSystem.LegacyV9YardPathOf(wood.transform));
                Assert.AreEqual(oldContainer, SaveSystem.LegacyV9YardPathOf(nested.transform));

                GameObject other = new GameObject("Other Object");
                other.transform.SetParent(yard.transform, false);
                Assert.IsNull(SaveSystem.LegacyV9YardPathOf(other.transform));
            }
            finally
            {
                Object.DestroyImmediate(shelter);
            }
        }
    }
}
