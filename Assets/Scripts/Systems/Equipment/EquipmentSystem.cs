// ============================================================
// EquipmentSystem.cs — equipment operations, aggregation, generation
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;
using Xiuxian.Systems.Procedural;

namespace Xiuxian.Systems
{
    public sealed class EquipStatBonus
    {
        public int Atk, Def, Speed, Hp, Mp, MoveSpeed, Physique;
        public double CritRate, CritDmgMultiplier, CritResist, PhysiqueDmgReduce;
    }

    public sealed class EquipResult
    {
        public Player Player;
        public bool Success;
        public string Message;
    }

    public sealed class GeneratedEquipInstance
    {
        public string InstanceId, BaseTemplateId, Quality, FinalName, Description;
        public readonly List<string> PrefixIds = new();
        public readonly List<string> SuffixIds = new();
        public int Seed, FinalSellPrice;
        public Dictionary<string, double> FinalStats = new();
    }

    public sealed class ProceduralItemState
    {
        public int MasterSeed = 1234567;
        public int EquipCounter;
        public readonly List<GeneratedEquipInstance> GeneratedEquips = new();
    }

    public sealed class GenerateEquipOptions
    {
        public string SlotFilter;
        public string ForcedQuality;
        public int? Seed;
    }

    public static class EquipmentSystem
    {
        private sealed class QualityTier
        {
            public string Quality, Rarity, DisplayName;
            public double StatMultiplier, PriceMultiplier, DropWeight, LuckScaling;
            public int PrefixSlots, SuffixSlots;
        }

        private static readonly QualityTier[] QualityTiers =
        {
            new QualityTier { Quality = "mortal", DisplayName = ProceduralTexts.QualityMortal, Rarity = "common", StatMultiplier = 1.0, PrefixSlots = 0, SuffixSlots = 0, PriceMultiplier = 1.0, DropWeight = 50, LuckScaling = 0 },
            new QualityTier { Quality = "spirit", DisplayName = ProceduralTexts.QualitySpirit, Rarity = "uncommon", StatMultiplier = 1.5, PrefixSlots = 1, SuffixSlots = 0, PriceMultiplier = 2.0, DropWeight = 30, LuckScaling = 0.005 },
            new QualityTier { Quality = "treasure", DisplayName = ProceduralTexts.QualityTreasure, Rarity = "rare", StatMultiplier = 2.5, PrefixSlots = 1, SuffixSlots = 1, PriceMultiplier = 5.0, DropWeight = 12, LuckScaling = 0.01 },
            new QualityTier { Quality = "immortal", DisplayName = ProceduralTexts.QualityImmortal, Rarity = "epic", StatMultiplier = 4.0, PrefixSlots = 2, SuffixSlots = 1, PriceMultiplier = 12.0, DropWeight = 6, LuckScaling = 0.015 },
            new QualityTier { Quality = "ancient", DisplayName = ProceduralTexts.QualityAncient, Rarity = "legendary", StatMultiplier = 6.0, PrefixSlots = 2, SuffixSlots = 2, PriceMultiplier = 25.0, DropWeight = 2, LuckScaling = 0.02 },
        };

        public static EquipResult EquipItem(GameDatabase db, Player player, string equipId)
        {
            var def = GetEquipDef(db, player, equipId);
            if (def == null) return new EquipResult { Player = player, Message = EquipmentTexts.Unknown(equipId) };
            if (player.RealmIndex < (def.MinRealm ?? 0)) return new EquipResult { Player = player, Message = EquipmentTexts.RealmInsufficient(def.Name) };
            if (InventorySystem.CountItem(player, equipId) <= 0) return new EquipResult { Player = player, Message = EquipmentTexts.NoItem(def.Name) };

            string old = GetSlot(player.Equipped, def.Slot);
            if (!string.IsNullOrEmpty(old)) InventorySystem.AddItem(player, old, 1);
            InventorySystem.RemoveItem(player, equipId, 1);
            SetSlot(player.Equipped, def.Slot, equipId);
            PlayerStatsSystem.RecalcStats(db, player);
            return new EquipResult { Player = player, Success = true, Message = EquipmentTexts.Equipped(def.Name) };
        }

