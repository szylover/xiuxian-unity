// ============================================================
// SecretRealmSystem.cs — secret realm exploration runs
// UnityEngine-free
// ============================================================
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class SecretRealmResult { public Player Player; public bool Success; public readonly System.Collections.Generic.List<string> Logs = new(); public CombatResult Combat; }
    public static class SecretRealmSystem
    {
        public static SecretRealmSystemState GetSecretRealmState(Player p) => WorldState.Get(p, "secretRealm", () => new SecretRealmSystemState());
        public static string GetSecretRealmLockReason(GameDatabase db, Player p, string realmId)
        {
            if (!db.SecretRealms.TryGetValue(realmId, out var def)) return "notFound"; var s = GetSecretRealmState(p); if (p.RealmIndex < (def.MinRealm ?? 0)) return "realm"; if (MapSystem.GetCurrentRegion(db, p)?.Id != def.RegionId) return "region"; if ((s.Cooldowns.TryGetValue(realmId, out var cd) ? cd : 0) > p.Age) return "cooldown";
            int stamina = def.EntryCost?.Value<int?>("stamina") ?? 0, gold = def.EntryCost?.Value<int?>("gold") ?? 0; if (p.Stamina < stamina || p.Gold < gold) return "cost"; string item = def.EntryCost?.Value<string>("itemId"); int count = def.EntryCost?.Value<int?>("itemCount") ?? 0; if (!string.IsNullOrEmpty(item) && !WorldJson.HasItem(p, item, count)) return "cost"; return null;
        }
        public static SecretRealmResult StartSecretRealm(GameDatabase db, Player p, string realmId)
        {
            var res = new SecretRealmResult { Player = p }; var lockReason = GetSecretRealmLockReason(db, p, realmId); if (lockReason != null) { res.Logs.Add(lockReason); return res; } var def = db.SecretRealms[realmId]; var s = GetSecretRealmState(p); if (s.ActiveRun != null && !s.ActiveRun.Completed && !s.ActiveRun.Failed) return res;
            p.Stamina -= def.EntryCost?.Value<int?>("stamina") ?? 0; p.Gold -= def.EntryCost?.Value<int?>("gold") ?? 0; string item = def.EntryCost?.Value<string>("itemId"); if (!string.IsNullOrEmpty(item)) InventorySystem.RemoveItem(p, item, def.EntryCost.Value<int?>("itemCount") ?? 1);
            s.ActiveRun = new SecretRealmRun { RealmId = realmId, StartedAt = p.Age, StageIndex = 0, Rewards = new JObject() }; res.Success = true; res.Logs.Add($"started:{realmId}"); return res;
        }
        public static SecretRealmResult AdvanceSecretRealm(GameDatabase db, Player p, IRng rng)
        {
            var res = new SecretRealmResult { Player = p }; var s = GetSecretRealmState(p); var run = s.ActiveRun; if (run == null || !db.SecretRealms.TryGetValue(run.RealmId, out var def)) return res; var stage = def.Stages?.ElementAtOrDefault(run.StageIndex) as JObject; if (stage == null) return FinishSecretRealm(db, p);
            string type = stage.Value<string>("type");
            if (type == "treasure") { run.Rewards = WorldJson.MergeRewards(run.Rewards, stage["reward"]); res.Logs.Add("treasure"); }
            else if (type == "trap") { p.Hp = System.Math.Max(1, p.Hp - System.Math.Max(1, (int)System.Math.Floor(p.MaxHp * (stage.Value<double?>("damageRate") ?? ((def.Risk?.Value<double?>() ?? 0)))))); p.Mp = System.Math.Max(0, p.Mp - (int)System.Math.Floor(p.MaxMp * (stage.Value<double?>("mpDamageRate") ?? (((def.Risk?.Value<double?>() ?? 0)) / 2)))); res.Logs.Add("trap"); }
            else if (type == "rest") { p.Hp = System.Math.Min(p.MaxHp, p.Hp + System.Math.Max(1, (int)System.Math.Floor(p.MaxHp * 0.2))); p.Mp = System.Math.Min(p.MaxMp, p.Mp + System.Math.Max(1, (int)System.Math.Floor(p.MaxMp * 0.2))); res.Logs.Add("rest"); }
            else { var monsterId = stage.Value<string>("monsterId"); if (!string.IsNullOrEmpty(monsterId) && db.Monsters.TryGetValue(monsterId, out var monster)) { var combat = CombatEngine.RunCombat(db, p, monster, rng); res.Combat = combat; if (combat.Winner == "player") { run.Rewards = WorldJson.MergeRewards(run.Rewards, stage["reward"]); run.Rewards = WorldJson.MergeRewards(run.Rewards, new JObject { ["exp"] = combat.ExpGained, ["gold"] = combat.GoldGained }); res.Logs.Add("combatWin"); } else { run.Failed = true; res.Logs.Add("combatLose"); } } }
            run.StageIndex++; if (!run.Failed && run.StageIndex >= (def.Stages?.Count ?? 0)) { run.Completed = true; run.Rewards = WorldJson.MergeRewards(run.Rewards, def.CompletionReward); } res.Success = true; run.Logs.AddRange(res.Logs); return res;
        }
        public static SecretRealmResult FinishSecretRealm(GameDatabase db, Player p)
        {
            var res = new SecretRealmResult { Player = p }; var s = GetSecretRealmState(p); var run = s.ActiveRun; if (run == null || !db.SecretRealms.TryGetValue(run.RealmId, out var def)) return res; if (run.Completed) { res.Logs.AddRange(WorldJson.ApplyReward(db, p, run.Rewards)); res.Success = true; }
            s.Cooldowns[def.Id] = p.Age + (def.CooldownMonths ?? 0); s.CompletedRuns[def.Id] = (s.CompletedRuns.TryGetValue(def.Id, out var c) ? c : 0) + (run.Completed ? 1 : 0); s.ActiveRun = null; return res;
        }
        public static SecretRealmResult RunToEnd(GameDatabase db, Player p, string realmId, IRng rng) { var res = StartSecretRealm(db, p, realmId); while (GetSecretRealmState(p).ActiveRun != null && !GetSecretRealmState(p).ActiveRun.Completed && !GetSecretRealmState(p).ActiveRun.Failed) { var a = AdvanceSecretRealm(db, p, rng); res.Logs.AddRange(a.Logs); } var f = FinishSecretRealm(db, p); res.Logs.AddRange(f.Logs); res.Success = f.Success; return res; }
    }
}

