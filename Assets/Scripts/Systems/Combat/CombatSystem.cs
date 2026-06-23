// ============================================================
// CombatSystem.cs — combatant assembly, damage, skills, divine arts
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class Combatant
    {
        public string Id, Name, Element;
        public int RealmIndex, Hp, MaxHp, Atk, Def, Speed, MoveSpeed, ExpReward, GoldReward;
        public double CritRate, CritDmgMultiplier = 1.5, CritResist, PhysiqueDmgReduce;
        public Dictionary<string, double> ElementResists = new();
    }

    public sealed class DamageResult
    {
        public int Damage;
        public bool IsCrit, IsDodge;
        public readonly List<string> Logs = new();
    }

    public sealed class CombatHitSnapshot
    {
        public string SourceName, TargetName;
        public int Damage;
        public bool IsCrit, IsDodge, FromPlayer;
    }

    public sealed class SkillState
    {
        public int CooldownLeft, TotalMpUsed, TotalStaminaUsed, UseCount;
    }

    public sealed class StatusEffect
    {
        public string Type, SourceName;
        public int Value, RemainingRounds;
    }

    public sealed class RoundSnapshot
    {
        public int Round, PlayerHp, PlayerMp, MonsterHp;
    }

    public sealed class CombatResult
    {
        public string Winner;
        public int PlayerHpLeft, ExpGained, GoldGained, MpUsed, SkillUseCount, MonsterMaxHp, PlayerMaxHp, PlayerMaxMp, BodyExpGained;
        public readonly List<string> Logs = new();
        public readonly List<RoundSnapshot> Snapshots = new();
        public readonly List<CombatHitSnapshot> HitSnapshots = new();
    }

    public sealed class ActiveSkillInfo
    {
        public TechniqueDef Def;
        public TechniqueSlot Slot;
        public JToken Skill;
        public double AptitudeBonus;
    }

    public static class CombatantFactory
    {
        public static Combatant FromPlayer(GameDatabase db, Player player)
        {
            PlayerStatsSystem.RecalcStats(db, player);
            var techBonus = TechniqueSystem.GetActiveTechniqueBonus(db, player);
            int extraAtk = TechniqueSystem.GetPhysiqueWeaponAttackBonus(db, player);
            return new Combatant
            {
                Id = "player",
                Name = CombatTexts.PlayerName,
                RealmIndex = player.RealmIndex,
                Hp = player.Hp,
                MaxHp = player.MaxHp + (int)techBonus.Get("hp"),
                Atk = player.Atk + (int)techBonus.Get("atk") + extraAtk,
                Def = player.Def + (int)techBonus.Get("def"),
                Speed = player.Speed + (int)techBonus.Get("speed"),
                MoveSpeed = (int)Math.Max(0, player.MoveSpeed),
                CritRate = player.CritRate + techBonus.Get("critRate"),
                CritDmgMultiplier = player.CritDmgMultiplier + techBonus.Get("critDmgMultiplier"),
                CritResist = player.CritResist,
                PhysiqueDmgReduce = player.PhysiqueDmgReduce,
            };
        }

        public static Combatant FromMonster(MonsterDef monster)
        {
            return new Combatant
            {
                Id = monster.Id,
                Name = monster.Name,
                RealmIndex = monster.RealmIndex ?? 0,
                Hp = monster.Hp ?? 1,
                MaxHp = monster.Hp ?? 1,
                Atk = monster.Atk ?? 1,
                Def = monster.Def ?? 0,
                Speed = monster.Speed ?? 0,
                MoveSpeed = monster.MoveSpeed ?? 0,
                CritRate = monster.CritRate ?? 0,
                CritDmgMultiplier = monster.CritDmgMultiplier ?? 1.5,
                CritResist = monster.CritResist ?? 0,
                ExpReward = monster.ExpReward ?? 0,
                GoldReward = monster.GoldReward ?? 0,
                Element = monster.Element,
            };
        }

        public static Combatant FromTribulationWave(TribulationWave wave)
        {
            return new Combatant
            {
                Id = "tribulation",
                Name = wave.Name,
                Hp = wave.Hp,
                MaxHp = wave.Hp,
                Atk = wave.Atk,
                Def = wave.Def,
                Speed = wave.Speed,
                MoveSpeed = 0,
                CritRate = 0,
                CritDmgMultiplier = 1.5,
            };
        }
    }

    public static class CombatEngine
    {
        public const int MaxRounds = 30;

        public static CombatResult RunCombat(GameDatabase db, Player player, MonsterDef monster, IRng rng)
        {
            return RunCombat(db, player, CombatantFactory.FromMonster(monster), rng, true, null);
        }

        public static CombatResult RunCombat(GameDatabase db, Player player, Combatant monster, IRng rng, bool applyRewards, JToken specialEffect = null)
        {
            var logs = new List<string>();
            var result = new CombatResult { MonsterMaxHp = monster.MaxHp, PlayerMaxMp = player.Mp };
            var buffedPlayer = CombatantFactory.FromPlayer(db, player);
            result.PlayerMaxHp = buffedPlayer.MaxHp;

            var skillInfo = TechniqueSystem.GetActiveSkillInfo(db, player);
            var skillState = new SkillState();
            var divineState = DivineArtSystem.GetState(player);
            DivineArtDef activeArt = !string.IsNullOrEmpty(divineState.ActiveArtId) && db.DivineArts.TryGetValue(divineState.ActiveArtId, out var a) ? a : null;
            var divineSkillState = new SkillState();
            var playerEffects = new List<StatusEffect>();
            var monsterEffects = new List<StatusEffect>();

            int availableMp = player.Mp;
            int availableStamina = player.Stamina;
            int pHp = Math.Min(player.Hp, buffedPlayer.MaxHp);
            int mHp = monster.Hp;
            int monsterBaseDef = monster.Def, monsterBaseAtk = monster.Atk, monsterCurrentDef = monster.Def, monsterCurrentAtk = monster.Atk;

            double tribDefMult = 1, tribAtkMult = 1;
            int tribDot = 0;
            if (specialEffect != null)
            {
                string type = (string)specialEffect["type"];
                string desc = (string)specialEffect["description"];
                if (!string.IsNullOrEmpty(desc)) logs.Add(CombatTexts.WaveEffect(desc));
                if (type == "dot") tribDot = (int?)specialEffect["value"] ?? 0;
                else if (type == "debuff_def") tribDefMult = 1 - ((double?)specialEffect["value"] ?? 0) / 100.0;
                else if (type == "debuff_atk") tribAtkMult = 1 - ((double?)specialEffect["value"] ?? 0) / 100.0;
            }
            buffedPlayer.Atk = (int)Math.Floor(buffedPlayer.Atk * tribAtkMult);
            buffedPlayer.Def = (int)Math.Floor(buffedPlayer.Def * tribDefMult);

            result.Snapshots.Add(new RoundSnapshot { Round = 0, PlayerHp = pHp, PlayerMp = availableMp, MonsterHp = mHp });
            logs.Add(CombatTexts.Encounter(monster.Name, mHp));

            bool playerFirst = buffedPlayer.Speed > monster.Speed || (buffedPlayer.Speed == monster.Speed && rng.NextDouble() > 0.5);
            logs.Add(playerFirst ? CombatTexts.PlayerFirst : CombatTexts.MonsterFirst(monster.Name));

            int round = 0;
            while (pHp > 0 && mHp > 0 && round < MaxRounds)
            {
                round++;
                logs.Add(CombatTexts.RoundHeader(round));
                var order = playerFirst ? new[] { true, false } : new[] { false, true };
                foreach (bool isPlayer in order)
                {
                    if (pHp <= 0 || mHp <= 0) break;
                    var currentMonster = Clone(monster);
                    currentMonster.Hp = mHp;
                    currentMonster.Def = monsterCurrentDef;
                    currentMonster.Atk = monsterCurrentAtk;

                    if (isPlayer)
                    {
                        if (activeArt != null && DivineArtSystem.TryUse(activeArt, divineSkillState, availableMp, rng))
                        {
                            availableMp -= activeArt.MpCost ?? 0;
                            divineSkillState.TotalMpUsed += activeArt.MpCost ?? 0;
                            divineSkillState.UseCount++;
                            divineSkillState.CooldownLeft = activeArt.Cooldown ?? 0;
                            var artDmg = DivineArtSystem.CalcDamage(player, buffedPlayer, activeArt, currentMonster, mHp, rng);
                            mHp -= artDmg.Damage;
                            AddHitSnapshot(result, activeArt.Name, monster.Name, artDmg, true);
                            logs.Add(CombatTexts.ArtUse(activeArt.Name, activeArt.HitCount ?? 1, artDmg.Damage, activeArt.MpCost ?? 0));
                            logs.AddRange(artDmg.Logs);
                            ApplyEffects(activeArt.Effects, activeArt.Name, monster.Name, buffedPlayer.MaxHp, ref pHp, ref monsterCurrentDef, ref monsterCurrentAtk, monsterEffects, playerEffects, logs, mHp);
                        }
                        if (mHp <= 0) break;

                        bool usedSkill = false;
                        if (skillInfo?.Skill != null && TryUseSkill(skillInfo.Skill, skillState, availableMp, availableStamina, rng))
                        {
                            int mpCost = TokenInt(skillInfo.Skill, "mpCost"), staminaCost = TokenInt(skillInfo.Skill, "staminaCost");
                            availableMp -= mpCost;
                            availableStamina -= staminaCost;
                            skillState.TotalMpUsed += mpCost;
                            skillState.TotalStaminaUsed += staminaCost;
                            skillState.UseCount++;
                            skillState.CooldownLeft = TokenInt(skillInfo.Skill, "cooldown");
                            var skillDmg = CalcSkillDamage(buffedPlayer, currentMonster, skillInfo.Skill, skillInfo.AptitudeBonus, mHp, rng);
                            mHp -= skillDmg.Damage;
                            AddHitSnapshot(result, (string)skillInfo.Skill["name"] ?? skillInfo.Def.Name, monster.Name, skillDmg, true);
                            logs.Add(CombatTexts.SkillUse(skillInfo.Def.Name, (string)skillInfo.Skill["name"], TokenInt(skillInfo.Skill, "hitCount", 1), skillDmg.Damage, mpCost));
                            logs.AddRange(skillDmg.Logs);
                            ApplyEffects(skillInfo.Skill["effect"] == null ? null : new List<JToken> { skillInfo.Skill["effect"] }, (string)skillInfo.Skill["name"], monster.Name, buffedPlayer.MaxHp, ref pHp, ref monsterCurrentDef, ref monsterCurrentAtk, monsterEffects, playerEffects, logs, mHp);
                            usedSkill = true;
                        }
                        if (!usedSkill)
                        {
                            var dmg = CalcDamage(buffedPlayer, currentMonster, rng);
                            mHp -= dmg.Damage;
                            AddHitSnapshot(result, buffedPlayer.Name, monster.Name, dmg, true);
                            logs.AddRange(dmg.Logs);
                        }
                    }
                    else
                    {
                        var attackingMonster = Clone(monster);
                        attackingMonster.Hp = mHp;
                        attackingMonster.Def = monsterCurrentDef;
                        attackingMonster.Atk = monsterCurrentAtk;
                        var dmg = CalcDamage(attackingMonster, buffedPlayer, rng);
                        int actual = dmg.Damage;
                        AddHitSnapshot(result, attackingMonster.Name, buffedPlayer.Name, dmg, false);
                        logs.AddRange(dmg.Logs);
                        if (actual > 0)
                        {
                            foreach (var eff in playerEffects.Where(x => x.Type == "shield_self"))
                            {
                                int shield = Math.Min(eff.Value, actual);
                                if (shield > 0)
                                {
                                    actual -= shield;
                                    logs.Add(CombatTexts.ShieldBlock(eff.SourceName, shield));
                                }
                            }
                        }
                        pHp -= actual;
                    }
                }

                TickMonsterEffects(monsterEffects, monster.Name, monsterBaseDef, monsterBaseAtk, ref monsterCurrentDef, ref monsterCurrentAtk, ref mHp, logs);
                TickPlayerEffects(playerEffects, logs);
                if (tribDot > 0 && pHp > 0) pHp -= tribDot;
                if (skillState.CooldownLeft > 0) skillState.CooldownLeft--;
                if (divineSkillState.CooldownLeft > 0) divineSkillState.CooldownLeft--;
                result.Snapshots.Add(new RoundSnapshot { Round = round, PlayerHp = Math.Max(0, pHp), PlayerMp = availableMp, MonsterHp = Math.Max(0, mHp) });
            }

            bool draw = round >= MaxRounds && pHp > 0 && mHp > 0;
            bool playerWon = pHp > 0 && !draw;
            int damageTaken = Math.Max(0, buffedPlayer.Hp - pHp);
            double damageRatio = damageTaken / Math.Max(1.0, buffedPlayer.MaxHp);
            int bodyExp = 5 + (int)Math.Floor(damageRatio * 30) + monster.RealmIndex * 5;
            if (TechniqueSystem.EquippedWeaponHasTechType(db, player, "fist", "finger")) bodyExp *= 2;

            if (draw)
            {
                logs.Add(CombatTexts.Timeout);
                result.Winner = "draw";
            }
            else if (playerWon)
            {
                logs.Add(CombatTexts.Victory(monster.Name));
                logs.Add(CombatTexts.Rewards(monster.ExpReward, monster.GoldReward));
                result.Winner = "player";
            }
            else
            {
                logs.Add(CombatTexts.Defeat(monster.Name));
                result.Winner = "monster";
            }
            if (bodyExp > 0) logs.Add(CombatTexts.BodyExp(bodyExp));

            result.PlayerHpLeft = Math.Max(0, pHp);
            result.ExpGained = playerWon ? monster.ExpReward : 0;
            result.GoldGained = playerWon ? monster.GoldReward : 0;
            result.MpUsed = skillState.TotalMpUsed + divineSkillState.TotalMpUsed;
            result.SkillUseCount = skillState.UseCount;
            result.BodyExpGained = bodyExp;
            result.Logs.AddRange(logs);

            if (applyRewards)
            {
                player.Hp = result.PlayerHpLeft;
                player.Mp = Math.Max(0, player.Mp - result.MpUsed);
                player.Stamina = Math.Max(0, player.Stamina - skillState.TotalStaminaUsed);
                if (playerWon)
                {
                    player.Exp += result.ExpGained;
                    player.Gold += result.GoldGained;
                    player.Tracking.KillCount++;
                    if (monster.RealmIndex > player.RealmIndex) player.Tracking.DefeatedHigherRealm = true;
                    BodyCultivationSystem.GainBodyRealmExp(db, player, bodyExp);
                }
            }
            return result;
        }

        public static DamageResult CalcDamage(Combatant attacker, Combatant defender, IRng rng)
        {
            double baseDmg = attacker.Atk * Rand(rng, 0.85, 1.15);
            double dmg = Math.Max(1, baseDmg - defender.Def * 0.6);
            bool isCrit = false;
            if (rng.NextDouble() * 100 < Math.Max(0, attacker.CritRate - defender.CritResist))
            {
                dmg *= attacker.CritDmgMultiplier <= 0 ? 1.5 : attacker.CritDmgMultiplier;
                isCrit = true;
            }
            bool isDodge = false;
            double dodgeChance = defender.MoveSpeed / (defender.MoveSpeed + 100.0);
            if (rng.NextDouble() < dodgeChance)
            {
                dmg = 0;
                isDodge = true;
            }
            if (!isDodge && defender.PhysiqueDmgReduce > 0)
                dmg = Math.Floor(dmg * (1 - Math.Min(0.50, defender.PhysiqueDmgReduce / 100.0)));
            int finalDmg = (int)Math.Floor(dmg);
            var result = new DamageResult { Damage = finalDmg, IsCrit = isCrit, IsDodge = isDodge };
            result.Logs.Add(isDodge ? CombatTexts.Dodge(defender.Name) : isCrit ? CombatTexts.Crit(attacker.Name, defender.Name, finalDmg) : CombatTexts.NormalHit(attacker.Name, defender.Name, finalDmg));
            return result;
        }

        private static DamageResult CalcSkillDamage(Combatant attacker, Combatant defender, JToken skill, double aptitudeBonus, int defenderCurrentHp, IRng rng)
        {
            var result = new DamageResult();
            int hitCount = TokenInt(skill, "hitCount", 1);
            for (int hit = 0; hit < hitCount; hit++)
            {
                if (defenderCurrentHp - result.Damage <= 0) break;
                double baseDmg = attacker.Atk * Rand(rng, 0.85, 1.15) * TokenDouble(skill, "dmgMultiplier", 1) * aptitudeBonus;
                double dmg = Math.Max(1, baseDmg - defender.Def * 0.6);
                bool crit = false, dodge = false;
                if (rng.NextDouble() * 100 < Math.Max(0, attacker.CritRate - defender.CritResist)) { dmg *= attacker.CritDmgMultiplier <= 0 ? 1.5 : attacker.CritDmgMultiplier; crit = true; }
                if (rng.NextDouble() < defender.MoveSpeed / (defender.MoveSpeed + 100.0)) { dmg = 0; dodge = true; }
                if (!dodge && defender.PhysiqueDmgReduce > 0) dmg = Math.Floor(dmg * (1 - Math.Min(0.50, defender.PhysiqueDmgReduce / 100.0)));
                int final = (int)Math.Floor(dmg);
                result.Damage += final;
                result.IsCrit |= crit;
                result.IsDodge |= dodge;
                if (hitCount > 1) result.Logs.Add(dodge ? CombatTexts.SegmentDodge(hit + 1) : crit ? CombatTexts.SegmentCrit(hit + 1, final) : CombatTexts.SegmentHit(hit + 1, final));
            }
            return result;
        }

        private static void AddHitSnapshot(CombatResult result, string sourceName, string targetName, DamageResult damage, bool fromPlayer)
        {
            if (result == null || damage == null) return;
            result.HitSnapshots.Add(new CombatHitSnapshot
            {
                SourceName = sourceName,
                TargetName = targetName,
                Damage = damage.Damage,
                IsCrit = damage.IsCrit,
                IsDodge = damage.IsDodge,
                FromPlayer = fromPlayer,
            });
        }

        private static bool TryUseSkill(JToken skill, SkillState state, int mp, int stamina, IRng rng)
        {
            return state.CooldownLeft <= 0 && mp >= TokenInt(skill, "mpCost") && stamina >= TokenInt(skill, "staminaCost") && rng.NextDouble() < TokenDouble(skill, "triggerRate");
        }

        private static void ApplyEffects(IList<JToken> effects, string sourceName, string monsterName, int playerMaxHp, ref int pHp, ref int monsterDef, ref int monsterAtk, List<StatusEffect> monsterEffects, List<StatusEffect> playerEffects, List<string> logs, int monsterHp)
        {
            if (effects == null) return;
            foreach (var eff in effects)
            {
                string type = (string)eff["type"];
                int value = TokenInt(eff, "value");
                int duration = TokenInt(eff, "duration", 1);
                if (monsterHp <= 0 && type != "shield_self" && type != "heal_self") continue;
                if (type == "heal_self")
                {
                    int heal = Math.Min(value, playerMaxHp - pHp);
                    pHp += heal;
                    if (heal > 0) logs.Add(CombatTexts.Heal(sourceName, heal));
                }
                else if (type == "shield_self")
                {
                    playerEffects.Add(new StatusEffect { Type = type, Value = value, RemainingRounds = duration, SourceName = sourceName });
                    logs.Add(CombatTexts.Shield(sourceName, value, duration));
                }
                else if (type == "debuff_def")
                {
                    monsterEffects.Add(new StatusEffect { Type = type, Value = value, RemainingRounds = duration, SourceName = sourceName });
                    monsterDef = Math.Max(0, monsterDef - value);
                    logs.Add(CombatTexts.DebuffDef(monsterName, value, duration));
                }
                else if (type == "debuff_atk")
                {
                    monsterEffects.Add(new StatusEffect { Type = type, Value = value, RemainingRounds = duration, SourceName = sourceName });
                    monsterAtk = Math.Max(0, monsterAtk - value);
                    logs.Add(CombatTexts.DebuffAtk(monsterName, value, duration));
                }
                else if (type == "dot")
                {
                    monsterEffects.Add(new StatusEffect { Type = type, Value = value, RemainingRounds = duration, SourceName = sourceName });
                    logs.Add(CombatTexts.DotApply(monsterName, value, duration));
                }
            }
        }

        private static void TickMonsterEffects(List<StatusEffect> effects, string name, int baseDef, int baseAtk, ref int currentDef, ref int currentAtk, ref int hp, List<string> logs)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var eff = effects[i];
                if (eff.Type == "dot" && hp > 0) { hp -= eff.Value; logs.Add(CombatTexts.DotTick(name, eff.Value)); }
                eff.RemainingRounds--;
                if (eff.RemainingRounds <= 0)
                {
                    if (eff.Type == "debuff_def") currentDef = Math.Min(baseDef, currentDef + eff.Value);
                    else if (eff.Type == "debuff_atk") currentAtk = Math.Min(baseAtk, currentAtk + eff.Value);
                    effects.RemoveAt(i);
                }
            }
        }

        private static void TickPlayerEffects(List<StatusEffect> effects, List<string> logs)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].RemainingRounds--;
                if (effects[i].RemainingRounds <= 0)
                {
                    logs.Add(CombatTexts.ShieldExpire(effects[i].SourceName));
                    effects.RemoveAt(i);
                }
            }
        }

        internal static double Rand(IRng rng, double min, double max) => min + rng.NextDouble() * (max - min);
        internal static int TokenInt(JToken token, string name, int fallback = 0) => (int?)token?[name] ?? fallback;
        internal static double TokenDouble(JToken token, string name, double fallback = 0) => (double?)token?[name] ?? fallback;
        private static Combatant Clone(Combatant c) => new Combatant
        {
            Id = c.Id, Name = c.Name, Element = c.Element, RealmIndex = c.RealmIndex, Hp = c.Hp, MaxHp = c.MaxHp, Atk = c.Atk, Def = c.Def, Speed = c.Speed,
            MoveSpeed = c.MoveSpeed, ExpReward = c.ExpReward, GoldReward = c.GoldReward, CritRate = c.CritRate, CritDmgMultiplier = c.CritDmgMultiplier,
            CritResist = c.CritResist, PhysiqueDmgReduce = c.PhysiqueDmgReduce, ElementResists = new Dictionary<string, double>(c.ElementResists),
        };
    }

    public static class CombatDictionaryExtensions
    {
        public static double Get(this Dictionary<string, double> dict, string key) => dict != null && dict.TryGetValue(key, out var v) ? v : 0;
    }
}
