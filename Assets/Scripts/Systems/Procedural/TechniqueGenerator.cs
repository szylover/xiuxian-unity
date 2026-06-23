using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Systems.Procedural
{
    public sealed class TechniqueTraitSlot { public string TraitId, Tier, Stat; public double FinalValue; }
    public sealed class GeneratedTechniqueInstance { public string InstanceId, BaseTechniqueId, QualityOverride; public int Seed; public readonly List<TechniqueTraitSlot> Traits = new(); }
    public sealed class ProceduralTechniqueState { public int MasterSeed = 1234567; public int TechniqueCounter; public readonly List<GeneratedTechniqueInstance> Instances = new(); }
    public sealed class GenerateTechniqueOptions { public string ForcedQuality; public int? Seed; }
    public sealed class TechniqueQualityConfig { public string Rarity, DisplayName; public int Minor, Major, Legendary; public double ValueMultiplier, DropWeight; }

    public static class TechniqueGenerator
    {
        private static readonly double[] LuckScaling = { 0, 0.005, 0.01, 0.015, 0.02 };
        public static readonly TechniqueQualityConfig[] QualityConfigs =
        {
            new TechniqueQualityConfig { Rarity = "common", DisplayName = ProceduralTexts.TechniqueCommon, Minor = 1, Major = 0, Legendary = 0, ValueMultiplier = 1.0, DropWeight = 40 },
            new TechniqueQualityConfig { Rarity = "uncommon", DisplayName = ProceduralTexts.TechniqueUncommon, Minor = 2, Major = 1, Legendary = 0, ValueMultiplier = 1.3, DropWeight = 30 },
            new TechniqueQualityConfig { Rarity = "rare", DisplayName = ProceduralTexts.TechniqueRare, Minor = 2, Major = 1, Legendary = 1, ValueMultiplier = 1.8, DropWeight = 18 },
            new TechniqueQualityConfig { Rarity = "epic", DisplayName = ProceduralTexts.TechniqueEpic, Minor = 3, Major = 2, Legendary = 1, ValueMultiplier = 2.5, DropWeight = 9 },
            new TechniqueQualityConfig { Rarity = "legendary", DisplayName = ProceduralTexts.TechniqueLegendary, Minor = 3, Major = 2, Legendary = 2, ValueMultiplier = 4.0, DropWeight = 3 },
        };
        public static ProceduralTechniqueState GetState(Player player)
        {
            if (player.Systems.TryGetValue("proceduralTechniqueState", out var existing))
            {
                if (existing is ProceduralTechniqueState s) return s;
                if (existing is JObject jo) return jo.ToObject<ProceduralTechniqueState>() ?? new ProceduralTechniqueState();
            }
            var state = new ProceduralTechniqueState(); player.Systems["proceduralTechniqueState"] = state; return state;
        }
        public static GeneratedTechniqueInstance GenerateTechniqueInstance(GameDatabase db, Player player, string baseTechniqueId, string techniqueType, GenerateTechniqueOptions options = null)
        {
            options ??= new GenerateTechniqueOptions(); var state = GetState(player); int seed = options.Seed ?? ProceduralSeed.HashSeed(state.MasterSeed, state.TechniqueCounter); var rng = ProceduralSeed.CreateRng(seed);
            var cfg = string.IsNullOrEmpty(options.ForcedQuality) ? RollQuality(rng, player.Luck) : QualityConfigs.FirstOrDefault(x => x.Rarity == options.ForcedQuality) ?? QualityConfigs[0];
            var selected = new HashSet<string>(); var inst = new GeneratedTechniqueInstance { InstanceId = $"proc-tech:{((uint)seed).ToString("x")}", BaseTechniqueId = baseTechniqueId, QualityOverride = cfg.Rarity, Seed = seed };
            foreach (var tier in new[] { "minor", "major", "legendary" })
            {
                int count = tier == "minor" ? cfg.Minor : tier == "major" ? cfg.Major : cfg.Legendary;
                for (int i = 0; i < count; i++)
                {
                    var pool = db.TechniqueTraits.Values.Where(t => t.Tier == tier && !selected.Contains(t.Id) && (t.MinRealm == null || player.RealmIndex >= t.MinRealm.Value) && (t.MaxRealm == null || player.RealmIndex <= t.MaxRealm.Value) && TypeAllowed(t.TypeRestriction, techniqueType) && (t.ExcludeTraits == null || !t.ExcludeTraits.Any(selected.Contains))).ToList();
                    if (pool.Count == 0) break;
                    var picked = ProceduralSeed.WeightedPick(pool, t => t.Weight ?? 1, rng); selected.Add(picked.Id);
                    double raw = (picked.BaseValue ?? 0) * cfg.ValueMultiplier * (picked.QualityScaling ?? 1) * (0.8 + rng.NextDouble() * 0.4);
                    double final = picked.Stat == "critDmgMultiplier" ? Math.Round(raw, 2) : Math.Floor(raw);
                    inst.Traits.Add(new TechniqueTraitSlot { TraitId = picked.Id, Tier = tier, FinalValue = final, Stat = picked.Stat });
                }
            }
            state.TechniqueCounter++; state.Instances.Add(inst); return inst;
        }
        public static Dictionary<string, double> GetTraitBonus(Player player, string instanceId)
        {
            var bonus = new Dictionary<string, double>(); if (string.IsNullOrEmpty(instanceId)) return bonus;
            var inst = GetState(player).Instances.FirstOrDefault(x => x.InstanceId == instanceId); if (inst == null) return bonus;
            foreach (var slot in inst.Traits) bonus[slot.Stat] = bonus.TryGetValue(slot.Stat, out var v) ? Math.Round(v + slot.FinalValue, slot.Stat == "critDmgMultiplier" ? 2 : 0) : slot.FinalValue;
            return bonus;
        }
        private static TechniqueQualityConfig RollQuality(IRng rng, int luck)
        {
            var weighted = QualityConfigs.Select((c, i) => (Cfg: c, Weight: c.DropWeight * (1 + luck * LuckScaling[i]))).ToList();
            return ProceduralSeed.WeightedPick(weighted, x => x.Weight, rng).Cfg;
        }
        private static bool TypeAllowed(JToken restriction, string techniqueType)
        {
            if (restriction == null) return true;
            if (restriction is JArray arr) return arr.Values<string>().Contains(techniqueType);
            return restriction.ToString() == techniqueType;
        }
    }
}
