using NUnit.Framework;
using UnityEngine;

namespace Afterrust.Tests
{
    public sealed class TraderMarketTests
    {
        private GameObject playerObject;
        private GameObject traderObject;
        private AfterrustInventory inventory;
        private AfterrustTrader trader;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Trade Test Player");
            traderObject = new GameObject("Trade Test Merchant");
            inventory = playerObject.AddComponent<AfterrustInventory>();
            trader = traderObject.AddComponent<AfterrustTrader>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(traderObject);
        }

        [Test]
        public void SellOne_RemovesExactlyOneSalvageAndPreservesFoodAndOtherParts()
        {
            inventory.TryAdd("alternator", "Alternatör", 6f, 95, 40);
            inventory.TryAdd("alternator", "Alternatör", 6f, 175, 90);
            inventory.TryAdd("canned_food", "Konserve Yemek", 0.4f, 12);

            StringAssert.Contains("satıldı", trader.SellOne(inventory, "alternator"));
            Assert.AreEqual(95, inventory.credits);
            Assert.AreEqual(1, inventory.CountOf("alternator"));
            Assert.AreEqual(1, inventory.CountOf("canned_food"));
            Assert.AreEqual(175, inventory.items.Find(i => i.id == "alternator").saleValue);
            Assert.AreEqual(90, inventory.items.Find(i => i.id == "alternator").condition);
        }

        [Test]
        public void CannotSellSupplies_OrUnknownMissingItems()
        {
            inventory.TryAdd("cloth", "Bez", 0.15f, 2);
            Assert.AreEqual("Tüccar yalnızca hurda satın alıyor.", trader.SellOne(inventory, "cloth"));
            Assert.AreEqual(1, inventory.CountOf("cloth"));
            Assert.AreEqual(0, inventory.credits);
            Assert.AreEqual("Satılacak eşya yok.", trader.SellOne(inventory, "battery"));
        }

        [Test]
        public void BuyingRequiresMoneyAndBagSpace_AndChargesExactlyOnce()
        {
            int price = AfterrustTrader.GetPrice("water_clean");
            Assert.AreEqual(16, price);
            Assert.AreEqual("Yeterli kredi yok (" + price + " K).",
                trader.Buy(inventory, "water_clean"));
            Assert.AreEqual(0, inventory.CountOf("water_clean"));

            inventory.credits = price;
            inventory.capacityKg = 0.2f;
            Assert.AreEqual("Çantada yeterli yer yok.", trader.Buy(inventory, "water_clean"));
            Assert.AreEqual(price, inventory.credits);
            Assert.AreEqual(0, inventory.CountOf("water_clean"));

            inventory.capacityKg = 35f;
            StringAssert.Contains("satın alındı", trader.Buy(inventory, "water_clean"));
            Assert.AreEqual(1, inventory.CountOf("water_clean"));
            Assert.AreEqual(0, inventory.credits);
        }

        [Test]
        public void WireCutter_CanOnlyBeBoughtOnce()
        {
            inventory.credits = 120;
            StringAssert.Contains("alındı", trader.Buy(inventory, "wire_cutter"));
            Assert.IsTrue(inventory.hasWireCutter);
            Assert.AreEqual(60, inventory.credits);
            Assert.AreEqual("Kablo kesici zaten sende.", trader.Buy(inventory, "wire_cutter"));
            Assert.AreEqual(60, inventory.credits);
        }

        [Test]
        public void MarketOnlyOffersKnownItems_UnknownPurchasesNeverCharge()
        {
            inventory.credits = 1000;
            foreach (AfterrustTrader.Offer offer in AfterrustTrader.Offers)
            {
                Assert.Greater(offer.price, 0);
                Assert.IsTrue(offer.id == "wire_cutter" || ItemDatabase.Get(offer.id) != null);
            }
            Assert.AreEqual(-1, AfterrustTrader.GetPrice("nonexistent_item"));
            Assert.AreEqual("Bu ürün tüccarda bulunmuyor.",
                trader.Buy(inventory, "nonexistent_item"));
            Assert.AreEqual(1000, inventory.credits);
        }
    }
}
