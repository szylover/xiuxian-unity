// ============================================================
// AuctionSystem.cs — auction lots, bidding, settlement
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class AuctionLotState
    {
        public string Id, TemplateId, ItemId, HighestBidder;
        public int Count, BasePrice, CurrentBid, PlayerBid, EndsAt, AiBids;
    }

    public sealed class AuctionConsignmentState
    {
        public string Id, ItemId;
        public int Count, AskPrice, ListedAt, EndsAt;
    }

    public sealed class AuctionSystemState
    {
        public readonly List<AuctionLotState> Lots = new();
        public readonly List<AuctionConsignmentState> Consignments = new();
        public readonly List<string> History = new();
        public int LastRefreshAge = -1;
        public int CycleIndex;
    }

    public sealed class AuctionActionResult
    {
        public Player Player;
        public readonly List<string> Logs = new();
    }

    public static class AuctionSystem
    {
        public const int AuctionSize = 4;
        public const int RefreshMonths = 6;
        public const int ConsignMonths = 6;
        public const double SellFeeRate = 0.1;
        private static readonly string[] AiBidders = { "散修甲", "商会管事", "宗门弟子", "神秘客" };

        public static AuctionSystemState GetAuctionState(Player player)
        {
            if (player.Systems.TryGetValue("auction", out var existing) && existing is AuctionSystemState typed) return typed;
            var state = new AuctionSystemState();
            player.Systems["auction"] = state;
            return state;
        }

        public static AuctionActionResult EnsureAuctionHouse(GameDatabase db, Player player, IRng rng, bool force = false)
        {
            var result = SettleDueAuctions(db, player, rng, false);
            player = result.Player;
            var state = GetAuctionState(player);
            bool shouldRefresh = force || state.LastRefreshAge < 0 || player.Age - state.LastRefreshAge >= RefreshMonths || state.Lots.Count == 0;
            if (!shouldRefresh) return result;
            state.Lots.Clear();
            state.Lots.AddRange(GenerateLots(db, player, state.CycleIndex + 1));
            state.LastRefreshAge = player.Age;
            state.CycleIndex += 1;
            state.History.Insert(0, "refreshed");
            result.Logs.Add("refreshed");
            return result;
        }

        public static AuctionActionResult RefreshAuctionHouse(GameDatabase db, Player player, IRng rng) => EnsureAuctionHouse(db, player, rng, true);

        public static AuctionActionResult PlaceAuctionBid(GameDatabase db, Player player, string lotId, IRng rng, int? bidAmount = null)
        {
            var result = EnsureAuctionHouse(db, player, rng);
            player = result.Player;
            var state = GetAuctionState(player);
            var lot = state.Lots.FirstOrDefault(x => x.Id == lotId);
            if (lot == null) { result.Logs.Add(EconomyTexts.LotMissing); return result; }
            int minimum = GetNextBid(lot.CurrentBid == 0 ? lot.BasePrice : lot.CurrentBid);
            int nextBid = bidAmount ?? minimum;
            if (nextBid < minimum) { result.Logs.Add(EconomyTexts.BidTooLow); return result; }
            int extraCost = nextBid - (lot.HighestBidder == "player" ? lot.PlayerBid : 0);
            if (player.Gold < extraCost) { result.Logs.Add(EconomyTexts.GoldInsufficient); return result; }
            player.Gold -= extraCost;
            lot.CurrentBid = nextBid;
            lot.HighestBidder = "player";
            lot.PlayerBid = nextBid;
            bool raised = RunAiCompetition(lot, player, rng);
            result.Logs.Add("playerBid");
            if (lot.HighestBidder != "player")
            {
                player.Gold += nextBid;
                lot.PlayerBid = 0;
                result.Logs.Add("aiOutbid");
            }
            else if (raised) result.Logs.Add("playerLeading");
            return result;
        }

        public static AuctionActionResult ConsignAuctionItem(GameDatabase db, Player player, string itemId, int count, int askPrice, IRng rng)
        {
            var result = EnsureAuctionHouse(db, player, rng);
            player = result.Player;
            if (!db.Items.ContainsKey(itemId)) { result.Logs.Add(EconomyTexts.ItemNotFound); return result; }
            int safeCount = Math.Max(1, count), safeAsk = Math.Max(1, askPrice);
            if (!InventorySystem.HasItem(player, itemId, safeCount)) { result.Logs.Add(EconomyTexts.MaterialInsufficient); return result; }
            InventorySystem.RemoveItem(player, itemId, safeCount);
            var state = GetAuctionState(player);
            state.Consignments.Add(new AuctionConsignmentState { Id = $"consign:{player.Age}:{state.Consignments.Count}:{itemId}", ItemId = itemId, Count = safeCount, AskPrice = safeAsk, ListedAt = player.Age, EndsAt = player.Age + ConsignMonths });
            result.Logs.Add("consigned");
            return result;
        }

        public static AuctionActionResult SettleDueAuctions(GameDatabase db, Player player, IRng rng, bool force = false)
        {
            var result = new AuctionActionResult { Player = player };
            var state = GetAuctionState(player);
            var remainingLots = new List<AuctionLotState>();
            foreach (var lot in state.Lots)
            {
                if (!force && lot.EndsAt > player.Age) { remainingLots.Add(lot); continue; }
                if (lot.HighestBidder == "player")
                {
                    var added = InventorySystem.AddItem(db, player, lot.ItemId, lot.Count);
                    if (added.Overflow > 0) player.Gold += (int)Math.Floor(lot.CurrentBid * added.Overflow / (double)Math.Max(1, lot.Count));
                    result.Logs.Add("winLot");
                }
                else if (lot.PlayerBid > 0)
                {
                    player.Gold += lot.PlayerBid;
                    result.Logs.Add("refundBid");
                }
            }
            var remainingConsignments = new List<AuctionConsignmentState>();
            foreach (var item in state.Consignments)
            {
                if (!force && item.EndsAt > player.Age) { remainingConsignments.Add(item); continue; }
                var sale = ResolveConsignment(db, item, player, rng);
                if (sale.Sold) { player.Gold += sale.Payout; result.Logs.Add("consignmentSold"); }
                else { InventorySystem.AddItem(db, player, item.ItemId, item.Count); result.Logs.Add("consignmentReturned"); }
            }
            state.Lots.Clear(); state.Lots.AddRange(remainingLots);
            state.Consignments.Clear(); state.Consignments.AddRange(remainingConsignments);
            foreach (var log in result.Logs) state.History.Insert(0, log);
            result.Player = player;
            return result;
        }

        public static int GetAuctionTimeLeft(Player player)
        {
            var state = GetAuctionState(player);
            return state.LastRefreshAge < 0 ? 0 : Math.Max(0, state.LastRefreshAge + RefreshMonths - player.Age);
        }

        public static int GetNextBid(int current) => Math.Max(current + 1, (int)Math.Ceiling(current * 1.12));

        private static List<AuctionLotState> GenerateLots(GameDatabase db, Player player, int cycleIndex)
        {
            var region = EconomyStateHelpers.GetCurrentRegion(db, player);
            var tags = region?.RegionTags ?? new List<string>();
            var pool = db.AuctionLots.Values.Where(l => player.RealmIndex >= (l.MinRealm ?? 0) && (l.RegionTags == null || l.RegionTags.Count == 0 || l.RegionTags.Any(tags.Contains))).ToList();
            var result = new List<AuctionLotState>();
            var used = new HashSet<string>();
            for (int i = 0; i < AuctionSize && used.Count < pool.Count; i++)
            {
                var picked = WeightedPick(pool.Where(x => !used.Contains(x.Id)).ToList(), player.Age + cycleIndex * 17 + i * 31);
                if (picked == null) break;
                used.Add(picked.Id);
                result.Add(new AuctionLotState { Id = $"{picked.Id}:{player.Age}:{cycleIndex}:{i}", TemplateId = picked.Id, ItemId = picked.ItemId, Count = picked.Count ?? 1, BasePrice = picked.BasePrice ?? 1, CurrentBid = picked.BasePrice ?? 1, HighestBidder = "none", PlayerBid = 0, EndsAt = player.Age + RefreshMonths, AiBids = 0 });
            }
            return result;
        }

        private static bool RunAiCompetition(AuctionLotState lot, Player player, IRng rng)
        {
            bool raised = false;
            double pressure = 0.35 + player.RealmIndex * 0.04 - player.Charisma * 0.001;
            for (int i = 0; i < AiBidders.Length; i++)
            {
                double chance = Math.Max(0.12, Math.Min(0.78, pressure + lot.AiBids * 0.06 + i * 0.03));
                if (rng.NextDouble() < chance && lot.CurrentBid < lot.BasePrice * (1.8 + player.RealmIndex * 0.12))
                {
                    lot.CurrentBid = GetNextBid(lot.CurrentBid);
                    lot.HighestBidder = AiBidders[i];
                    lot.AiBids += 1;
                    raised = true;
                    if (rng.NextDouble() < 0.45) break;
                }
            }
            return raised;
        }

        private sealed class SaleResult { public bool Sold; public int Payout; }
        private static SaleResult ResolveConsignment(GameDatabase db, AuctionConsignmentState item, Player player, IRng rng)
        {
            db.Items.TryGetValue(item.ItemId, out var def);
            int fair = Math.Max(1, (def?.SellPrice ?? item.AskPrice) * item.Count * 3);
            double priceFactor = item.AskPrice / (double)fair;
            double chance = Math.Max(0.18, Math.Min(0.92, 0.75 - (priceFactor - 1) * 0.35 + player.Charisma * 0.002 + player.Luck * 0.001));
            bool sold = rng.NextDouble() < chance;
            int hammer = Math.Max(item.AskPrice, (int)Math.Floor(item.AskPrice * (1 + rng.NextDouble() * 0.25)));
            return new SaleResult { Sold = sold, Payout = (int)Math.Floor(hammer * (1 - SellFeeRate)) };
        }

        private static AuctionLotDef WeightedPick(List<AuctionLotDef> items, int seed)
        {
            int total = items.Sum(x => Math.Max(0, x.Weight ?? 0));
            if (total <= 0) return items.FirstOrDefault();
            double roll = Seeded(seed) * total;
            foreach (var item in items)
            {
                roll -= Math.Max(0, item.Weight ?? 0);
                if (roll <= 0) return item;
            }
            return items.FirstOrDefault();
        }

        private static double Seeded(int seed)
        {
            double x = Math.Sin(seed * 999 + 7) * 10000;
            return x - Math.Floor(x);
        }
    }
}
