// ============================================================
// NpcSystem.cs — NPC relation and region queries
// UnityEngine-free
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class NpcActionResult { public Player Player; public bool Success; public string Message; public int AffinityChange; public string NewLevel; }
    public static class NpcSystem
    {
        public const int GiftCooldownMonths = 3;
        public static NpcSystemState GetNpcState(Player p) => WorldState.Get(p, "npc", () => new NpcSystemState());
        public static NpcRelation GetRelation(Player p, string npcId)
        {
            var state = GetNpcState(p); if (state.Relations.TryGetValue(npcId, out var rel)) return rel;
            return new NpcRelation { NpcId = npcId, Affinity = 0, Met = false, MetAt = 0, InteractionCount = 0, LastInteractionYear = 0, RelationLevel = "stranger" };
        }
        public static NpcActionResult MeetNpc(GameDatabase db, Player p, string npcId)
        {
            if (!db.Npcs.ContainsKey(npcId)) return new NpcActionResult { Player = p, Message = "npcNotFound" };
            var state = GetNpcState(p); if (state.Relations.TryGetValue(npcId, out var existing) && existing.Met) return new NpcActionResult { Player = p, Success = true, Message = "alreadyMet" };
            var rel = new NpcRelation { NpcId = npcId, Affinity = 5, Met = true, MetAt = p.GameYear, InteractionCount = 1, LastInteractionYear = p.GameYear, RelationLevel = WorldState.RelationLevel(5) };
            state.Relations[npcId] = rel; if (!state.DiscoveredNpcs.Contains(npcId)) state.DiscoveredNpcs.Add(npcId);
            return new NpcActionResult { Player = p, Success = true, Message = "met", AffinityChange = 5, NewLevel = rel.RelationLevel };
        }
        public static NpcActionResult ChangeAffinity(GameDatabase db, Player p, string npcId, int delta, string reason = null)
        {
            if (!db.Npcs.TryGetValue(npcId, out var npc)) return new NpcActionResult { Player = p, Message = "npcNotFound" };
            var state = GetNpcState(p); var rel = state.Relations.TryGetValue(npcId, out var current) ? current : GetRelation(p, npcId);
            int old = rel.Affinity, max = npc.MaxAffinity ?? 100; rel.Affinity = Math.Max(-100, Math.Min(max, rel.Affinity + delta)); rel.Met = true; rel.InteractionCount++; rel.LastInteractionYear = p.GameYear; rel.RelationLevel = WorldState.RelationLevel(rel.Affinity);
            state.Relations[npcId] = rel; if (!state.DiscoveredNpcs.Contains(npcId)) state.DiscoveredNpcs.Add(npcId);
            return new NpcActionResult { Player = p, Success = true, Message = "affinityChanged", AffinityChange = rel.Affinity - old, NewLevel = rel.RelationLevel };
        }
        public static NpcActionResult GiveGift(GameDatabase db, Player p, string npcId, string itemId, IRng rng)
        {
            if (!db.Npcs.TryGetValue(npcId, out var npc)) return new NpcActionResult { Player = p, Message = "npcNotFound" };
            var state = GetNpcState(p); var rel = state.Relations.TryGetValue(npcId, out var r) ? r : null; if (rel == null || !rel.Met) return new NpcActionResult { Player = p, Message = "notMet" };
            int last = state.LastGiftAge.TryGetValue(npcId, out var l) ? l : int.MinValue / 2; if (last + GiftCooldownMonths > p.Age) return new NpcActionResult { Player = p, Message = "giftCooldown" };
            if (!InventorySystem.RemoveItem(p, itemId, 1)) return new NpcActionResult { Player = p, Message = "itemMissing" };
            int baseDelta = 2 + rng.NextIntInclusive(0, 3);
            if (npc.GiftPreferences is JObject prefs) { var loved = prefs["loved"]?.Values<string>().Contains(itemId) == true; var liked = prefs["liked"]?.Values<string>().Contains(itemId) == true; var disliked = prefs["disliked"]?.Values<string>().Contains(itemId) == true; if (loved) baseDelta = 15 + rng.NextIntInclusive(0, 10); else if (liked) baseDelta = 5 + rng.NextIntInclusive(0, 5); else if (disliked) baseDelta = -(5 + rng.NextIntInclusive(0, 5)); }
            int delta = baseDelta > 0 ? (int)Math.Round(baseDelta * (1 + p.Charisma / 200.0)) : baseDelta; state.LastGiftAge[npcId] = p.Age;
            var change = ChangeAffinity(db, p, npcId, delta, "gift"); change.Message = "gift"; return change;
        }
        public static List<NpcDef> GetNpcsInRegion(GameDatabase db, Player p)
        {
            var region = MapSystem.GetCurrentRegion(db, p); if (region == null) return new List<NpcDef>(); var tags = region.RegionTags ?? new List<string>();
            return db.Npcs.Values.Where(n => p.RealmIndex >= (n.MinRealm ?? 0) && (n.HomeRegionId == region.Id || (n.RegionTags != null && n.RegionTags.Any(tags.Contains)))).ToList();
        }
    }
}
