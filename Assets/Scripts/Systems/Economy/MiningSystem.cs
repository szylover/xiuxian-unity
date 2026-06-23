// ============================================================
// MiningSystem.cs — feng-shui mining sites and deterministic yields
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class MiningYieldRuntimeDef
    {
        public string ItemId;
        public int Min, Max, Weight;
        public bool Rare;
    }

    public sealed class MiningSystemState
    {
        public int MinedCount;
        public string LastSiteId;
        public int LastMinedAt;
        public int TotalFengShui;
    }

    public sealed class MiningActionResult
    {
        public Player Player;
        public readonly Dictionary<string, int> Yields = new();
        public readonly List<string> Logs = new();
    }

    public static class MiningSystem
    {
        public static MiningSystemState GetMiningState(Player player)
        {
            if (player.Systems.TryGetValue("mining", out var existing) && existing is MiningSystemState typed) return typed;
            var state = new MiningSystemState();
            player.Systems["mining"] = state;
            return state;
        }

        public static IEnumerable<MiningSiteDef> GetAvailableMiningSites(GameDatabase db, Player player)
        {
            var region = EconomyStateHelpers.GetCurrentRegion(db, player);
            var tags = region?.RegionTags ?? new List<string>();
            return db.MiningSites.Values.Where(site => player.RealmIndex >= (site.MinRealm ?? 0)
                && (string.IsNullOrEmpty(site.RegionId) || site.RegionId == region?.Id)
                && (site.RegionTags == null || site.RegionTags.Count == 0 || site.RegionTags.Any(tags.Contains)));
        }

        public static string GetMiningLockReason(GameDatabase db, Player player, string siteId)
        {
            if (!db.MiningSites.TryGetValue(siteId, out var site)) return EconomyTexts.SiteMissing;
            if (player.RealmIndex < (site.MinRealm ?? 0)) return EconomyTexts.RealmInsufficient;
            if (player.Stamina < (site.StaminaCost ?? 0)) return EconomyTexts.StaminaInsufficient;
            var region = EconomyStateHelpers.GetCurrentRegion(db, player);
            if (!string.IsNullOrEmpty(site.RegionId) && site.RegionId != region?.Id) return EconomyTexts.RealmInsufficient;
            return null;
        }

        public static MiningActionResult PerformMining(GameDatabase db, Player player, string siteId, IRng rng)
        {
            var result = new MiningActionResult { Player = player };
            if (!db.MiningSites.TryGetValue(siteId, out var site)) { result.Logs.Add(EconomyTexts.SiteMissing); return result; }
            string lockReason = GetMiningLockReason(db, player, siteId);
            if (lockReason != null) { result.Logs.Add(lockReason); return result; }
            player.Stamina -= site.StaminaCost ?? 0;
            AdvanceMonths(player, site.Months ?? 0);
            foreach (var kv in RollYields(player, site, rng))
            {
                var added = InventorySystem.AddItem(db, player, kv.Key, kv.Value);
                if (added.Added > 0) result.Yields[kv.Key] = added.Added;
            }
            var state = GetMiningState(player);
            state.MinedCount += 1;
            state.LastSiteId = site.Id;
            state.LastMinedAt = player.Age;
            state.TotalFengShui += ReadInt(site.FengShui, 0);
            result.Player = player;
            result.Logs.Add(EconomyTexts.Success);
            return result;
        }

        public static string GetFengShuiGrade(int value)
        {
            if (value >= 5) return "excellent";
            if (value >= 4) return "good";
            if (value >= 3) return "normal";
            if (value >= 2) return "poor";
            return "bad";
        }

        private static Dictionary<string, int> RollYields(Player player, MiningSiteDef site, IRng rng)
        {
            int miningApt = player.Aptitudes.Mining;
            int fengApt = player.Aptitudes.Fengshui;
            int fengShui = ReadInt(site.FengShui, 0);
            int attempts = Math.Max(1, ReadInt(site.BaseYields, 1) + (int)Math.Floor(fengShui / 2.0) + (int)Math.Floor(miningApt / 45.0));
            double rareBoost = fengShui * 0.06 + player.Luck * 0.002 + fengApt * 0.003;
            var yields = ReadYields(site.Yields);
            var result = new Dictionary<string, int>();
            for (int i = 0; i < attempts; i++)
            {
                var pool = yields.Select(y => new MiningYieldRuntimeDef { ItemId = y.ItemId, Min = y.Min, Max = y.Max, Weight = (int)Math.Floor(y.Weight * (y.Rare ? (1 + rareBoost) : 1)), Rare = y.Rare }).ToList();
                var picked = WeightedPick(pool, rng);
                if (picked == null) continue;
                int bonus = rng.NextDouble() < (fengShui * 0.08 + miningApt * 0.002) ? 1 : 0;
                int count = rng.NextIntInclusive(picked.Min, picked.Max) + bonus;
                result[picked.ItemId] = (result.TryGetValue(picked.ItemId, out var existing) ? existing : 0) + count;
            }
            return result;
        }

        private static void AdvanceMonths(Player player, int months)
        {
            int gameMonth = player.GameMonth + months;
            int gameYear = player.GameYear;
            if (gameMonth > 12)
            {
                gameYear += (int)Math.Floor((gameMonth - 1) / 12.0);
                gameMonth = ((gameMonth - 1) % 12) + 1;
            }
            player.Age += months;
            player.GameYear = gameYear;
            player.GameMonth = gameMonth;
        }

        private static MiningYieldRuntimeDef WeightedPick(List<MiningYieldRuntimeDef> items, IRng rng)
        {
            int total = items.Sum(x => Math.Max(0, x.Weight));
            if (total <= 0) return items.FirstOrDefault();
            double roll = rng.NextDouble() * total;
            foreach (var item in items)
            {
                roll -= Math.Max(0, item.Weight);
                if (roll <= 0) return item;
            }
            return items.FirstOrDefault();
        }

        private static int ReadInt(JToken token, int fallback)
        {
            if (token == null) return fallback;
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float) return token.Value<int>();
            return fallback;
        }

        private static List<MiningYieldRuntimeDef> ReadYields(JToken token)
        {
            var result = new List<MiningYieldRuntimeDef>();
            if (token is not JArray arr) return result;
            foreach (var entry in arr.OfType<JObject>())
            {
                result.Add(new MiningYieldRuntimeDef
                {
                    ItemId = entry.Value<string>("itemId"),
                    Min = entry.Value<int?>("min") ?? 1,
                    Max = entry.Value<int?>("max") ?? 1,
                    Weight = entry.Value<int?>("weight") ?? 1,
                    Rare = entry.Value<bool?>("rare") ?? false,
                });
            }
            return result;
        }
    }
}
