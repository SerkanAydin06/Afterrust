using System.Collections.Generic;
using UnityEngine;

namespace Afterrust
{
    public sealed class AfterrustInventory : MonoBehaviour
    {
        [System.Serializable]
        public class ItemStack
        {
            public string id;
            public string displayName;
            public float massKg;
            public int saleValue;
        }

        [Min(1f)] public float capacityKg = 35f;
        public bool hasWrench = true;
        public int credits = 0;
        public List<ItemStack> items = new List<ItemStack>();

        public float CurrentMassKg
        {
            get
            {
                float sum = 0f;
                foreach (ItemStack item in items) sum += item.massKg;
                return sum;
            }
        }

        public bool TryAdd(string id, string displayName, float massKg, int saleValue)
        {
            if (massKg < 0f || CurrentMassKg + massKg > capacityKg) return false;
            items.Add(new ItemStack { id = id, displayName = displayName, massKg = massKg, saleValue = saleValue });
            return true;
        }

        public int SellAll()
        {
            int value = 0;
            foreach (ItemStack item in items) value += item.saleValue;
            credits += value;
            items.Clear();
            return value;
        }
    }
}