        public static EquipResult UnequipItem(GameDatabase db, Player player, string slot)
        {
            string equipId = GetSlot(player.Equipped, slot);
            if (string.IsNullOrEmpty(equipId)) return new EquipResult { Player = player, Message = EquipmentTexts.SlotEmpty };
            var def = GetEquipDef(db, player, equipId);
            InventorySystem.AddItem(player, equipId, 1);
            SetSlot(player.Equipped, slot, null);
            PlayerStatsSystem.RecalcStats(db, player);
            return new EquipResult { Player = player, Success = true, Message = EquipmentTexts.Unequipped(def?.Name ?? equipId) };
        }

        public static EquipStatBonus GetEquipmentStatBonus(GameDatabase db, Player player)
        {
            var bonus = new EquipStatBonus();
            foreach (string equipId in EquippedIds(player.Equipped))
            {
                var def = GetEquipDef(db, player, equipId);
                if (def?.Stats == null) continue;
                AddStats(bonus, def.Stats);
            }
            return bonus;
        }

        public static EquipDef GetEquipDef(GameDatabase db, Player player, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (db.Equips.TryGetValue(id, out var def)) return def;
            var inst = GetProceduralItemState(player).GeneratedEquips.FirstOrDefault(x => x.InstanceId == id);
            if (inst == null || !db.EquipTemplates.TryGetValue(inst.BaseTemplateId, out var template)) return null;
            return new EquipDef
            {
                Id = inst.InstanceId,
                Name = inst.FinalName,
                Description = inst.Description,
                Slot = template.Slot,
                Rarity = GetQualityTier(inst.Quality).Rarity,
                MinRealm = template.MinRealm,
                Stats = inst.FinalStats,
                TechType = template.TechType,
                SellPrice = inst.FinalSellPrice,
            };
        }

        public static ProceduralItemState GetProceduralItemState(Player player)
        {
            if (player.Systems.TryGetValue("proceduralItemState", out var existing))
            {
                if (existing is ProceduralItemState typed) return typed;
                if (existing is JObject jo) return jo.ToObject<ProceduralItemState>() ?? new ProceduralItemState();
            }
            var state = new ProceduralItemState();
            player.Systems["proceduralItemState"] = state;
            return state;
        }

        public static GeneratedEquipInstance GenerateEquip(GameDatabase db, Player player, IRng rng, GenerateEquipOptions options = null)
        {
            return Procedural.EquipGenerator.GenerateEquip(db, player, options);
        }

        internal static string GetSlot(EquippedSlots slots, string slot)
        {
            switch (slot)
            {
                case "weapon": return slots.Weapon;
                case "helmet": return slots.Helmet;
                case "armor": return slots.Armor;
                case "boots": return slots.Boots;
                case "accessory1": return slots.Accessory1;
                case "accessory2": return slots.Accessory2;
                default: return null;
            }
        }

        private static void SetSlot(EquippedSlots slots, string slot, string value)
        {
            switch (slot)
            {
                case "weapon": slots.Weapon = value; break;
                case "helmet": slots.Helmet = value; break;
                case "armor": slots.Armor = value; break;
                case "boots": slots.Boots = value; break;
                case "accessory1": slots.Accessory1 = value; break;
                case "accessory2": slots.Accessory2 = value; break;
            }
        }

        private static IEnumerable<string> EquippedIds(EquippedSlots slots)
        {
            if (!string.IsNullOrEmpty(slots.Weapon)) yield return slots.Weapon;
            if (!string.IsNullOrEmpty(slots.Helmet)) yield return slots.Helmet;
            if (!string.IsNullOrEmpty(slots.Armor)) yield return slots.Armor;
            if (!string.IsNullOrEmpty(slots.Boots)) yield return slots.Boots;
            if (!string.IsNullOrEmpty(slots.Accessory1)) yield return slots.Accessory1;
            if (!string.IsNullOrEmpty(slots.Accessory2)) yield return slots.Accessory2;
        }

        private static void AddStats(EquipStatBonus bonus, Dictionary<string, double> stats)
        {
            foreach (var kv in stats) AddStat(bonus, kv.Key, kv.Value);
        }

