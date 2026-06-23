// ============================================================
// MapSystem.cs — region access and travel
// UnityEngine-free
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class TravelResult { public Player Player; public bool Success; public RegionDef Region; public int Cost; public string Message; }
    public sealed class SceneExit { public RegionDef Region; public string Direction; public int TravelCost; public bool CanEnter; public string LockReason; }

    public static class MapSystem
    {
        public static MapSystemState GetMapState(GameDatabase db, Player player)
        {
            var state = WorldState.Get(player, "map", () => new MapSystemState());
            if (string.IsNullOrEmpty(state.CurrentRegionId)) state.CurrentRegionId = GetDefaultRegionId(db);
            if (state.UnlockedRegions.Count == 0) state.UnlockedRegions.Add(state.CurrentRegionId);
            return state;
        }
        public static string GetDefaultRegionId(GameDatabase db)
        {
            return db.Regions.Values.FirstOrDefault(r => (r.SafeZone ?? false) && (r.MinRealm ?? 0) == 0)?.Id
                ?? db.Regions.Values.FirstOrDefault(r => (r.MinRealm ?? 0) == 0)?.Id
                ?? db.Regions.Values.FirstOrDefault()?.Id;
        }
        public static RegionDef GetCurrentRegion(GameDatabase db, Player player)
        {
            var state = GetMapState(db, player);
            return state.CurrentRegionId != null && db.Regions.TryGetValue(state.CurrentRegionId, out var r) ? r : null;
        }
        public static Player RefreshUnlockedRegions(GameDatabase db, Player player)
        {
            var state = GetMapState(db, player);
            foreach (var r in db.Regions.Values.Where(r => WorldState.MaxCultivation(player) >= (r.MinRealm ?? 0))) if (!state.UnlockedRegions.Contains(r.Id)) state.UnlockedRegions.Add(r.Id);
            return player;
        }
        public static string GetRegionAccessLockReason(GameDatabase db, Player player, string regionId)
        {
            if (!db.Regions.TryGetValue(regionId, out var region)) return "regionNotFound";
            if (WorldState.MaxCultivation(player) < (region.MinRealm ?? 0)) return "realmInsufficient";
            return null;
        }
        public static int CalcTravelCost(Player player, RegionDef region)
        {
            int baseCost = region.TravelCostBase ?? 0; if (baseCost <= 0) return 0;
            double speedFactor = 100.0 / (100.0 + player.Speed * 0.5);
            double moveSpeedFactor = 100.0 / (100.0 + player.MoveSpeed);
            return Math.Max(1, (int)Math.Floor(baseCost * speedFactor * moveSpeedFactor));
        }
        public static TravelResult TravelTo(GameDatabase db, Player player, string regionId)
        {
            var result = new TravelResult { Player = player };
            if (!db.Regions.TryGetValue(regionId, out var region)) { result.Message = "regionNotFound"; return result; }
            if (region.IsContainer ?? false) { result.Message = "containerRegion"; return result; }
            var state = GetMapState(db, player);
            if (state.CurrentRegionId == regionId) { result.Success = true; result.Region = region; result.Message = "alreadyHere"; return result; }
            var lockReason = GetRegionAccessLockReason(db, player, regionId); if (lockReason != null) { result.Message = lockReason; return result; }
            int cost = CalcTravelCost(player, region); if (player.Stamina < cost) { result.Message = "staminaInsufficient"; return result; }
            player.Stamina -= cost; WorldState.AdvanceMonths(player, region.TravelTimeMonths ?? 0);
            state.CurrentRegionId = regionId; state.TravelCount++; if (!state.UnlockedRegions.Contains(regionId)) state.UnlockedRegions.Add(regionId);
            result.Success = true; result.Region = region; result.Cost = cost; result.Message = "arrived"; return result;
        }
        public static List<SceneExit> GetSceneExits(GameDatabase db, Player player)
        {
            var current = GetCurrentRegion(db, player); if (current == null) return new List<SceneExit>();
            var exits = new List<SceneExit>();
            void Add(RegionDef r, string dir) { var lockReason = GetRegionAccessLockReason(db, player, r.Id); exits.Add(new SceneExit { Region = r, Direction = dir, TravelCost = CalcTravelCost(player, r), CanEnter = lockReason == null, LockReason = lockReason }); }
            if (!string.IsNullOrEmpty(current.ParentId) && db.Regions.TryGetValue(current.ParentId, out var parent) && !(parent.IsContainer ?? false)) Add(parent, "up");
            foreach (var child in db.Regions.Values.Where(r => r.ParentId == current.Id && !(r.IsContainer ?? false)).OrderBy(r => r.MinRealm ?? 0)) Add(child, "down");
            if (!string.IsNullOrEmpty(current.ParentId)) foreach (var sib in db.Regions.Values.Where(r => r.ParentId == current.ParentId && r.Id != current.Id && !(r.IsContainer ?? false)).OrderBy(r => r.MinRealm ?? 0)) Add(sib, "sibling");
            return exits;
        }
    }
}
