namespace Afterrust
{
    /// <summary>Barter counter: E sells salvage (food, water and materials are kept), F buys a wire cutter.</summary>
    public sealed class AfterrustTrader : UnityEngine.MonoBehaviour, IInteractable
    {
        public const int WireCutterPrice = 60;

        public float HoldSeconds => 0f;

        public string GetPrompt(AfterrustInteraction player)
        {
            return "E - Hurda sat | F - Kablo kesici al (" + WireCutterPrice + " K)";
        }

        public string Interact(AfterrustInteraction player)
        {
            if (player == null || player.inventory == null) return "Envanter yok.";
            int value = player.inventory.SellSalvage();
            return value > 0 ? "Satış tamamlandı: +" + value + " K" : "Satılacak hurda yok.";
        }

        public string BuyWireCutter(AfterrustInventory inventory)
        {
            if (inventory == null) return "Envanter yok.";
            if (inventory.hasWireCutter) return "Kablo kesici zaten sende.";
            if (inventory.credits < WireCutterPrice) return "Kablo kesici için " + WireCutterPrice + " kredi gerekiyor.";
            inventory.credits -= WireCutterPrice;
            inventory.hasWireCutter = true;
            return "Kablo kesici alındı.";
        }
    }
}
