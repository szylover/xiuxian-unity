// ============================================================
// ProgressionSystems.cs — stats, cultivation, body, bottleneck helpers
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public static class SystemBalance
    {
        public const int BaseCultivateExp = 10;
        public const double BreakthroughBaseRate = 0.5;
        public const double BreakthroughComprehensionBonus = 0.003;
        public const double BreakthroughLuckBonus = 0.002;
        public const double BreakthroughFailExpLoss = 0.1;
    }

    public static class PlayerStatsSystem
    {
        public static SpiritRootGrade GetSpiritRootGrade(Aptitudes a)
        {
            int[] roots = { a.Fire, a.Water, a.Thunder, a.Wind, a.Earth, a.Wood };
            double avg = roots.Average();
            int high = roots.Count(v => v > 90), low = roots.Count(v => v < 30);
            if (high == 1 && low == 5) return new SpiritRootGrade { Grade = SystemTexts.RootSingle, Multiplier = 3.0, Color = "#FFD700" };
            if (avg > 85) return new SpiritRootGrade { Grade = SystemTexts.RootHeaven, Multiplier = 2.0, Color = "#FFD700" };
            if (avg > 65) return new SpiritRootGrade { Grade = SystemTexts.RootVariant, Multiplier = 1.5, Color = "#9C27B0" };
            if (avg > 40) return new SpiritRootGrade { Grade = SystemTexts.RootNormal, Multiplier = 1.0, Color = "#4CAF50" };
            if (avg > 20) return new SpiritRootGrade { Grade = SystemTexts.RootMixed, Multiplier = 0.7, Color = "#FF9800" };
            return new SpiritRootGrade { Grade = SystemTexts.RootWaste, Multiplier = 0.4, Color = "#9E9E9E" };
        }

        public static SpiritRootGrade GetSpiritRootDisplay(PlayerSpiritRoots spiritRoots)
        {
            switch (spiritRoots.Combo)
            {
                case "none": return new SpiritRootGrade { Grade = SystemTexts.RootNone, Multiplier = spiritRoots.CultivationMultiplier, Color = "#9E9E9E" };
                case "single": return new SpiritRootGrade { Grade = SystemTexts.RootSingle, Multiplier = spiritRoots.CultivationMultiplier, Color = "#FFD700" };
                case "dual": return new SpiritRootGrade { Grade = SystemTexts.RootDual, Multiplier = spiritRoots.CultivationMultiplier, Color = "#FF5722" };
                case "triple": return new SpiritRootGrade { Grade = SystemTexts.RootTriple, Multiplier = spiritRoots.CultivationMultiplier, Color = "#9C27B0" };
                case "quad": return new SpiritRootGrade { Grade = SystemTexts.RootQuad, Multiplier = spiritRoots.CultivationMultiplier, Color = "#4CAF50" };
                default: return new SpiritRootGrade { Grade = SystemTexts.RootPenta, Multiplier = spiritRoots.CultivationMultiplier, Color = "#607D8B" };
            }
        }

        public static Player RecalcStats(GameDatabase db, Player p)
        {
            if (!db.Realms.TryGetValue(p.RealmIndex, out var realm)) return p;
            p.MaxStamina = 100 + realm.Index * 10;
            p.InventoryCapacity = 20 + realm.Index * 5;
            p.MaxHp = realm.HpBase ?? p.MaxHp;
            p.MaxMp = realm.MpBase ?? p.MaxMp;
            p.MaxMentalPower = realm.MentalBase ?? p.MaxMentalPower;
            p.Atk = realm.AtkBase ?? p.Atk;
            p.Def = realm.DefBase ?? p.Def;
            p.Speed = realm.SpeedBase ?? p.Speed;

            var body = BodyCultivationSystem.GetBodyRealmBonus(db, p);
            p.MaxHp += body.Hp;
            p.Atk += body.Atk;
            p.Def += body.Def;
            p.MaxPhysique = body.MaxPhysique;
            p.PhysiqueDmgReduce = Math.Min(50, body.PhysiqueDmgReduce);

            var equip = EquipmentSystem.GetEquipmentStatBonus(db, p);
            p.MaxHp += equip.Hp;
            p.MaxMp += equip.Mp;
            p.Atk += equip.Atk;
            p.Def += equip.Def;
            p.Speed += equip.Speed;
            p.MoveSpeed += equip.MoveSpeed;
            p.CritRate += equip.CritRate;
            p.CritDmgMultiplier += equip.CritDmgMultiplier;
            p.CritResist += equip.CritResist;
            p.MaxPhysique += equip.Physique;
            p.PhysiqueDmgReduce = Math.Min(50, p.PhysiqueDmgReduce + equip.PhysiqueDmgReduce);

            p.Hp = Math.Min(p.Hp, p.MaxHp);
            p.Mp = Math.Min(p.Mp, p.MaxMp);
            p.Stamina = Math.Min(p.Stamina, p.MaxStamina);
            p.MentalPower = Math.Min(p.MentalPower, p.MaxMentalPower);
            p.Physique = Math.Min(p.Physique, p.MaxPhysique);
            return p;
        }

        public static RealmDef GetNextRealm(GameDatabase db, Player p)
        {
            return db.Realms.TryGetValue(p.RealmIndex + 1, out var next) && next.AscensionRequired == null ? next : null;
        }
    }

    public sealed class BodyRealmBonus
    {
        public int Hp, Atk, Def, MaxPhysique;
        public double PhysiqueDmgReduce;
    }

    public sealed class BodyBreakthroughStatus
    {
        public bool CanAttempt, ExpReady, PhysiqueReady;
        public BodyRealmDef NextRealm;
        public int PhysiqueRequired;
    }

    public sealed class BodyGainResult
    {
        public Player Player;
        public bool Breakthrough, BlockedByBottleneck;
        public string Message;
        public int ActualGain;
    }

    public static class BodyCultivationSystem
    {
        public static BodyRealmDef GetNextBodyRealm(GameDatabase db, Player player) => db.BodyRealms.TryGetValue(player.BodyRealmIndex + 1, out var r) ? r : null;

        public static double GetSpiritRootBodyExpMultiplier(GameDatabase db, Player p)
        {
            double multiplier = 1.0;
            foreach (var root in p.SpiritRoots.Roots)
                if (db.SpiritRootBodyBonuses.TryGetValue(root.Type, out var b) && b.BodyExpMultiplier.HasValue)
                    multiplier += (b.BodyExpMultiplier.Value - 1) * (root.Affinity / 100.0);
            return multiplier;
        }

        public static double GetSpiritRootRegenMultiplier(GameDatabase db, Player p)
        {
            double multiplier = 1.0;
            foreach (var root in p.SpiritRoots.Roots)
                if (db.SpiritRootBodyBonuses.TryGetValue(root.Type, out var b) && b.PhysiqueRegenRate.HasValue)
                    multiplier += (b.PhysiqueRegenRate.Value - 1) * (root.Affinity / 100.0);
            return multiplier;
        }

        public static double GetSpiritRootDmgReduceBonus(GameDatabase db, Player p)
        {
            double bonus = 0;
            foreach (var root in p.SpiritRoots.Roots)
                if (db.SpiritRootBodyBonuses.TryGetValue(root.Type, out var b) && b.DmgReduceBonus.HasValue)
                    bonus += b.DmgReduceBonus.Value * (root.Affinity / 100.0);
            return bonus;
        }

        public static double GetSpiritRootHpBonusRate(GameDatabase db, Player p)
        {
            double rate = 0;
            foreach (var root in p.SpiritRoots.Roots)
                if (db.SpiritRootBodyBonuses.TryGetValue(root.Type, out var b) && b.HpBonusRate.HasValue)
                    rate += b.HpBonusRate.Value * (root.Affinity / 100.0);
            return rate;
        }

        public static BodyBreakthroughStatus GetBodyBreakthroughStatus(GameDatabase db, Player p)
        {
            var next = GetNextBodyRealm(db, p);
            if (next == null) return new BodyBreakthroughStatus();
            int required = (int)Math.Ceiling(p.MaxPhysique * 0.8);
            bool expReady = p.BodyRealmExp >= (next.ExpReq ?? 0), physiqueReady = p.Physique >= required;
            return new BodyBreakthroughStatus { NextRealm = next, ExpReady = expReady, PhysiqueReady = physiqueReady, PhysiqueRequired = required, CanAttempt = expReady && physiqueReady };
        }

        public static BodyGainResult TryBodyRealmBreakthrough(GameDatabase db, Player p)
        {
            var next = GetNextBodyRealm(db, p);
            if (next == null) return new BodyGainResult { Player = p };
            if (p.BodyRealmExp < (next.ExpReq ?? 0) || p.Physique < p.MaxPhysique * 0.8) return new BodyGainResult { Player = p };

            var bn = BottleneckSystem.CheckBottleneck(db, p, "body_realm", p.BodyRealmIndex, null);
            if (bn.Blocked && bn.Def != null)
            {
                if (bn.IsNewlyActivated) BottleneckSystem.ActivateBottleneck(db, p, bn.Def.Id);
                return new BodyGainResult { Player = p, BlockedByBottleneck = true, Message = SystemTexts.BottleneckActivated };
            }

            p.BodyRealmIndex = next.Index;
            p.BodyRealmExp = 0;
            p.BodyTempering += 1;
            p.MaxPhysique = next.MaxPhysique ?? p.MaxPhysique;
            p.PhysiqueDmgReduce = next.PhysiqueDmgReduce ?? p.PhysiqueDmgReduce;
            return new BodyGainResult { Player = p, Breakthrough = true };
        }

        public static BodyGainResult GainBodyRealmExp(GameDatabase db, Player p, int baseAmount)
        {
            int actual = (int)Math.Floor(baseAmount * GetSpiritRootBodyExpMultiplier(db, p));
            if (actual <= 0) return new BodyGainResult { Player = p, ActualGain = 0 };
            p.BodyRealmExp += actual;
            var bt = TryBodyRealmBreakthrough(db, p);
            bt.ActualGain = actual;
            return bt;
        }

        public static BodyRealmBonus GetBodyRealmBonus(GameDatabase db, Player p)
        {
            if (!db.BodyRealms.TryGetValue(p.BodyRealmIndex, out var r)) return new BodyRealmBonus { MaxPhysique = 50 };
            double hpRate = GetSpiritRootHpBonusRate(db, p);
            return new BodyRealmBonus
            {
                Hp = (int)Math.Floor((r.HpBonus ?? 0) * (1 + hpRate)),
                Atk = r.AtkBonus ?? 0,
                Def = r.DefBonus ?? 0,
                MaxPhysique = r.MaxPhysique ?? 50,
                PhysiqueDmgReduce = (r.PhysiqueDmgReduce ?? 0) + GetSpiritRootDmgReduceBonus(db, p),
            };
        }

        public static Player RestorePhysique(GameDatabase db, Player p)
        {
            int restore = (int)Math.Floor(p.MaxPhysique * 0.1 * GetSpiritRootRegenMultiplier(db, p));
            p.Physique = Math.Min(p.MaxPhysique, p.Physique + restore);
            return p;
        }
    }

    public sealed class BottleneckCheckResult
    {
        public bool Blocked, IsNewlyActivated;
        public BottleneckDef Def;
    }

    public static class BottleneckSystem
    {
        public static IEnumerable<(BottleneckDef Def, BottleneckEntry Entry)> GetActiveBottlenecks(GameDatabase db, Player p)
        {
            foreach (var kv in p.Bottleneck.Active)
                if (db.Bottlenecks.TryGetValue(kv.Key, out var def)) yield return (def, kv.Value);
        }

        public static BottleneckCheckResult CheckBottleneck(GameDatabase db, Player p, string targetType, int index, string techniqueId)
        {
            IEnumerable<BottleneckDef> defs = db.Bottlenecks.Values.Where(d => d.TargetType == targetType);
            if (targetType == "realm") defs = defs.Where(d => d.FromRealmIndex == index);
            else if (targetType == "body_realm") defs = defs.Where(d => d.FromBodyRealmIndex == index);
            else defs = defs.Where(d => d.TechniqueId == techniqueId && d.AtLevel == index);

            foreach (var def in defs.OrderBy(d => d.Id))
            {
                if (p.Bottleneck.Unlocked.ContainsKey(def.Id)) continue;
                return new BottleneckCheckResult { Blocked = true, Def = def, IsNewlyActivated = !p.Bottleneck.Active.ContainsKey(def.Id) };
            }
            return new BottleneckCheckResult();
        }

        public static string ActivateBottleneck(GameDatabase db, Player p, string bottleneckId)
        {
            if (!db.Bottlenecks.ContainsKey(bottleneckId) || p.Bottleneck.Active.ContainsKey(bottleneckId) || p.Bottleneck.Unlocked.ContainsKey(bottleneckId)) return string.Empty;
            p.Bottleneck.Active[bottleneckId] = new BottleneckEntry { BottleneckId = bottleneckId, ActivatedAt = p.GameYear, PersistenceCultivationCount = 0 };
            return SystemTexts.BottleneckActivated;
        }

        public static string UnlockBottleneck(GameDatabase db, Player p, string bottleneckId, string method)
        {
            if (!db.Bottlenecks.TryGetValue(bottleneckId, out var def) || !p.Bottleneck.Active.ContainsKey(bottleneckId) || p.Bottleneck.Unlocked.ContainsKey(bottleneckId)) return string.Empty;
            p.Bottleneck.Active.Remove(bottleneckId);
            p.Bottleneck.Unlocked[bottleneckId] = new BottleneckUnlockedEntry { BottleneckId = bottleneckId, UnlockedAt = p.GameYear, Method = method };
            ApplyUnlockBonus(p, def.UnlockBonus);
            return SystemTexts.BottleneckUnlocked;
        }

        public static (Player Player, bool Unlocked, string Log) TickPersistenceCultivation(GameDatabase db, Player p, string bottleneckId)
        {
            if (!db.Bottlenecks.TryGetValue(bottleneckId, out var def) || !p.Bottleneck.Active.TryGetValue(bottleneckId, out var entry)) return (p, false, string.Empty);
            var method = (def.UnlockMethods ?? new List<JToken>()).FirstOrDefault(m => (string)m["type"] == "persistence");
            if (method == null) return (p, false, string.Empty);
            entry.PersistenceCultivationCount += 1;
            int required = (int?)method["cultivationCount"] ?? int.MaxValue;
            if (entry.PersistenceCultivationCount >= required)
            {
                string log = UnlockBottleneck(db, p, bottleneckId, "persistence");
                return (p, true, log);
            }
            return (p, false, string.Empty);
        }

        public static (Player Player, bool Triggered, string Log) TryOverflowUnlock(GameDatabase db, Player p)
        {
            foreach (string id in p.Bottleneck.Active.Keys.ToList())
            {
                var def = db.Bottlenecks[id];
                double ratio = def.OverflowRatio ?? 1.5;
                if (ratio <= 0 || double.IsInfinity(ratio)) continue;
                if (def.TargetType == "realm" && def.FromRealmIndex.HasValue && db.Realms.TryGetValue(def.FromRealmIndex.Value + 1, out var next) && p.Exp >= (next.ExpReq ?? 0) * ratio)
                    return (p, true, UnlockBottleneck(db, p, id, "overflow"));
                if (def.TargetType == "body_realm" && def.FromBodyRealmIndex.HasValue && db.BodyRealms.TryGetValue(def.FromBodyRealmIndex.Value + 1, out var nextBody) && p.BodyRealmExp >= (nextBody.ExpReq ?? 0) * ratio)
                    return (p, true, UnlockBottleneck(db, p, id, "overflow"));
            }
            return (p, false, string.Empty);
        }

        private static void ApplyUnlockBonus(Player p, JToken bonus)
        {
            if (bonus == null) return;
            p.Exp += (int?)bonus["expBonus"] ?? 0;
            var stat = bonus["statBonus"] as JObject;
            if (stat != null)
                foreach (var kv in stat)
                    ApplyStat(p, kv.Key, kv.Value.Value<int>());
            var items = bonus["items"] as JArray;
            if (items != null)
                foreach (var item in items) InventorySystem.AddItem(p, (string)item["itemId"], (int)item["count"]);
        }

        private static void ApplyStat(Player p, string key, int value)
        {
            if (key == "atk") p.Atk += value;
            else if (key == "def") p.Def += value;
            else if (key == "comprehension") p.Comprehension += value;
            else if (key == "luck") p.Luck += value;
        }

        private static double ReadDouble(JToken token, double fallback) => token == null ? fallback : token.Value<double>();
    }

    public sealed class CultivationGainResult
    {
        public Player Player;
        public int ExpGain;
        public bool OverflowUnlocked;
        public readonly List<string> Logs = new();
    }

    public static class CultivationSystem
    {
        public static CultivationGainResult GainCultivation(GameDatabase db, Player p)
        {
            double compBonus = 1 + p.Comprehension / 50.0;
            double cultivationMult = p.SpiritRoots?.CultivationMultiplier ?? PlayerStatsSystem.GetSpiritRootGrade(p.Aptitudes).Multiplier;
            double moodBonus = 0.5 + p.Mood / 100.0;
            int expGain = (int)Math.Floor(SystemBalance.BaseCultivateExp * compBonus * cultivationMult * moodBonus);
            p.Exp += expGain;
            p.Tracking.ConsecutiveCultivates += 1;
            p.Tracking.ConsecutiveRests = 0;
            foreach (var active in BottleneckSystem.GetActiveBottlenecks(db, p).ToList())
            {
                var tick = BottleneckSystem.TickPersistenceCultivation(db, p, active.Entry.BottleneckId);
                if (tick.Unlocked && !string.IsNullOrEmpty(tick.Log)) { }
            }
            var overflow = BottleneckSystem.TryOverflowUnlock(db, p);
            return new CultivationGainResult { Player = p, ExpGain = expGain, OverflowUnlocked = overflow.Triggered };
        }
    }
}
