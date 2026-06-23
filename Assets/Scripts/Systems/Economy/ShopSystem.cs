// ============================================================
// ShopSystem.cs — buy/sell, charisma and karma pricing
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class ShopResult
    {
        public Player Player;
        public bool Success;
        public int Count;
        public int GoldDelta;
        public string Message;
    }

    public static class ShopSystem
    {
        public static IEnumerable<ShopEntryDef> GetAllShopGoods(GameDatabase db) => db.ShopEntries.Values;

        public static IEnumerable<ShopEntryDef> GetShopGoodsForRegion(GameDatabase db, Player player)
        {
            var region = EconomyStateHelpers.GetCurrentRegion(db, player);
            var tags = region?.RegionTags ?? new List<string>();
            return db.ShopEntries.Values.Where(g => (g.RegionTags == null || g.RegionTags.Count == 0 || g.RegionTags.Any(tags.Contains))
                && (string.IsNullOrEmpty(g.RequiredAlignment) || GetAlignment(player.Karma) == g.RequiredAlignment));
        }

        public static int CalcBuyPrice(int basePrice, int charisma)
        {
            double discount = 1 - (charisma / 100.0) * 0.3;
            return Math.Max(1, (int)Math.Floor(basePrice * discount));
        }

        public static int CalcKarmaBuyPrice(int basePrice, int charisma, int karma)
        {
            string alignment = GetAlignment(karma);
            double factor = alignment == "righteous" ? 0.95 : alignment == "evil" ? 1.08 : 1;
            return Math.Max(1, (int)Math.Floor(CalcBuyPrice(basePrice, charisma) * factor));
        }

        public static int CalcSellPrice(ItemDef itemDef) => itemDef.SellPrice;

        public static ShopResult BuyItem(GameDatabase db, Player player, string itemId, int count = 1)
        {
            var good = db.ShopEntries.Values.FirstOrDefault(g => g.ItemId == itemId);
            if (good == null) return new ShopResult { Player = player, Message = EconomyTexts.ItemNotFound };
            if (!string.IsNullOrEmpty(good.RequiredAlignment) && GetAlignment(player.Karma) != good.RequiredAlignment) return new ShopResult { Player = player, Message = EconomyTexts.ItemNotFound };
            if (!db.Items.TryGetValue(itemId, out var def)) return new ShopResult { Player = player, Message = EconomyTexts.ItemNotFound };
            int safeCount = Math.Max(1, count);
            int unitPrice = CalcKarmaBuyPrice(good.BuyPrice ?? 0, player.Charisma, player.Karma);
            int totalCost = unitPrice * safeCount;
            if (player.Gold < totalCost) return new ShopResult { Player = player, Message = EconomyTexts.GoldInsufficient };
            var added = InventorySystem.AddItem(db, player, itemId, safeCount);
            if (added.Added == 0) return new ShopResult { Player = player, Message = EconomyTexts.InventoryFull };
            int actualCost = unitPrice * added.Added;
            player.Gold -= actualCost;
            return new ShopResult { Player = player, Success = true, Count = added.Added, GoldDelta = -actualCost, Message = EconomyTexts.Success };
        }

        public static ShopResult SellItem(GameDatabase db, Player player, string itemId, int count = 1)
        {
            if (!db.Items.TryGetValue(itemId, out var def)) return new ShopResult { Player = player, Message = EconomyTexts.ItemNotFound };
            int safeCount = Math.Max(1, count);
            if (!InventorySystem.HasItem(player, itemId, safeCount)) return new ShopResult { Player = player, Message = EconomyTexts.MaterialInsufficient };
            int totalGold = CalcSellPrice(def) * safeCount;
            InventorySystem.RemoveItem(player, itemId, safeCount);
            player.Gold += totalGold;
            return new ShopResult { Player = player, Success = true, Count = safeCount, GoldDelta = totalGold, Message = EconomyTexts.Success };
        }

        private static string GetAlignment(int karma)
        {
            if (karma >= 30) return "righteous";
            if (karma <= -30) return "evil";
            return "neutral";
        }
    }
}