        private static void AddStat(EquipStatBonus b, string key, double value)
        {
            switch (key)
            {
                case "atk": b.Atk += (int)value; break;
                case "def": b.Def += (int)value; break;
                case "speed": b.Speed += (int)value; break;
                case "hp": b.Hp += (int)value; break;
                case "mp": b.Mp += (int)value; break;
                case "moveSpeed": b.MoveSpeed += (int)value; break;
                case "physique": b.Physique += (int)value; break;
                case "critRate": b.CritRate += value; break;
                case "critDmgMultiplier": b.CritDmgMultiplier += value; break;
                case "critResist": b.CritResist += value; break;
                case "physiqueDmgReduce": b.PhysiqueDmgReduce += value; break;
            }
        }

        private static QualityTier GetQualityTier(string quality) => QualityTiers.FirstOrDefault(x => x.Quality == quality) ?? QualityTiers[0];
        private static QualityTier RollQuality(IRng rng, int luck)
        {
            double total = QualityTiers.Sum(x => x.DropWeight * (1 + luck * x.LuckScaling));
            double roll = rng.NextDouble() * total;
            foreach (var tier in QualityTiers)
            {
                roll -= tier.DropWeight * (1 + luck * tier.LuckScaling);
                if (roll <= 0) return tier;
            }
            return QualityTiers[0];
        }

        private static List<AffixDef> PickAffixes(GameDatabase db, EquipBaseTemplateDef template, string position, int count, int realm, HashSet<string> selected, IRng rng)
        {
            var result = new List<AffixDef>();
            for (int i = 0; i < count; i++)
            {
                var pool = db.Affixes.Values.Where(a =>
                    a.Position == position &&
                    !selected.Contains(a.Id) &&
                    (a.MinRealm ?? 0) <= realm &&
                    (a.SlotRestriction == null || a.SlotRestriction.Count == 0 || a.SlotRestriction.Contains(template.Slot)) &&
                    (a.ExcludeAffixes == null || !a.ExcludeAffixes.Any(selected.Contains))).ToList();
                if (pool.Count == 0) break;
                int total = pool.Sum(a => a.Weight ?? 1);
                int roll = rng.NextIntInclusive(1, Math.Max(1, total));
                AffixDef picked = pool[0];
                foreach (var a in pool)
                {
                    roll -= a.Weight ?? 1;
                    if (roll <= 0) { picked = a; break; }
                }
                selected.Add(picked.Id);
                result.Add(picked);
            }
            return result;
        }

        private static Dictionary<string, double> ScaleStats(Dictionary<string, double> stats, double multiplier)
        {
            var result = new Dictionary<string, double>();
            foreach (var kv in stats ?? new Dictionary<string, double>()) result[kv.Key] = Math.Floor(kv.Value * multiplier);
            return result;
        }
        private static void MergeStats(Dictionary<string, double> stats, Dictionary<string, double> add)
        {
            foreach (var kv in add ?? new Dictionary<string, double>()) stats[kv.Key] = (stats.TryGetValue(kv.Key, out var v) ? v : 0) + kv.Value;
        }
        private static string BuildName(EquipBaseTemplateDef t, List<AffixDef> prefixes, List<AffixDef> suffixes, QualityTier tier)
        {
            var parts = prefixes.Select(x => x.Name).Concat(new[] { t.BaseName }).Concat(suffixes.Select(x => x.Name));
            var body = string.Join("·", parts);
            return tier.Quality == "mortal" ? body : $"[{tier.DisplayName}] {body}";
        }
        private static string BuildDescription(EquipBaseTemplateDef t, List<AffixDef> prefixes, List<AffixDef> suffixes, QualityTier tier)
        {
            return (t.DescriptionPattern ?? "{baseName}")
                .Replace("{prefix_desc}", string.Join("，", prefixes.Select(x => x.Description)))
                .Replace("{quality_adj}", tier.DisplayName)
                .Replace("{baseName}", t.BaseName)
                .Replace("{suffix_desc}", string.Join("，", suffixes.Select(x => x.Description)));
        }
    }
}
