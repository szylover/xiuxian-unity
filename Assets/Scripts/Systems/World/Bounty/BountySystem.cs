// ============================================================
// BountySystem.cs — bounty board, accept, progress and claim
// UnityEngine-free
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class BountyResult { public Player Player; public bool Success; public readonly List<string> Logs = new(); public GeneratedBounty Bounty; }
    public static class BountySystem
    {
        public const int BoardSize = 3, MaxActive = 2, RefreshMonths = 3;
        public static BountySystemState GetBountyState(Player p) => WorldState.Get(p, "bounty", () => new BountySystemState());
        public static Player EnsureBountyBoard(GameDatabase db, Player p, IRng rng, bool force = false)
        {
            var s = GetBountyState(p); ExpireBounties(p); bool refresh = force || s.LastRefreshAge < 0 || p.Age - s.LastRefreshAge >= RefreshMonths; if (!refresh) return p;
            s.Available.Clear(); var regionTags = MapSystem.GetCurrentRegion(db, p)?.RegionTags ?? new List<string>(); var pool = db.Bounties.Values.Where(t => p.RealmIndex >= (t.MinRealm ?? 0) && (!t.MaxRealm.HasValue || p.RealmIndex <= t.MaxRealm.Value) && (t.RegionTags == null || t.RegionTags.Count == 0 || t.RegionTags.Any(regionTags.Contains))).ToList(); var used = new HashSet<string>();
            for (int i = 0; i < BoardSize && used.Count < pool.Count; i++) { var template = WeightedPick(pool.Where(t => !used.Contains(t.Id)).ToList(), rng); if (template == null) break; used.Add(template.Id); s.Available.Add(FromTemplate(template, p.Age, i)); }
            s.LastRefreshAge = p.Age; return p;
        }
        public static BountyResult RefreshBountyBoard(GameDatabase db, Player p, IRng rng) { EnsureBountyBoard(db, p, rng, true); return new BountyResult { Player = p, Success = true }; }
        public static BountyResult AcceptBounty(GameDatabase db, Player p, string bountyId, IRng rng)
        {
            EnsureBountyBoard(db, p, rng); var res = new BountyResult { Player = p }; var s = GetBountyState(p); if (s.Active.Count >= MaxActive) return res; var b = s.Available.FirstOrDefault(x => x.Id == bountyId); if (b == null) return res;
            var active = new ActiveBounty { Id = b.Id, TemplateId = b.TemplateId, Title = b.Title, Description = b.Description, Issuer = b.Issuer, Icon = b.Icon, CreatedAt = b.CreatedAt, ExpiresAt = b.ExpiresAt, Objective = b.Objective, Rewards = b.Rewards, Reputation = b.Reputation, AcceptedAt = p.Age, Progress = GetCurrentProgress(p, b.Objective), Completed = false };
            active.Completed = active.Progress >= (active.Objective.Value<int?>("count") ?? 1); s.Available.Remove(b); s.Active[b.Id] = active; res.Success = true; res.Bounty = active; res.Logs.Add($"accepted:{b.Id}"); return res;
        }
        public static BountyResult TickBountyObjectives(GameDatabase db, Player p, QuestTrigger trigger)
        {
            ExpireBounties(p); var res = new BountyResult { Player = p }; var s = GetBountyState(p);
            foreach (var kv in s.Active.ToList()) { var b = kv.Value; if (b.Completed) continue; int delta = GetTriggerDelta(p, b.Objective, trigger); int count = b.Objective.Value<int?>("count") ?? 1; int current = Math.Min(count, Math.Max(b.Progress, GetCurrentProgress(p, b.Objective)) + delta); if (current != b.Progress) { b.Progress = current; res.Success = true; } b.Completed = b.Progress >= count; }
            return res;
        }
        public static BountyResult ClaimBounty(GameDatabase db, Player p, string bountyId)
        {
            TickBountyObjectives(db, p, new QuestTrigger { Type = "item_change" }); var res = new BountyResult { Player = p }; var s = GetBountyState(p); if (!s.Active.TryGetValue(bountyId, out var b) || !b.Completed) return res;
            res.Logs.AddRange(WorldJson.ApplyReward(db, p, b.Rewards)); s.Active.Remove(bountyId); s.Completed[bountyId] = new QuestCompletion { QuestId = bountyId, CompletedAt = p.Age, RepeatCount = 1 }; s.Reputation += b.Reputation; res.Success = true; res.Bounty = b; res.Logs.Add($"reputation+{b.Reputation}"); return res;
        }
        private static void ExpireBounties(Player p) { var s = GetBountyState(p); s.Available.RemoveAll(b => b.ExpiresAt <= p.Age); foreach (var id in s.Active.Where(kv => kv.Value.ExpiresAt <= p.Age).Select(kv => kv.Key).ToList()) s.Active.Remove(id); }
        private static GeneratedBounty FromTemplate(BountyTemplateDef t, int age, int i) => new GeneratedBounty { Id = $"{t.Id}:{age}:{i}", TemplateId = t.Id, Title = t.Title, Description = t.Description, Issuer = t.Issuer, Icon = t.Icon, CreatedAt = age, ExpiresAt = age + (t.DurationMonths ?? 0), Objective = t.Objective, Rewards = t.Rewards, Reputation = t.Reputation?.Value<int?>() ?? t.Reputation?.Value<int>() ?? 0 };
        private static BountyTemplateDef WeightedPick(List<BountyTemplateDef> items, IRng rng) { int total = items.Sum(i => Math.Max(0, i.Weight ?? 1)); if (total <= 0) return items.FirstOrDefault(); double roll = rng.NextDouble() * total; foreach (var i in items) { roll -= Math.Max(0, i.Weight ?? 1); if (roll <= 0) return i; } return items.FirstOrDefault(); }
        private static int GetTriggerDelta(Player p, JToken obj, QuestTrigger trigger) { string type = obj.Value<string>("type"), target = obj.Value<string>("targetId"); int count = obj.Value<int?>("count") ?? 1; if (type == "kill_monster" && trigger.Type == "kill_monster" && trigger.MonsterId == target) return 1; if (type == "reach_region" && trigger.Type == "reach_region" && trigger.RegionId == target) return count; if (type == "collect_item" && (trigger.Type == "item_change" || trigger.Type == "explore" || trigger.Type == "combat" || trigger.Type == "craft_item") && WorldJson.HasItem(p, target, count)) return count; return 0; }
        private static int GetCurrentProgress(Player p, JToken obj) { string type = obj.Value<string>("type"), target = obj.Value<string>("targetId"); int count = obj.Value<int?>("count") ?? 1; if (type == "collect_item" && WorldJson.HasItem(p, target, count)) return count; return 0; }
    }
}
