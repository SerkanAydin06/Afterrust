using System.Collections.Generic;
using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Player-owned shelter chest. Moves full item instances (including their
    /// condition/value) between backpack and a weight-limited persistent store.
    /// No free items are generated, and failed moves change neither inventory.
    /// </summary>
    public sealed class ShelterStorage : MonoBehaviour, IInteractable
    {
        public string displayName = "Kişisel Depo";
        [Min(1f)] public float capacityKg = 120f;
        public List<AfterrustInventory.ItemStack> items = new List<AfterrustInventory.ItemStack>();

        public float HoldSeconds => 0f;

        public float CurrentMassKg
        {
            get
            {
                float total = 0f;
                foreach (AfterrustInventory.ItemStack item in items)
                    if (item != null) total += Mathf.Max(0f, item.massKg);
                return total;
            }
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            return "E - " + displayName + " (" + CurrentMassKg.ToString("0.0") +
                   " / " + capacityKg.ToString("0") + " kg)";
        }

        public string Interact(AfterrustInteraction player)
        {
            if (player == null || player.hud == null || player.inventory == null)
                return "Depo için oyuncu envanteri gerekli.";
            player.hud.OpenStorage(this);
            return "";
        }

        public List<AfterrustInventory.Group> Grouped()
        {
            var result = new List<AfterrustInventory.Group>();
            foreach (AfterrustInventory.ItemStack item in items)
            {
                if (item == null) continue;
                int index = result.FindIndex(g => g.id == item.id);
                if (index < 0)
                    result.Add(new AfterrustInventory.Group
                    {
                        id = item.id, displayName = item.displayName,
                        count = 1, massKg = item.massKg
                    });
                else
                {
                    AfterrustInventory.Group group = result[index];
                    group.count++;
                    group.massKg += item.massKg;
                    result[index] = group;
                }
            }
            return result;
        }

        /// <summary>Moves one full item instance, preserving its condition and value.</summary>
        public string StoreOne(AfterrustInventory backpack, string id)
        {
            if (backpack == null) return "Çanta bulunamadı.";
            int index = backpack.items.FindIndex(item => item != null && item.id == id);
            if (index < 0) return "Çantada bu eşyadan yok.";

            AfterrustInventory.ItemStack source = backpack.items[index];
            if (source.massKg < 0f || CurrentMassKg + source.massKg > capacityKg + 0.0001f)
                return "Depoda yeterli yer yok.";

            items.Add(Clone(source));
            backpack.items.RemoveAt(index);
            return source.displayName + " depoya kondu.";
        }

        /// <summary>Transfers only after destination weight validation succeeds.</summary>
        public string TakeOne(AfterrustInventory backpack, string id)
        {
            if (backpack == null) return "Çanta bulunamadı.";
            int index = items.FindIndex(item => item != null && item.id == id);
            if (index < 0) return "Depoda bu eşyadan yok.";

            AfterrustInventory.ItemStack source = items[index];
            if (!backpack.TryAdd(source.id, source.displayName, source.massKg,
                    source.saleValue, source.condition))
                return "Çantada yeterli yer yok.";

            items.RemoveAt(index);
            return source.displayName + " çantaya alındı.";
        }

        public List<AfterrustInventory.ItemStack> Snapshot()
        {
            var result = new List<AfterrustInventory.ItemStack>(items.Count);
            foreach (AfterrustInventory.ItemStack item in items)
                if (item != null) result.Add(Clone(item));
            return result;
        }

        public void Restore(List<AfterrustInventory.ItemStack> savedItems)
        {
            items.Clear();
            if (savedItems == null) return;
            foreach (AfterrustInventory.ItemStack item in savedItems)
                if (item != null && item.massKg >= 0f) items.Add(Clone(item));
        }

        private static AfterrustInventory.ItemStack Clone(AfterrustInventory.ItemStack src)
        {
            return new AfterrustInventory.ItemStack
            {
                id = src.id,
                displayName = src.displayName,
                massKg = src.massKg,
                saleValue = src.saleValue,
                condition = src.condition
            };
        }
    }
}
