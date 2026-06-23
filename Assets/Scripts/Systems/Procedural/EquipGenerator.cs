using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Systems.Procedural
{
    public static class EquipGenerator
    {
        public static GeneratedEquipInstance GenerateEquip(GameDatabase db, Player player, GenerateEquipOptions options = null)
        {
            options ??= new GenerateEquipOptions();
            var state = EquipmentSystem.GetProceduralItemState(player);
            int subSeed = options.Seed ?? ProceduralSeed.HashSeed(state.MasterSeed, state.EquipCounter);
            var rng = ProceduralSeed.CreateRng(subSeed);
            var tier = string.IsNullOrEmpty(options.ForcedQuality) ? ProceduralRarity.RollRarity(rng, player.Luck) : ProceduralRarity.GetQualityTier(options.ForcedQuality);
            var templates = db.EquipTemplates.Values.Where(t => (t.MinRealm ?? 0) <= player.RealmIndex && (string.IsNullOrEmpty(options.SlotFilter) || t.Slot == options.SlotFilter)).ToList();
            if (templates.Count == 0) return null;
            var template = templates[Math.Min(templates.Count - 1, (int)Math.Floor(rng.NextDouble() * templates.Count))];
            var selected = new HashSet<string>();
            var prefixes = PickAffixes(db, template, "prefix", tier.PrefixSlots, player.RealmIndex, selected, rng);
            var suffixes = PickAffixes(db, template, "suffix", tier.SuffixSlots, player.RealmIndex, selected, rng);
            var stats = ScaleStats(template.BaseStats, tier.StatMultiplier);
            foreach (var affix in prefixes.Concat(suffixes)) MergeStats(stats, affix.StatBonus);
            var inst = new GeneratedEquipInstance
            {
                InstanceId = $"proc-equip:{((uint)subSeed).ToString("x")}",
                BaseTemplateId = template.Id,
                Quality = tier.Quality,
                Seed = subSeed,
                FinalName = BuildName(template, prefixes, suffixes, tier),
                FinalStats = stats,
                FinalSellPrice = (int)Math.Floor((template.BaseSellPrice ?? 0) * tier.PriceMultiplier * (1 + (prefixes.Count + suffixes.Count) * 0.3)),
                Description = BuildDescription(template, prefixes, suffixes, tier),
            };
            inst.PrefixIds.AddRange(prefixes.Select(x => x.Id));
            inst.SuffixIds.AddRange(suffixes.Select(x => x.Id));
            state.GeneratedEquips.Add(inst); state.EquipCounter++;
            return inst;
        }
        private static List<AffixDef> PickAffixes(GameDatabase db, EquipBaseTemplateDef template, string position, int count, int realm, HashSet<string> selected, IRng rng)
        {
            var result = new List<AffixDef>();
            for (int i = 0; i < count; i++)
            {
                var pool = db.Affixes.Values.Where(a => a.Position == position && !selected.Contains(a.Id) && (a.MinRealm ?? 0) <= realm &&
                    (a.MaxRealm == null || realm <= a.MaxRealm.Value) &&
                    (a.SlotRestriction == null || a.SlotRestriction.Count == 0 || a.SlotRestriction.Contains(template.Slot)) &&
                    (a.ExcludeAffixes == null || !a.ExcludeAffixes.Any(selected.Contains)) &&
                    (position != "prefix" || template.AllowedPrefixes == null || template.AllowedPrefixes.Count == 0 || template.AllowedPrefixes.Contains(a.Id)) &&
                    (position != "suffix" || template.AllowedSuffixes == null || template.AllowedSuffixes.Count == 0 || template.AllowedSuffixes.Contains(a.Id))).ToList();
                if (pool.Count == 0) break;
                var picked = ProceduralSeed.WeightedPick(pool, a => a.Weight ?? 1, rng);
                selected.Add(picked.Id); result.Add(picked);
            }
            return result;
        }
        private static Dictionary<string, double> ScaleStats(Dictionary<string, double> stats, double mult) => (stats ?? new Dictionary<string, double>()).ToDictionary(kv => kv.Key, kv => Math.Floor(kv.Value * mult));
        private static void MergeStats(Dictionary<string, double> stats, Dictionary<string, double> add) { foreach (var kv in add ?? new Dictionary<string, double>()) stats[kv.Key] = (stats.TryGetValue(kv.Key, out var v) ? v : 0) + kv.Value; }
        private static string BuildName(EquipBaseTemplateDef t, List<AffixDef> p, List<AffixDef> s, QualityTier tier) { var body = string.Join("·", p.Select(x => x.Name).Concat(new[] { t.BaseName }).Concat(s.Select(x => x.Name))); return tier.Quality == "mortal" ? body : $"[{tier.DisplayName}] {body}"; }
        private static string BuildDescription(EquipBaseTemplateDef t, List<AffixDef> p, List<AffixDef> s, QualityTier tier)
        {
            var prefixDesc = string.Join("，", p.Select(x => x.Description));
            var suffixDesc = string.Join("，", s.Select(x => x.Description));
            return (t.DescriptionPattern ?? "{baseName}")
                .Replace("{prefix_desc}", prefixDesc.Length == 0 ? ProceduralTexts.NormalPrefixDescription : prefixDesc)
                .Replace("{quality_adj}", tier.DisplayName)
                .Replace("{baseName}", t.BaseName)
                .Replace("{suffix_desc}", suffixDesc.Length == 0 ? ProceduralTexts.NoSuffixDescription : suffixDesc);
        }
    }
}
