// ============================================================
// InventorySystem.cs — inventory, stacking, capacity, currency
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class AddItemResult
    {
        public Player Player;
        public int Added;
        public int Overflow;
    }

    public sealed class CurrencyResult
    {
        public Player Player;
        public bool Success;
        public int Amount;
    }

    public static class InventorySystem
    {
        public static bool HasItem(Player player, string itemId, int count = 1) => CountItem(player, itemId) >= count;
        public static int CountItem(Player player, string itemId) => player.Inventory.Where(x => x.ItemId == itemId).Sum(x => Math.Max(0, x.Count));
        public static int GetUsedSlots(Player player) => player.Inventory.Count;
        public static bool IsInventoryFull(Player player) => player.Inventory.Count >= player.InventoryCapacity;

        public static AddItemResult AddItem(GameDatabase db, Player player, string itemId, int count = 1)
        {
            if (count <= 0) return new AddItemResult { Player = player, Added = 0, Overflow = 0 };
            if (db == null || !db.Items.TryGetValue(itemId, out var def))
                return new AddItemResult { Player = player, Added = 0, Overflow = count };

            int remaining = count;
            if (def.Stackable)
            {
                int maxStack = Math.Max(1, def.MaxStack);
                var existing = player.Inventory.FirstOrDefault(s => s.ItemId == itemId && s.Count < maxStack);
                if (existing != null)
                {
                    int toAdd = Math.Min(remaining, maxStack - existing.Count);
                    existing.Count += toAdd;
                    remaining -= toAdd;
                }
            }

            while (remaining > 0 && player.Inventory.Count < player.InventoryCapacity)
            {
                int toAdd = def.Stackable ? Math.Min(remaining, Math.Max(1, def.MaxStack)) : 1;
                player.Inventory.Add(new InventorySlot { ItemId = itemId, Count = toAdd });
                remaining -= toAdd;
            }

            return new AddItemResult { Player = player, Added = count - remaining, Overflow = remaining };
        }

        public static void AddItem(Player player, string itemId, int count)
        {
            if (count <= 0) return;
            var slot = player.Inventory.FirstOrDefault(x => x.ItemId == itemId);
            if (slot == null) player.Inventory.Add(new InventorySlot { ItemId = itemId, Count = count });
            else slot.Count += count;
        }

        public static bool RemoveItem(Player player, string itemId, int count = 1)
        {
            if (count <= 0) return true;
            if (!HasItem(player, itemId, count)) return false;
            int remaining = count;
            for (int i = player.Inventory.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = player.Inventory[i];
                if (slot.ItemId != itemId) continue;
                int take = Math.Min(remaining, slot.Count);
                slot.Count -= take;
                remaining -= take;
                if (slot.Count <= 0) player.Inventory.RemoveAt(i);
            }
            return true;
        }

        public static CurrencyResult AddGold(Player player, int amount)
        {
            if (amount <= 0) return new CurrencyResult { Player = player, Success = false, Amount = 0 };
            player.Gold += amount;
            return new CurrencyResult { Player = player, Success = true, Amount = amount };
        }

        public static CurrencyResult SpendGold(Player player, int amount)
        {
            if (amount <= 0) return new CurrencyResult { Player = player, Success = true, Amount = 0 };
            if (player.Gold < amount) return new CurrencyResult { Player = player, Success = false, Amount = amount };
            player.Gold -= amount;
            return new CurrencyResult { Player = player, Success = true, Amount = amount };
        }
    }
}
