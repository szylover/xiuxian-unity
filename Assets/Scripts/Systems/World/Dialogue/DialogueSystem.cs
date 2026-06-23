// ============================================================
// DialogueSystem.cs — branching dialogue flow
// UnityEngine-free
// ============================================================
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class DialogueResult { public Player Player; public JToken Node; public readonly List<string> Logs = new(); public string CombatTrigger, QuestTrigger; }
    public static class DialogueSystem
    {
        public static DialogueSystemState GetDialogueState(Player p) => WorldState.Get(p, "dialogue", () => new DialogueSystemState());
        public static List<DialogueChainDef> GetAvailableDialogues(GameDatabase db, Player p, string npcId)
        {
            var state = GetDialogueState(p);
            return db.Dialogues.Values.Where(d => d.NpcId == npcId)
                .Where(d => !(d.Once ?? false) || !state.TriggeredOnce.Contains(d.Id))
                .Where(d => !(d.Cooldown.HasValue && state.LastTriggerAge.TryGetValue(d.Id, out var last) && p.Age - last < d.Cooldown.Value))
                .Where(d => WorldConditions.CheckBasic(db, p, d.Condition, d.NpcId) && CheckDialogueSpecific(p, d.Condition, d.NpcId))
                .OrderByDescending(d => d.Priority ?? 0).ToList();
        }
        public static DialogueChainDef GetTopDialogue(GameDatabase db, Player p, string npcId) => GetAvailableDialogues(db, p, npcId).FirstOrDefault();
        public static DialogueResult StartDialogue(GameDatabase db, Player p, string dialogueId)
        {
            var res = new DialogueResult { Player = p }; if (!db.Dialogues.TryGetValue(dialogueId, out var def)) return res;
            var state = GetDialogueState(p); if (!state.TriggeredOnce.Contains(def.Id)) state.TriggeredOnce.Add(def.Id); state.LastTriggerAge[def.Id] = p.Age;
            res.Node = FindNode(def, def.StartNodeId); if (res.Node?["effects"] != null) ApplyEffects(db, p, def.NpcId, res.Node["effects"], res);
            return res;
        }
        public static DialogueResult SelectChoice(GameDatabase db, Player p, string dialogueId, string nodeId, string choiceId)
        {
            var res = new DialogueResult { Player = p }; if (!db.Dialogues.TryGetValue(dialogueId, out var def)) return res;
            var choice = def.Nodes?.SelectMany(n => WorldJson.Objects(n["choices"])).FirstOrDefault(c => c.Value<string>("id") == choiceId); if (choice == null) return res;
            if (choice["effects"] != null) ApplyEffects(db, p, def.NpcId, choice["effects"], res);
            var nextId = choice.Value<string>("nextNodeId"); res.Node = string.IsNullOrEmpty(nextId) ? null : FindNode(def, nextId);
            if (res.Node?["effects"] != null) ApplyEffects(db, p, def.NpcId, res.Node["effects"], res);
            return res;
        }
        public static DialogueResult AdvanceToNextNode(GameDatabase db, Player p, string dialogueId, string nodeId)
        {
            var res = new DialogueResult { Player = p }; if (!db.Dialogues.TryGetValue(dialogueId, out var def)) return res;
            var node = FindNode(def, nodeId); var nextId = node?.Value<string>("nextNodeId"); res.Node = string.IsNullOrEmpty(nextId) ? null : FindNode(def, nextId);
            if (res.Node?["effects"] != null) ApplyEffects(db, p, def.NpcId, res.Node["effects"], res); return res;
        }
        public static List<JObject> GetAvailableChoices(Player p, string npcId, JToken node)
        {
            return WorldJson.Objects(node?["choices"]).Where(c => CheckChoiceCondition(p, npcId, c["condition"])).ToList();
        }
        private static JToken FindNode(DialogueChainDef d, string id) => d.Nodes?.FirstOrDefault(n => n.Value<string>("id") == id);
        private static bool CheckDialogueSpecific(Player p, JToken cond, string npcId)
        {
            if (cond == null) return true; var q = QuestSystem.GetQuestState(p); var dlg = GetDialogueState(p);
            foreach (var id in cond["requiredQuests"]?.Values<string>() ?? Enumerable.Empty<string>()) if (!q.CompletedQuests.ContainsKey(id)) return false;
            var active = cond.Value<string>("hasActiveQuest"); if (!string.IsNullOrEmpty(active) && !q.ActiveQuests.ContainsKey(active)) return false;
            foreach (var id in cond["requiredDialogues"]?.Values<string>() ?? Enumerable.Empty<string>()) if (!dlg.TriggeredOnce.Contains(id)) return false;
            return true;
        }
        private static bool CheckChoiceCondition(Player p, string npcId, JToken cond)
        {
            if (cond == null) return true; if (!WorldConditions.CheckBasic(null, p, cond, npcId)) return false;
            if (cond["hasItem"] is JObject item && !WorldJson.HasItem(p, item.Value<string>("itemId"), item.Value<int?>("count") ?? 1)) return false;
            var q = QuestSystem.GetQuestState(p); var completed = cond.Value<string>("completedQuest"); if (!string.IsNullOrEmpty(completed) && !q.CompletedQuests.ContainsKey(completed)) return false;
            var active = cond.Value<string>("hasActiveQuest"); if (!string.IsNullOrEmpty(active) && !q.ActiveQuests.ContainsKey(active)) return false; return true;
        }
        private static void ApplyEffects(GameDatabase db, Player p, string npcId, JToken effect, DialogueResult res)
        {
            int affinity = effect.Value<int?>("affinityChange") ?? 0; if (affinity != 0) { var r = NpcSystem.ChangeAffinity(db, p, npcId, affinity, "dialogue"); res.Logs.Add($"affinity:{r.AffinityChange}"); }
            foreach (var it in WorldJson.Objects(effect["giveItems"])) { InventorySystem.AddItem(db, p, it.Value<string>("itemId"), it.Value<int?>("count") ?? 1); res.Logs.Add($"give:{it.Value<string>("itemId")}"); }
            foreach (var it in WorldJson.Objects(effect["takeItems"])) { InventorySystem.RemoveItem(p, it.Value<string>("itemId"), it.Value<int?>("count") ?? 1); res.Logs.Add($"take:{it.Value<string>("itemId")}"); }
            int gold = effect.Value<int?>("goldChange") ?? 0; if (gold != 0) p.Gold = System.Math.Max(0, p.Gold + gold);
            int exp = effect.Value<int?>("expChange") ?? 0; if (exp != 0) p.Exp = System.Math.Max(0, p.Exp + exp);
            int karma = effect.Value<int?>("karmaChange") ?? 0; if (karma != 0) p.Karma = System.Math.Max(-100, System.Math.Min(100, p.Karma + karma));
            if (effect["statBonus"] is JObject stats) foreach (var kv in stats) WorldJson.AddNumeric(p, kv.Key, kv.Value.Value<int>());
            var npcFlag = effect["setNpcFlag"] as JObject; if (npcFlag != null) NpcSystem.GetRelation(p, npcId).Flags[npcFlag.Value<string>("key")] = npcFlag["value"]?.ToObject<object>();
            var dlgFlag = effect["setDialogueFlag"] as JObject; if (dlgFlag != null) GetDialogueState(p).Flags[dlgFlag.Value<string>("key")] = dlgFlag["value"]?.ToObject<object>();
            var unlock = effect.Value<string>("unlockDialogueId"); if (!string.IsNullOrEmpty(unlock)) GetDialogueState(p).LastTriggerAge.Remove(unlock);
            res.CombatTrigger = effect.Value<string>("triggerCombatNpcId") ?? res.CombatTrigger; res.QuestTrigger = effect.Value<string>("triggerQuestId") ?? res.QuestTrigger;
        }
    }
}
