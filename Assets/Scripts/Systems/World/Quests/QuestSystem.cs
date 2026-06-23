// ============================================================
// QuestSystem.cs — discovery, objectives, completion, rewards
// UnityEngine-free
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class QuestTrigger { public string Type, MonsterId, RegionId, NpcId, RecipeId, OutputItemId, QuestId; public int RealmIndex; }
    public sealed class QuestResult { public Player Player; public readonly List<string> Logs = new(); public bool Success; public string QuestId; }

    public static class QuestSystem
    {
        public static QuestSystemState GetQuestState(Player p) => WorldState.Get(p, "quest", () => new QuestSystemState());
        public static bool CheckQuestCondition(GameDatabase db, Player p, QuestChainDef def)
        {
            if (!WorldConditions.CheckBasic(db, p, def.Condition)) return false;
            var state = GetQuestState(p); foreach (var id in def.Condition?["requiredQuests"]?.Values<string>() ?? Enumerable.Empty<string>()) if (!state.CompletedQuests.ContainsKey(id)) return false;
            return true;
        }
        public static QuestResult DiscoverQuest(GameDatabase db, Player p, string questId)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; if (!db.QuestChains.TryGetValue(questId, out _)) return res;
            var s = GetQuestState(p); if (s.DiscoveredQuests.Contains(questId) || s.ActiveQuests.ContainsKey(questId)) return res;
            s.DiscoveredQuests.Add(questId); res.Success = true; res.Logs.Add($"discovered:{questId}"); return res;
        }
        public static QuestResult CheckQuestDiscovery(GameDatabase db, Player p, QuestTrigger trigger, IRng rng = null)
        {
            var res = new QuestResult { Player = p };
            foreach (var def in db.QuestChains.Values)
            {
                var s = GetQuestState(p); if (s.DiscoveredQuests.Contains(def.Id) || s.ActiveQuests.ContainsKey(def.Id)) continue;
                if (s.CompletedQuests.ContainsKey(def.Id) && !(def.Repeatable ?? false)) continue;
                if (!CheckQuestCondition(db, p, def)) continue;
                var ds = def.DiscoverSource; if (ds == null) continue; bool discovered = false; string type = ds.Value<string>("type");
                switch (type)
                {
                    case "npc": discovered = trigger.Type == "talk_npc" && trigger.NpcId == ds.Value<string>("npcId"); break;
                    case "exploration": discovered = trigger.Type == "explore" && ((rng?.NextDouble() ?? 0) < (ds.Value<double?>("chance") ?? 1)); break;
                    case "combat_drop": discovered = trigger.Type == "kill_monster" && trigger.MonsterId == ds.Value<string>("monsterId") && ((rng?.NextDouble() ?? 0) < (ds.Value<double?>("chance") ?? 1)); break;
                    case "region_enter": discovered = trigger.Type == "reach_region" && trigger.RegionId == ds.Value<string>("regionId"); break;
                    case "realm_reach": discovered = trigger.Type == "reach_realm" && trigger.RealmIndex >= (ds.Value<int?>("realmIndex") ?? 0); break;
                    case "quest_complete": discovered = trigger.Type == "quest_complete" && trigger.QuestId == ds.Value<string>("questId"); break;
                    case "auto": discovered = true; break;
                }
                if (discovered) { var d = DiscoverQuest(db, p, def.Id); res.Logs.AddRange(d.Logs); res.Success = true; }
            }
            return res;
        }
        public static QuestResult AcceptQuest(GameDatabase db, Player p, string questId)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; if (!db.QuestChains.TryGetValue(questId, out var def)) return res;
            var s = GetQuestState(p); if (s.ActiveQuests.ContainsKey(questId)) return res; if (s.CompletedQuests.ContainsKey(questId) && !(def.Repeatable ?? false)) return res;
            var progress = new QuestProgress { QuestId = questId, Status = "active", CurrentStepIndex = 0, AcceptedAt = p.Age, StepStartedAt = p.Age };
            InitObjectives(def, 0, progress); s.ActiveQuests[questId] = progress; s.DiscoveredQuests.Remove(questId);
            res.Success = true; res.Logs.Add($"accepted:{questId}"); return res;
        }
        public static QuestResult TickQuestObjectives(GameDatabase db, Player p, QuestTrigger trigger)
        {
            var res = new QuestResult { Player = p }; var s = GetQuestState(p);
            if (trigger.Type == "explore" || trigger.Type == "cultivate" || trigger.Type == "combat") s.ActionCounters[trigger.Type] = (s.ActionCounters.TryGetValue(trigger.Type, out var c) ? c : 0) + 1;
            foreach (var questId in s.ActiveQuests.Keys.ToList())
            {
                var progress = s.ActiveQuests[questId]; if (progress.Status != "active") continue; if (!db.QuestChains.TryGetValue(questId, out var def)) continue;
                var step = def.Steps[progress.CurrentStepIndex]; bool changed = false;
                var objectives = WorldJson.Objects(step["objectives"]).ToList();
                for (int i = 0; i < objectives.Count; i++)
                {
                    var op = progress.ObjectiveProgress[i]; if (op.Completed) continue; int delta = GetObjectiveDelta(p, objectives[i], trigger, progress);
                    if (delta > 0) { int need = objectives[i].Value<int?>("count") ?? 1; op.CurrentCount = Math.Min(need, op.CurrentCount + delta); op.Completed = op.CurrentCount >= need; changed = true; }
                }
                if (changed) { res.Success = true; var stepRes = CheckStepCompletion(db, p, questId); res.Logs.AddRange(stepRes.Logs); }
            }
            return res;
        }
        public static QuestResult DeliverQuestItem(GameDatabase db, Player p, string questId, int objectiveIndex)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; var s = GetQuestState(p); if (!s.ActiveQuests.TryGetValue(questId, out var progress)) return res; if (!db.QuestChains.TryGetValue(questId, out var def)) return res;
            var obj = WorldJson.Objects(def.Steps[progress.CurrentStepIndex]["objectives"]).ElementAtOrDefault(objectiveIndex); if (obj == null || obj.Value<string>("type") != "deliver_item") return res;
            string itemId = obj.Value<string>("targetId"); int count = obj.Value<int?>("count") ?? 1; if (!InventorySystem.RemoveItem(p, itemId, count)) return res;
            progress.ObjectiveProgress[objectiveIndex].CurrentCount = count; progress.ObjectiveProgress[objectiveIndex].Completed = true; res.Success = true; res.Logs.Add($"delivered:{itemId}"); res.Logs.AddRange(CheckStepCompletion(db, p, questId).Logs); return res;
        }
        public static QuestResult TurnInQuest(GameDatabase db, Player p, string questId)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; var s = GetQuestState(p); if (!s.ActiveQuests.TryGetValue(questId, out var progress) || progress.Status != "pending_turnin") return res;
            res.Logs.AddRange(CompleteQuest(db, p, questId).Logs); res.Success = true; return res;
        }
        public static QuestResult CheckStepCompletion(GameDatabase db, Player p, string questId)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; var s = GetQuestState(p); if (!s.ActiveQuests.TryGetValue(questId, out var progress) || !db.QuestChains.TryGetValue(questId, out var def)) return res;
            if (!progress.ObjectiveProgress.All(o => o.Completed)) return res;
            var step = def.Steps[progress.CurrentStepIndex]; res.Logs.AddRange(WorldJson.ApplyReward(db, p, step["rewards"])); progress.CompletedSteps.Add(progress.CurrentStepIndex);
            int next = progress.CurrentStepIndex + 1; if (next >= def.Steps.Count) { if (!string.IsNullOrEmpty(def.TurnInNpcId)) progress.Status = "pending_turnin"; else res.Logs.AddRange(CompleteQuest(db, p, questId).Logs); }
            else { progress.CurrentStepIndex = next; progress.StepStartedAt = p.Age; progress.ObjectiveProgress.Clear(); InitObjectives(def, next, progress); }
            res.Success = true; return res;
        }
        public static QuestResult CompleteQuest(GameDatabase db, Player p, string questId)
        {
            var res = new QuestResult { Player = p, QuestId = questId }; var s = GetQuestState(p); if (!db.QuestChains.TryGetValue(questId, out var def)) return res;
            res.Logs.AddRange(WorldJson.ApplyReward(db, p, def.Rewards)); s.ActiveQuests.Remove(questId); s.CompletedQuests.TryGetValue(questId, out var old); s.CompletedQuests[questId] = new QuestCompletion { QuestId = questId, CompletedAt = p.Age, RepeatCount = (old?.RepeatCount ?? 0) + 1 }; res.Success = true; res.Logs.Add($"completed:{questId}");
            res.Logs.AddRange(CheckQuestDiscovery(db, p, new QuestTrigger { Type = "quest_complete", QuestId = questId }).Logs); return res;
        }
        private static void InitObjectives(QuestChainDef def, int stepIndex, QuestProgress p) { int i = 0; foreach (var _ in WorldJson.Objects(def.Steps[stepIndex]["objectives"])) p.ObjectiveProgress.Add(new ObjectiveProgress { ObjectiveIndex = i++ }); }
        private static int GetObjectiveDelta(Player p, JObject obj, QuestTrigger trigger, QuestProgress progress)
        {
            string type = obj.Value<string>("type"), target = obj.Value<string>("targetId"); int count = obj.Value<int?>("count") ?? 1;
            return type switch { "kill_monster" when trigger.Type == "kill_monster" && trigger.MonsterId == target => 1, "collect_item" when (trigger.Type == "item_change" || trigger.Type == "explore" || trigger.Type == "combat" || trigger.Type == "craft_item") && WorldJson.HasItem(p, target, count) => count, "reach_region" when trigger.Type == "reach_region" && trigger.RegionId == target => 1, "reach_realm" when trigger.Type == "reach_realm" && trigger.RealmIndex >= (obj.Value<int?>("minRealmIndex") ?? 0) => 1, "talk_npc" when trigger.Type == "talk_npc" && trigger.NpcId == target => 1, "craft_item" when trigger.Type == "craft_item" && (trigger.RecipeId == target || trigger.OutputItemId == target) => 1, "explore_count" when trigger.Type == "explore" => 1, "cultivate_count" when trigger.Type == "cultivate" => 1, "combat_count" when trigger.Type == "combat" => 1, "survive_months" when trigger.Type == "time_tick" && p.Age - progress.StepStartedAt >= count => count, _ => 0 };
        }
    }
}
