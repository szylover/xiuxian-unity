// ============================================================
// BreakthroughAndTribulation.cs — breakthrough + tribulation orchestration
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class ItemCheckResult { public string ItemId, Name; public int Required, Have; public bool Ready; }
    public sealed class CondCheckResult { public string Id, Description; public bool Ready; }

    public sealed class BreakthroughStatus
    {
        public bool CanAttempt, ExpReady, RequiresTribulation, RequiresAscension;
        public RealmDef NextRealm;
        public BreakthroughReqDef Req;
        public readonly List<ItemCheckResult> ItemsReady = new();
        public readonly List<CondCheckResult> ConditionsReady = new();
        public double SuccessRate;
    }

    public sealed class BreakthroughResult
    {
        public bool Success, TriggerTribulation, BlockedByBottleneck;
        public Player Player;
        public readonly List<string> Logs = new();
        public double Roll, SuccessRate;
    }

    public static class BreakthroughSystem
    {
        public static BreakthroughStatus GetBreakthroughStatus(GameDatabase db, Player p)
        {
            var next = PlayerStatsSystem.GetNextRealm(db, p);
            var status = new BreakthroughStatus { NextRealm = next };
            if (next == null) return status;

            db.BreakthroughReqs.TryGetValue(next.Index, out var req);
            status.Req = req;
            status.ExpReady = p.Exp >= (next.ExpReq ?? 0);

            foreach (var cost in req?.ItemCosts ?? new List<ItemCountDef>())
            {
                db.Items.TryGetValue(cost.ItemId, out var def);
                int have = InventorySystem.CountItem(p, cost.ItemId);
                status.ItemsReady.Add(new ItemCheckResult { ItemId = cost.ItemId, Name = def?.Name ?? cost.ItemId, Required = cost.Count, Have = have, Ready = have >= cost.Count });
            }

            foreach (var cond in req?.Conditions ?? new List<JToken>())
                status.ConditionsReady.Add(new CondCheckResult { Id = (string)cond["id"], Description = (string)cond["description"], Ready = CheckCondition(p, cond) });

            status.RequiresTribulation = req?.RequiresTribulation ?? false;
            int failCount = p.Breakthrough.FailedAttempts.TryGetValue(p.RealmIndex, out var count) ? count : 0;
            double baseRate = req?.BaseSuccessRate ?? SystemBalance.BreakthroughBaseRate;
            double failBonus = Math.Min(0.25, failCount * 0.05);
            status.SuccessRate = status.RequiresTribulation ? 0 : Clamp(baseRate + p.Comprehension * SystemBalance.BreakthroughComprehensionBonus + p.Luck * SystemBalance.BreakthroughLuckBonus + failBonus, 0.05, 0.95);
            status.CanAttempt = status.ExpReady && status.ItemsReady.All(x => x.Ready) && status.ConditionsReady.All(x => x.Ready);
            return status;
        }

        public static BreakthroughResult AttemptBreakthrough(GameDatabase db, Player p, IRng rng)
        {
            var status = GetBreakthroughStatus(db, p);
            var result = new BreakthroughResult { Player = p, SuccessRate = status.SuccessRate };
            if (status.NextRealm == null) { result.Logs.Add(SystemTexts.MaxRealm); return result; }
            if (!status.ExpReady) { result.Logs.Add(SystemTexts.ExpInsufficient); return result; }
            if (status.ItemsReady.Any(x => !x.Ready)) result.Logs.Add(SystemTexts.MaterialsInsufficient);
            if (status.ConditionsReady.Any(x => !x.Ready)) result.Logs.Add(SystemTexts.ConditionsNotMet);
            if (!status.CanAttempt) return result;

            var bn = BottleneckSystem.CheckBottleneck(db, p, "realm", p.RealmIndex, null);
            if (bn.Blocked && bn.Def != null)
            {
                if (bn.IsNewlyActivated) BottleneckSystem.ActivateBottleneck(db, p, bn.Def.Id);
                result.BlockedByBottleneck = true;
                result.Logs.Add(SystemTexts.BottleneckActivated);
                return result;
            }

            foreach (var cost in status.Req?.ItemCosts ?? new List<ItemCountDef>()) InventorySystem.RemoveItem(p, cost.ItemId, cost.Count);
            if (status.RequiresTribulation)
            {
                result.TriggerTribulation = true;
                result.Logs.Add(SystemTexts.TribulationRequired);
                return result;
            }

            double roll = rng.NextDouble();
            result.Roll = roll;
            if (roll < status.SuccessRate)
            {
                AdvanceRealm(db, p, 20);
                p.Breakthrough.FailedAttempts.Remove(p.RealmIndex - 1);
                p.Tracking.ConsecutiveBreakthroughFails = 0;
                result.Success = true;
                result.Logs.Add(SystemTexts.BreakthroughSuccess);
                return result;
            }

            int expLoss = (int)Math.Floor(p.Exp * SystemBalance.BreakthroughFailExpLoss);
            p.Exp -= expLoss;
            p.Mood = Math.Max(0, p.Mood - 20);
            p.Health = Math.Max(0, p.Health - 10);
            p.Breakthrough.FailedAttempts[p.RealmIndex] = (p.Breakthrough.FailedAttempts.TryGetValue(p.RealmIndex, out var c) ? c : 0) + 1;
            p.Tracking.ConsecutiveBreakthroughFails += 1;
            result.Logs.Add(SystemTexts.BreakthroughFailed);
            return result;
        }

        public static void AdvanceRealm(GameDatabase db, Player p, int moodGain)
        {
            p.RealmIndex += 1;
            var newRealm = db.Realms[p.RealmIndex];
            p.Lifespan += newRealm.LifespanBonus ?? 0;
            PlayerStatsSystem.RecalcStats(db, p);
            p.Hp = p.MaxHp; p.Mp = p.MaxMp; p.Stamina = p.MaxStamina;
            p.Mood = Math.Min(100, p.Mood + moodGain);
        }

        private static bool CheckCondition(Player p, JToken cond)
        {
            string field = (string)cond["field"];
            string op = (string)cond["op"];
            JToken value = cond["value"];
            object actual = GetField(p, field);
            if (actual is bool b) return op == "==" ? b == value.Value<bool>() : b != value.Value<bool>();
            double a = Convert.ToDouble(actual ?? 0), v = value.Value<double>();
            switch (op)
            {
                case ">=": return a >= v;
                case ">": return a > v;
                case "<=": return a <= v;
                case "<": return a < v;
                case "==": return Math.Abs(a - v) < 0.00001;
                case "!=": return Math.Abs(a - v) >= 0.00001;
                default: return false;
            }
        }

        private static object GetField(Player p, string field)
        {
            switch (field)
            {
                case "tracking.killCount": return p.Tracking.KillCount;
                case "tracking.bossKillCount": return p.Tracking.BossKillCount;
                case "tracking.defeatedHigherRealm": return p.Tracking.DefeatedHigherRealm;
                case "realmIndex": return p.RealmIndex;
                case "bodyRealmIndex": return p.BodyRealmIndex;
                case "luck": return p.Luck;
                case "comprehension": return p.Comprehension;
                default: return 0;
            }
        }

        private static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
    }

    public sealed class TribulationWave
    {
        public string Name;
        public int Hp, Atk, Def, Speed;
        public JToken SpecialEffect;
        public static TribulationWave FromToken(JToken token) => new TribulationWave
        {
            Name = (string)token["name"],
            Hp = (int?)token["hp"] ?? 0,
            Atk = (int?)token["atk"] ?? 0,
            Def = (int?)token["def"] ?? 0,
            Speed = (int?)token["speed"] ?? 0,
            SpecialEffect = token["specialEffect"],
        };
    }

    public sealed class WaveCombatResult
    {
        public bool Won;
        public int HpLeft;
        public readonly List<string> Logs = new();
    }

    public interface ICombatResolver
    {
        WaveCombatResult Resolve(Player player, TribulationWave wave, int currentHp, IRng rng);
    }

    public sealed class StubTribulationCombatResolver : ICombatResolver
    {
        public WaveCombatResult Resolve(Player player, TribulationWave wave, int currentHp, IRng rng)
        {
            // TODO(#3): replace this deterministic stub with the real combat system resolver.
            return new WaveCombatResult { Won = true, HpLeft = Math.Max(1, currentHp) };
        }
    }

    public sealed class TribulationResult
    {
        public bool Success;
        public Player Player;
        public readonly List<string> Logs = new();
        public int WavesCleared, TotalWaves;
    }

    public static class TribulationSystem
    {
        public static TribulationResult RunTribulation(GameDatabase db, Player p, ICombatResolver resolver, IRng rng)
        {
            if (!db.Tribulations.TryGetValue(p.RealmIndex, out var def))
            {
                var none = new TribulationResult { Player = p };
                none.Logs.Add(SystemTexts.TribulationNotFound);
                return none;
            }

            var result = new TribulationResult { Player = p, TotalWaves = def.Waves?.Count ?? 0 };
            int currentHp = p.Hp;
            for (int i = 0; i < result.TotalWaves; i++)
            {
                var wave = TribulationWave.FromToken(def.Waves[i]);
                var waveResult = resolver.Resolve(p, wave, currentHp, rng);
                result.Logs.AddRange(waveResult.Logs);
                if (!waveResult.Won)
                {
                    result.Logs.Add(SystemTexts.TribulationFailed);
                    p.Hp = 1;
                    ApplyFailure(db, p, def);
                    return result;
                }
                currentHp = waveResult.HpLeft;
                result.WavesCleared++;
            }

            result.Logs.Add(SystemTexts.TribulationSuccess);
            p.Hp = currentHp;
            BreakthroughSystem.AdvanceRealm(db, p, 30);
            int bonusExp = (int?)def.Rewards?["bonusExp"] ?? 0;
            p.Exp += bonusExp;
            var items = def.Rewards?["items"] as JArray;
            if (items != null)
                foreach (var item in items) InventorySystem.AddItem(p, (string)item["itemId"], (int)item["count"]);
            p.Breakthrough.TribulationsPassed.Add(def.Id);
            result.Success = true;
            return result;
        }

        private static void ApplyFailure(GameDatabase db, Player p, TribulationDef def)
        {
            if (def.FailureType == "realm_drop" && p.RealmIndex > 0)
            {
                p.RealmIndex -= 1;
                PlayerStatsSystem.RecalcStats(db, p);
                p.Health = 0;
            }
            else if (def.FailureType == "become_loose_immortal")
            {
                p.Breakthrough.IsLooseImmortal = true;
            }
        }
    }
}
