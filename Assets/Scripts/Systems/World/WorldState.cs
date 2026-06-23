// ============================================================
// WorldState.cs — shared world/social runtime state and helpers
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class MapSystemState { public string CurrentRegionId; public readonly List<string> UnlockedRegions = new(); public int TravelCount; }
    public sealed class EventRuntimeState { public readonly HashSet<string> TriggeredOnce = new(); public readonly Dictionary<string, int> Cooldowns = new(); }
    public sealed class ObjectiveProgress { public int ObjectiveIndex; public int CurrentCount; public bool Completed; }
    public sealed class QuestProgress { public string QuestId, Status = "active"; public int CurrentStepIndex, AcceptedAt, StepStartedAt; public readonly List<ObjectiveProgress> ObjectiveProgress = new(); public readonly List<int> CompletedSteps = new(); }
    public sealed class QuestCompletion { public string QuestId; public int CompletedAt, RepeatCount; }
    public sealed class QuestFailure { public string QuestId, Reason; public int FailedAt; }
    public sealed class QuestSystemState { public readonly Dictionary<string, QuestProgress> ActiveQuests = new(); public readonly Dictionary<string, QuestCompletion> CompletedQuests = new(); public readonly Dictionary<string, QuestFailure> FailedQuests = new(); public readonly List<string> AbandonedQuests = new(); public readonly List<string> DiscoveredQuests = new(); public string TrackedQuestId; public readonly Dictionary<string, int> ActionCounters = new() { ["explore"] = 0, ["cultivate"] = 0, ["combat"] = 0 }; }
    public sealed class DialogueSystemState { public readonly List<string> TriggeredOnce = new(); public readonly Dictionary<string, int> LastTriggerAge = new(); public readonly Dictionary<string, object> Flags = new(); }
    public sealed class NpcRelation { public string NpcId, RelationLevel = "stranger"; public int Affinity, MetAt, InteractionCount, LastInteractionYear; public bool Met; public readonly Dictionary<string, object> Flags = new(); }
    public sealed class NpcSystemState { public readonly Dictionary<string, NpcRelation> Relations = new(); public readonly List<string> DiscoveredNpcs = new(); public readonly Dictionary<string, int> LastGiftAge = new(); }
    public sealed class SectSystemState { public string SectId, RankId; public int Contribution, TotalContribution, JoinedAt = -1, ClaimedStipendYear; public readonly Dictionary<string, int> MissionCooldowns = new(); public readonly Dictionary<string, int> StorePurchases = new(); }
    public sealed class SecretRealmRun { public string RealmId; public int StartedAt, StageIndex; public JObject Rewards = new(); public readonly List<string> Logs = new(); public bool Completed, Failed; }
    public sealed class SecretRealmSystemState { public readonly Dictionary<string, int> Cooldowns = new(); public SecretRealmRun ActiveRun; public readonly Dictionary<string, int> CompletedRuns = new(); }
    public class GeneratedBounty { public string Id, TemplateId, Title, Description, Issuer, Icon; public int CreatedAt, ExpiresAt, Reputation; public JToken Objective, Rewards; }
    public sealed class ActiveBounty : GeneratedBounty { public int AcceptedAt, Progress; public bool Completed; }
    public sealed class BountySystemState { public readonly List<GeneratedBounty> Available = new(); public readonly Dictionary<string, ActiveBounty> Active = new(); public readonly Dictionary<string, QuestCompletion> Completed = new(); public int LastRefreshAge = -1, Reputation; }

    public sealed class WorldActionResult { public Player Player; public readonly List<string> Logs = new(); public bool Success; public string Message; }

    internal static class WorldState
    {
        public static T Get<T>(Player p, string key, Func<T> create) where T : class
        {
            if (p.Systems.TryGetValue(key, out var obj) && obj is T typed) return typed;
            var state = create(); p.Systems[key] = state; return state;
        }
        public static string Alignment(Player p) => p.Karma >= 30 ? "righteous" : (p.Karma <= -30 ? "evil" : "neutral");
        public static int MaxCultivation(Player p) => Math.Max(p.RealmIndex, p.BodyRealmIndex);
        public static void AdvanceMonths(Player p, int months)
        {
            int gameMonth = p.GameMonth + months, gameYear = p.GameYear;
            if (gameMonth > 12) { gameYear += (int)Math.Floor((gameMonth - 1) / 12.0); gameMonth = ((gameMonth - 1) % 12) + 1; }
            p.Age += months; p.GameYear = gameYear; p.GameMonth = gameMonth;
        }
        public static string RelationLevel(int affinity) => affinity < -50 ? "hostile" : affinity < -10 ? "cold" : affinity < 10 ? "stranger" : affinity < 30 ? "acquaintance" : affinity < 60 ? "friend" : affinity < 90 ? "close_friend" : "soulmate";
    }

    internal static class WorldJson
    {
        public static JObject Obj(JToken t) => t as JObject;
        public static JArray Arr(JToken t) => t as JArray;
        public static string Str(JToken t, string name, string fallback = null) => (t as JObject)?.Value<string>(name) ?? fallback;
        public static int Int(JToken t, string name, int fallback = 0) => (t as JObject)?.Value<int?>(name) ?? fallback;
        public static double Dbl(JToken t, string name, double fallback = 0) => (t as JObject)?.Value<double?>(name) ?? fallback;
        public static bool Bool(JToken t, string name, bool fallback = false) => (t as JObject)?.Value<bool?>(name) ?? fallback;
        public static IEnumerable<JObject> Objects(JToken t) => (t as JArray)?.OfType<JObject>() ?? Enumerable.Empty<JObject>();
        public static bool HasItem(Player p, string itemId, int count) => !string.IsNullOrEmpty(itemId) && InventorySystem.CountItem(p, itemId) >= Math.Max(1, count);

        public static JObject MergeRewards(JObject baseReward, JToken extra)
        {
            var res = baseReward == null ? new JObject() : (JObject)baseReward.DeepClone();
            if (extra == null || extra.Type == JTokenType.Null) return res;
            res["exp"] = (res.Value<int?>("exp") ?? 0) + (extra.Value<int?>("exp") ?? 0);
            res["gold"] = (res.Value<int?>("gold") ?? 0) + (extra.Value<int?>("gold") ?? 0);
            var map = new Dictionary<string, int>();
            foreach (var it in Objects(res["items"])) map[it.Value<string>("itemId")] = (map.TryGetValue(it.Value<string>("itemId"), out var c) ? c : 0) + (it.Value<int?>("count") ?? 1);
            foreach (var it in Objects(extra["items"])) map[it.Value<string>("itemId")] = (map.TryGetValue(it.Value<string>("itemId"), out var c) ? c : 0) + (it.Value<int?>("count") ?? 1);
            if (map.Count > 0) res["items"] = new JArray(map.Select(kv => new JObject { ["itemId"] = kv.Key, ["count"] = kv.Value }));
            return res;
        }

        public static List<string> ApplyReward(GameDatabase db, Player p, JToken reward, bool includeContribution = false)
        {
            var logs = new List<string>(); if (reward == null || reward.Type == JTokenType.Null) return logs;
            int exp = reward.Value<int?>("exp") ?? 0; if (exp != 0) { p.Exp += exp; logs.Add($"exp+{exp}"); }
            int gold = reward.Value<int?>("gold") ?? 0; if (gold != 0) { p.Gold += gold; logs.Add($"gold+{gold}"); }
            foreach (var item in Objects(reward["items"])) { string id = item.Value<string>("itemId"); int count = item.Value<int?>("count") ?? 1; InventorySystem.AddItem(db, p, id, count); logs.Add($"item:{id}x{count}"); }
            if (reward["statBonus"] is JObject stats) foreach (var kv in stats) { AddNumeric(p, kv.Key, kv.Value.Value<int>()); logs.Add($"{kv.Key}+{kv.Value.Value<int>()}"); }
            int karma = reward.Value<int?>("karmaChange") ?? 0; if (karma != 0) { p.Karma = Math.Max(-100, Math.Min(100, p.Karma + karma)); logs.Add($"karma{karma:+#;-#;0}"); }
            if (includeContribution) { int c = reward.Value<int?>("contribution") ?? 0; if (c != 0) { var s = SectSystem.GetSectState(p); s.Contribution += c; s.TotalContribution += Math.Max(0, c); logs.Add($"contribution+{c}"); } }
            return logs;
        }

        public static void AddNumeric(Player p, string key, int delta)
        {
            switch (key) { case "hp": p.Hp += delta; break; case "mp": p.Mp += delta; break; case "atk": p.Atk += delta; break; case "def": p.Def += delta; break; case "speed": p.Speed += delta; break; case "luck": p.Luck += delta; break; case "comprehension": p.Comprehension += delta; break; case "charisma": p.Charisma += delta; break; case "health": p.Health += delta; break; case "mood": p.Mood += delta; break; }
        }
    }

    internal static class WorldConditions
    {
        public static bool CheckBasic(GameDatabase db, Player p, JToken cond, string npcId = null)
        {
            if (cond == null || cond.Type == JTokenType.Null) return true;
            if (WorldJson.Int(cond, "minRealm", int.MinValue) != int.MinValue && p.RealmIndex < WorldJson.Int(cond, "minRealm")) return false;
            if (WorldJson.Int(cond, "maxRealm", int.MaxValue) != int.MaxValue && p.RealmIndex > WorldJson.Int(cond, "maxRealm")) return false;
            if (WorldJson.Int(cond, "minAge", int.MinValue) != int.MinValue && p.Age < WorldJson.Int(cond, "minAge")) return false;
            if (WorldJson.Int(cond, "minLuck", int.MinValue) != int.MinValue && p.Luck < WorldJson.Int(cond, "minLuck")) return false;
            if (WorldJson.Int(cond, "maxLuck", int.MaxValue) != int.MaxValue && p.Luck > WorldJson.Int(cond, "maxLuck")) return false;
            if (WorldJson.Int(cond, "minComprehension", int.MinValue) != int.MinValue && p.Comprehension < WorldJson.Int(cond, "minComprehension")) return false;
            if (WorldJson.Int(cond, "minCharisma", int.MinValue) != int.MinValue && p.Charisma < WorldJson.Int(cond, "minCharisma")) return false;
            if (WorldJson.Int(cond, "minGold", int.MinValue) != int.MinValue && p.Gold < WorldJson.Int(cond, "minGold")) return false;
            if (WorldJson.Int(cond, "minMood", int.MinValue) != int.MinValue && p.Mood < WorldJson.Int(cond, "minMood")) return false;
            if (WorldJson.Int(cond, "maxMood", int.MaxValue) != int.MaxValue && p.Mood > WorldJson.Int(cond, "maxMood")) return false;
            if (WorldJson.Int(cond, "minHealth", int.MinValue) != int.MinValue && p.Health < WorldJson.Int(cond, "minHealth")) return false;
            if (WorldJson.Int(cond, "maxHealth", int.MaxValue) != int.MaxValue && p.Health > WorldJson.Int(cond, "maxHealth")) return false;
            var align = WorldJson.Str(cond, "requiredAlignment"); if (!string.IsNullOrEmpty(align) && WorldState.Alignment(p) != align) return false;
            if (WorldJson.Int(cond, "minKarma", int.MinValue) != int.MinValue && p.Karma < WorldJson.Int(cond, "minKarma")) return false;
            if (WorldJson.Int(cond, "maxKarma", int.MaxValue) != int.MaxValue && p.Karma > WorldJson.Int(cond, "maxKarma")) return false;
            var regionId = WorldJson.Str(cond, "regionId"); if (db != null && !string.IsNullOrEmpty(regionId) && MapSystem.GetCurrentRegion(db, p)?.Id != regionId) return false;
            var tags = cond["regionTags"]?.Values<string>().ToList(); if (db != null && tags?.Count > 0) { var cur = MapSystem.GetCurrentRegion(db, p); if (cur?.RegionTags == null || !tags.Any(t => cur.RegionTags.Contains(t))) return false; }
            foreach (var item in WorldJson.Objects(cond["requiredItems"])) if (!WorldJson.HasItem(p, item.Value<string>("itemId"), item.Value<int?>("count") ?? 1)) return false;
            var rels = WorldJson.Objects(cond["npcAffinity"]).ToList(); foreach (var r in rels) if (NpcSystem.GetRelation(p, r.Value<string>("npcId")).Affinity < (r.Value<int?>("min") ?? 0)) return false;
            if (npcId != null) { var rel = NpcSystem.GetRelation(p, npcId); if (WorldJson.Int(cond, "minAffinity", int.MinValue) != int.MinValue && rel.Affinity < WorldJson.Int(cond, "minAffinity")) return false; if (WorldJson.Int(cond, "maxAffinity", int.MaxValue) != int.MaxValue && rel.Affinity > WorldJson.Int(cond, "maxAffinity")) return false; var levels = cond["relationLevel"]?.Values<string>().ToList(); if (levels?.Count > 0 && !levels.Contains(rel.RelationLevel)) return false; }
            return true;
        }
    }
}
