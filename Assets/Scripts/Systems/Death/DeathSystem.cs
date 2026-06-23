// ============================================================
// DeathSystem.cs — death triggers, penalties, revival
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class DeathSystemState
    {
        public int DeathCount, RevivalCount;
        public string LastDeathCause;
        public readonly List<string> LifeSaverTriggered = new();
        public bool IsLooseImmortal;
    }

    public sealed class DeathContext
    {
        public string Source;
        public bool IsBoss;
    }

    public sealed class DeathTriggerDef
    {
        public string Id, Name, Description, Severity;
        public bool CanBeBlocked, BypassRevival;
        public int Priority;
        public Func<Player, DeathContext, bool> Check;
    }

    public sealed class DeathPenaltyDef
    {
        public string Severity;
        public double ExpLossRate, GoldLossRate;
        public int InventoryLossCount, HealthLoss, MoodLoss, RealmDrop;
        public bool GameOver;
    }

    public sealed class LifeSaverDef
    {
        public string Id, ItemId, Name, Description;
        public int Priority;
        public bool ConsumeOnUse;
        public readonly List<string> BlockSeverities = new();
        public Func<Player, bool> Condition;
        public Action<GameDatabase, Player> AfterEffect;
    }

    public sealed class RevivalMethodDef
    {
        public string Id, Name, Description, Type, ItemId, PassiveId;
        public bool ConsumeOnUse;
        public int Priority;
        public Func<Player, bool> Condition;
        public Action<GameDatabase, Player> Effect;
        public DeathPenaltyDef Penalty;
    }

    public sealed class DeathCheckResult
    {
        public bool Triggered, Blocked;
        public DeathTriggerDef Trigger;
        public LifeSaverDef BlockedBy;
        public Player Player;
        public readonly List<string> Logs = new();
    }

    public sealed class DeathResult
    {
        public Player Player;
        public bool GameOver;
        public readonly List<RevivalMethodDef> AvailableRevivals = new();
        public readonly List<string> Logs = new();
        public string Severity, GameOverReason;
    }

    public sealed class RevivalResult
    {
        public Player Player;
        public readonly List<string> Logs = new();
    }

    public static class DeathSystem
    {
        public static readonly Dictionary<string, DeathPenaltyDef> DefaultPenalties = new()
        {
            { "light", new DeathPenaltyDef { Severity = "light", ExpLossRate = 0.1, GoldLossRate = 0.1, InventoryLossCount = 0, HealthLoss = 20, MoodLoss = 15 } },
            { "moderate", new DeathPenaltyDef { Severity = "moderate", ExpLossRate = 0.3, GoldLossRate = 0.3, InventoryLossCount = 2, HealthLoss = 40, MoodLoss = 30, RealmDrop = 1 } },
            { "severe", new DeathPenaltyDef { Severity = "severe", ExpLossRate = 1.0, GoldLossRate = 1.0, GameOver = true } },
        };

        public static DeathSystemState GetState(Player player)
        {
            if (player.Systems.TryGetValue("death", out var existing) && existing is DeathSystemState typed) return typed;
            var state = new DeathSystemState();
            player.Systems["death"] = state;
            return state;
        }

        public static List<DeathTriggerDef> GetDefaultTriggers()
        {
            return new List<DeathTriggerDef>
            {
                new DeathTriggerDef { Id = "core:death_lifespan", Name = "寿元耗尽", Description = "大限将至，油尽灯枯…", Severity = "severe", CanBeBlocked = false, BypassRevival = true, Priority = 0, Check = (p, c) => p.Lifespan > 0 && p.Age >= p.Lifespan },
                new DeathTriggerDef { Id = "core:death_tribulation", Name = "渡劫失败", Description = "天劫降临，形神俱灭…", Severity = "severe", CanBeBlocked = false, BypassRevival = false, Priority = 0, Check = (p, c) => c.Source == "tribulation" && p.Hp <= 0 },
                new DeathTriggerDef { Id = "core:death_combat", Name = "战斗阵亡", Description = "在战斗中落败，身受重伤…", Severity = "light", CanBeBlocked = true, BypassRevival = false, Priority = 10, Check = (p, c) => c.Source == "combat" && !c.IsBoss && p.Hp <= 0 },
                new DeathTriggerDef { Id = "core:death_combat_boss", Name = "强敌阵亡", Description = "败于强敌之手，道基重创…", Severity = "moderate", CanBeBlocked = true, BypassRevival = false, Priority = 9, Check = (p, c) => c.Source == "combat" && c.IsBoss && p.Hp <= 0 },
                new DeathTriggerDef { Id = "core:death_health", Name = "气血耗尽", Description = "气血枯竭，再难支撑…", Severity = "moderate", CanBeBlocked = true, BypassRevival = false, Priority = 15, Check = (p, c) => p.Health <= 0 },
                new DeathTriggerDef { Id = "core:death_inner_demon", Name = "心魔入体", Description = "心魔趁虚而入，神识受损…", Severity = "moderate", CanBeBlocked = true, BypassRevival = false, Priority = 20, Check = (p, c) => p.Mood <= 10 && p.Tracking.LowMoodStreak >= 5 && p.Tracking.ConsecutiveBreakthroughFails >= 3 },
            };
        }

        public static List<LifeSaverDef> GetDefaultLifeSavers()
        {
            return new List<LifeSaverDef>
            {
                new LifeSaverDef { Id = "core:saver_jade", ItemId = "cp-01:jade_shield", Name = "护身玉佩", Description = "玉佩碎裂护主，化危为安", Priority = 5, ConsumeOnUse = true, BlockSeverities = { "light", "moderate" }, AfterEffect = (db, p) => p.Hp = Math.Max(1, p.Hp) },
                new LifeSaverDef { Id = "core:saver_talisman", ItemId = "cp-01:life_talisman", Name = "护身灵符", Description = "蕴含灵力的护身符碎裂，抵消了致命伤害", Priority = 10, ConsumeOnUse = true, BlockSeverities = { "light", "moderate" }, AfterEffect = (db, p) => p.Hp = Math.Max(1, (int)Math.Floor(p.MaxHp * 0.3)) },
            };
        }

        public static List<RevivalMethodDef> GetDefaultRevivalMethods()
        {
            return new List<RevivalMethodDef>
            {
                new RevivalMethodDef { Id = "core:revival_nine_turn_pill", Name = "九转回魂丹", Description = "传说仙丹之力，令你起死回生", Type = "item", ItemId = "cp-01:nine_turn_pill", ConsumeOnUse = true, Priority = 0, Effect = (db, p) => { p.Hp = p.MaxHp; p.Mp = p.MaxMp; }, Penalty = new DeathPenaltyDef { Severity = "severe", ExpLossRate = 0.2, RealmDrop = 1 } },
                new RevivalMethodDef { Id = "core:revival_loose_immortal", Name = "散仙化", Description = "肉身虽毁，散仙之躯重凝", Type = "passive", ConsumeOnUse = false, Priority = 10, Condition = p => p.RealmIndex >= 5 && !GetState(p).IsLooseImmortal, Effect = (db, p) => { p.Hp = (int)Math.Floor(p.MaxHp * 0.8); p.Mp = (int)Math.Floor(p.MaxMp * 0.8); p.Health = 80; p.Mood = 50; } },
                new RevivalMethodDef { Id = "core:revival_reincarnation", Name = "转世重修", Description = "以残存道韵转世，保留一缕修为", Type = "passive", ConsumeOnUse = false, Priority = 20, Condition = p => p.RealmIndex >= 3 && GetState(p).DeathCount <= 3, Effect = (db, p) => { int exp = (int)Math.Floor(p.Exp * 0.1); p.RealmIndex = 0; p.Exp = exp; p.Gold = 0; p.Inventory.Clear(); p.Equipped = new EquippedSlots(); PlayerStatsSystem.RecalcStats(db, p); p.Hp = p.MaxHp; p.Mp = p.MaxMp; p.Health = 100; p.Mood = 70; } },
            };
        }

        public static DeathCheckResult CheckDeathTriggers(GameDatabase db, Player player, DeathContext context)
        {
            foreach (var trigger in GetDefaultTriggers().OrderBy(x => x.Priority))
            {
                if (!trigger.Check(player, context)) continue;
                if (trigger.CanBeBlocked)
                {
                    var saver = CheckLifeSavers(db, player, trigger.Severity);
                    if (saver != null)
                    {
                        var state = GetState(player);
                        state.LifeSaverTriggered.Add(saver.Id);
                        return new DeathCheckResult { Triggered = true, Trigger = trigger, Blocked = true, BlockedBy = saver, Player = player, Logs = { DeathTexts.LifeSaverBlock(saver.Name) } };
                    }
                }
                return new DeathCheckResult { Triggered = true, Trigger = trigger, Player = player };
            }
            return new DeathCheckResult { Player = player };
        }

        public static DeathResult ApplyDeath(GameDatabase db, Player player, DeathTriggerDef trigger, IRng rng)
        {
            var result = new DeathResult { Player = player, Severity = trigger.Severity };
            var state = GetState(player);
            state.DeathCount++;
            state.LastDeathCause = trigger.Id;
            if (trigger.Severity != "severe")
            {
                ApplyPenalty(db, player, DefaultPenalties[trigger.Severity], result.Logs, rng);
                return result;
            }
            result.Logs.Add(DeathTexts.Death(trigger.Name, trigger.Description));
            if (trigger.BypassRevival)
            {
                result.GameOver = true;
                result.GameOverReason = $"{trigger.Name}，{trigger.Description}";
                return result;
            }
            result.AvailableRevivals.AddRange(CheckRevivalMethods(player));
            result.GameOver = result.AvailableRevivals.Count == 0;
            result.GameOverReason = result.GameOver ? $"{trigger.Name}！{trigger.Description}" : string.Empty;
            return result;
        }

        public static RevivalResult ApplyRevival(GameDatabase db, Player player, RevivalMethodDef method, IRng rng)
        {
            var result = new RevivalResult { Player = player };
            method.Effect?.Invoke(db, player);
            result.Logs.Add(DeathTexts.Revival(method.Name, method.Description));
            if (method.ConsumeOnUse && method.Type == "item" && !string.IsNullOrEmpty(method.ItemId))
            {
                InventorySystem.RemoveItem(player, method.ItemId, 1);
                result.Logs.Add(DeathTexts.ConsumeRevival(method.Name));
            }
            if (method.Penalty != null) ApplyPenalty(db, player, method.Penalty, result.Logs, rng);
            var state = GetState(player);
            state.RevivalCount++;
            if (method.Id == "core:revival_loose_immortal") state.IsLooseImmortal = true;
            return result;
        }

        public static void ApplyPenalty(GameDatabase db, Player player, DeathPenaltyDef penalty, List<string> logs, IRng rng)
        {
            int expLoss = (int)Math.Floor(player.Exp * penalty.ExpLossRate);
            if (expLoss > 0) { player.Exp = Math.Max(0, player.Exp - expLoss); logs.Add(DeathTexts.ExpLoss(expLoss)); }
            int goldLoss = (int)Math.Floor(player.Gold * penalty.GoldLossRate);
            if (goldLoss > 0) { player.Gold = Math.Max(0, player.Gold - goldLoss); logs.Add(DeathTexts.GoldLoss(goldLoss)); }
            int lost = 0;
            for (int i = 0; i < penalty.InventoryLossCount && player.Inventory.Count > 0; i++)
            {
                int idx = rng.NextIntInclusive(0, player.Inventory.Count - 1);
                player.Inventory.RemoveAt(idx);
                lost++;
            }
            if (lost > 0) logs.Add(DeathTexts.ItemLoss(lost));
            if (penalty.HealthLoss > 0) { player.Health = Math.Max(0, player.Health - penalty.HealthLoss); logs.Add(DeathTexts.HealthLoss(penalty.HealthLoss)); }
            if (penalty.MoodLoss > 0) { player.Mood = Math.Max(0, player.Mood - penalty.MoodLoss); logs.Add(DeathTexts.MoodLoss(penalty.MoodLoss)); }
            if (penalty.RealmDrop > 0 && player.RealmIndex > 0)
            {
                int drop = Math.Min(penalty.RealmDrop, player.RealmIndex);
                player.RealmIndex -= drop;
                PlayerStatsSystem.RecalcStats(db, player);
                player.Hp = Math.Max(1, Math.Min(player.Hp, player.MaxHp));
                player.Mp = Math.Min(player.Mp, player.MaxMp);
                logs.Add(DeathTexts.RealmDrop(drop));
            }
        }

        private static LifeSaverDef CheckLifeSavers(GameDatabase db, Player player, string severity)
        {
            foreach (var saver in GetDefaultLifeSavers().Where(x => x.BlockSeverities.Contains(severity)).OrderBy(x => x.Priority))
            {
                if (InventorySystem.CountItem(player, saver.ItemId) <= 0) continue;
                if (saver.Condition != null && !saver.Condition(player)) continue;
                if (saver.ConsumeOnUse) InventorySystem.RemoveItem(player, saver.ItemId, 1);
                saver.AfterEffect?.Invoke(db, player);
                return saver;
            }
            return null;
        }

        private static IEnumerable<RevivalMethodDef> CheckRevivalMethods(Player player)
        {
            foreach (var method in GetDefaultRevivalMethods().OrderBy(x => x.Priority))
            {
                if (method.Type == "item" && !string.IsNullOrEmpty(method.ItemId) && InventorySystem.CountItem(player, method.ItemId) <= 0) continue;
                if (method.Type == "passive" && !string.IsNullOrEmpty(method.PassiveId) && !player.Passives.ContainsKey(method.PassiveId)) continue;
                if (method.Condition != null && !method.Condition(player)) continue;
                yield return method;
            }
        }
    }
}
