using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Safe-yard merchant. Interacting opens a deliberate buy/sell panel rather
    /// than instantly selling the player's entire inventory.
    /// </summary>
    public sealed class AfterrustTrader : MonoBehaviour, IInteractable
    {
        public const int WireCutterPrice = 60;

        public readonly struct Offer
        {
            public readonly string id;
            public readonly string name;
            public readonly int price;
            public Offer(string id, string name, int price)
            {
                this.id = id;
                this.name = name;
                this.price = price;
            }
        }

        // Fixed prototype stock; can be moved to ScriptableObjects later.
        public static readonly Offer[] Offers =
        {
            new Offer("water_clean", "Temiz Su", 16),
            new Offer("canned_food", "Konserve Yemek", 24),
            new Offer("bandage", "Bandaj", 14),
            new Offer("cloth", "Bez", 6),
            new Offer("charcoal", "Kömür", 6),
            new Offer("wire_cutter", "Kablo Kesici", WireCutterPrice),
        };

        public float HoldSeconds => 0f;

        public string GetPrompt(AfterrustInteraction player) => "E - Tüccarla ticaret yap";

        public string Interact(AfterrustInteraction player)
        {
            if (player == null || player.inventory == null || player.hud == null)
                return "Tüccara erişmek için oyuncu envanteri gerekli.";
            player.hud.OpenTrader(this);
            return "";
        }

        /// <summary>Sells exactly one salvage instance, preserving other items.</summary>
        public string SellOne(AfterrustInventory inventory, string id)
        {
            if (inventory == null) return "Envanter yok.";
            if (ItemDatabase.CategoryOf(id) != ItemCategory.Salvage)
                return "Tüccar yalnızca hurda satın alıyor.";

            int index = inventory.items.FindIndex(i => i != null && i.id == id);
            if (index < 0) return "Satılacak eşya yok.";
            AfterrustInventory.ItemStack item = inventory.items[index];
            if (item.saleValue < 0 || inventory.credits > int.MaxValue - item.saleValue)
                return "Satış işlemi gerçekleştirilemedi.";

            inventory.credits += item.saleValue;
            inventory.items.RemoveAt(index);
            return item.displayName + " satıldı: +" + item.saleValue + " K";
        }

        public static int GetPrice(string id)
        {
            foreach (Offer offer in Offers)
                if (offer.id == id) return offer.price;
            return -1;
        }

        /// <summary>Purchases one known item atomically: no charge if backpack is full.</summary>
        public string Buy(AfterrustInventory inventory, string id)
        {
            if (inventory == null) return "Envanter yok.";
            int price = GetPrice(id);
            if (price < 0) return "Bu ürün tüccarda bulunmuyor.";
            if (id == "wire_cutter") return BuyWireCutter(inventory);

            ItemDef def = ItemDatabase.Get(id);
            if (def == null) return "Ürün tanımı bulunamadı.";
            if (inventory.credits < price) return "Yeterli kredi yok (" + price + " K).";

            // Don't charge credits until the inventory has accepted the item.
            if (inventory.TryAddItem(id) != 1) return "Çantada yeterli yer yok.";
            inventory.credits -= price;
            return def.name + " satın alındı: -" + price + " K";
        }

        public string BuyWireCutter(AfterrustInventory inventory)
        {
            if (inventory == null) return "Envanter yok.";
            if (inventory.hasWireCutter) return "Kablo kesici zaten sende.";
            if (inventory.credits < WireCutterPrice)
                return "Kablo kesici için " + WireCutterPrice + " kredi gerekiyor.";
            inventory.credits -= WireCutterPrice;
            inventory.hasWireCutter = true;
            return "Kablo kesici alındı.";
        }
    }
}
