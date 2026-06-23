// ============================================================
// SectSystem.cs — sect join, contribution and missions
// UnityEngine-free
// ============================================================
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class SectResult { public Player Player; public bool Success; public readonly System.Collections.Generic.List<string> Logs = new(); }
    public static class SectSystem
    {
        public static SectSystemState GetSectState(Player p) => WorldState.Get(p, "sect", () => new SectSystemState());
        public static string GetJoinLockReason(GameDatabase db, Player p, string sectId)
        {
            if (!db.Sects.TryGetValue(sectId, out var def)) return "sectNotFound"; var state = GetSectState(p); if (!string.IsNullOrEmpty(state.SectId)) return "alreadyJoined"; if (p.RealmIndex < (def.MinRealm ?? 0)) return "realm"; if (def.MinKarma.HasValue && p.Karma < def.MinKarma.Value) return "karma"; if ((def.EntryGold ?? 0) > p.Gold) return "gold"; return null;
        }
        public static SectResult JoinSect(GameDatabase db, Player p, string sectId)
        {
            var res = new SectResult { Player = p }; var lockReason = GetJoinLockReason(db, p, sectId); if (lockReason != null) { res.Logs.Add(lockReason); return res; }
            var def = db.Sects[sectId]; var firstRank = def.Ranks?.OfType<JObject>().FirstOrDefault(); p.Gold -= def.EntryGold ?? 0; var s = GetSectState(p); s.SectId = sectId; s.RankId = firstRank?.Value<string>("id"); s.JoinedAt = p.Age; s.Contribution = s.TotalContribution = 0; res.Success = true; res.Logs.Add($"joined:{sectId}"); return res;
        }
        public static SectResult ClaimSectStipend(GameDatabase db, Player p)
        {
            var res = new SectResult { Player = p }; var ctx = GetMembership(db, p); if (ctx.Def == null) { res.Logs.Add("notJoined"); return res; } var s = ctx.State; if (s.ClaimedStipendYear == p.GameYear) return res;
            p.Gold += ctx.Rank.Value<int?>("stipendGold") ?? 0; AddContribution(p, ctx.Rank.Value<int?>("stipendContribution") ?? 0); s.ClaimedStipendYear = p.GameYear; res.Success = true; return res;
        }
        public static SectResult CompleteSectMission(GameDatabase db, Player p, string missionId)
        {
            var res = new SectResult { Player = p }; var ctx = GetMembership(db, p); if (ctx.Def == null) return res; var m = ctx.Def.Missions?.OfType<JObject>().FirstOrDefault(x => x.Value<string>("id") == missionId); if (m == null) return res;
            int available = ctx.State.MissionCooldowns.TryGetValue(missionId, out var a) ? a : 0; if (available > p.Age) return res; if (p.Stamina < (m.Value<int?>("staminaCost") ?? 0) || p.Gold < (m.Value<int?>("goldCost") ?? 0)) return res;
            if (m["itemCost"] is JObject cost && !InventorySystem.RemoveItem(p, cost.Value<string>("itemId"), cost.Value<int?>("count") ?? 1)) return res;
            p.Stamina -= m.Value<int?>("staminaCost") ?? 0; p.Gold -= m.Value<int?>("goldCost") ?? 0; res.Logs.AddRange(WorldJson.ApplyReward(db, p, m["reward"], true)); ctx.State.MissionCooldowns[missionId] = p.Age + (m.Value<int?>("repeatCooldownMonths") ?? 0); res.Success = true; return res;
        }
        public static Player AddContribution(Player p, int amount) { var s = GetSectState(p); s.Contribution += amount; s.TotalContribution += System.Math.Max(0, amount); return p; }
        private static (SectSystemState State, SectDef Def, JObject Rank) GetMembership(GameDatabase db, Player p) { var s = GetSectState(p); if (string.IsNullOrEmpty(s.SectId) || !db.Sects.TryGetValue(s.SectId, out var def)) return (s, null, null); var rank = def.Ranks?.OfType<JObject>().FirstOrDefault(r => r.Value<string>("id") == s.RankId); return (s, def, rank); }
    }
}
