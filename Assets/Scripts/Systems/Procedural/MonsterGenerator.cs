using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Systems.Procedural
{
    public sealed class ProceduralMonsterState { public int MasterSeed = 1234567; public int MonsterCounter; }
    public sealed class GeneratedMonsterResult { public Player Player; public MonsterDef Monster; }
    public sealed class GenerateMonsterOptions { public IEnumerable<string> RegionTags; public bool ForceElite, ForceBoss; public int? Seed; }

    public static class MonsterGenerator
    {
        public static ProceduralMonsterState GetState(Player player)
        {
            if (player.Systems.TryGetValue("proceduralMonsterState", out var existing))
            {
                if (existing is ProceduralMonsterState s) return s;
                if (existing is JObject jo) return jo.ToObject<ProceduralMonsterState>() ?? new ProceduralMonsterState();
            }
            var state = new ProceduralMonsterState(); player.Systems["proceduralMonsterState"] = state; return state;
        }
        public static GeneratedMonsterResult GenerateMonsterVariant(GameDatabase db, Player player, GenerateMonsterOptions options = null)
        {
            options ??= new GenerateMonsterOptions();
            var tags = options.RegionTags?.ToList(); int realm = player.RealmIndex;
            var eligible = db.MonsterTemplates.Values.Where(t => (t.MinRealm == null || realm >= t.MinRealm.Value) && (t.MaxRealm == null || realm <= t.MaxRealm.Value) && (tags == null || t.RegionTags == null || t.RegionTags.Count == 0 || t.RegionTags.Any(tags.Contains))).ToList();
            if (eligible.Count == 0) return null;
            var state = GetState(player); int seed = options.Seed ?? ProceduralSeed.HashSeed(state.MasterSeed, state.MonsterCounter); state.MonsterCounter++;
            var rng = ProceduralSeed.CreateRng(seed);
            var template = ProceduralSeed.WeightedPick(eligible, _ => 1, rng);
            double statScale = Math.Pow(1.8, realm), expScale = Math.Pow(2.0, realm), goldScale = Math.Pow(1.5, realm);
            var applied = new List<MutationDef>();
            double bossChance = options.ForceBoss ? 1 : 0.01 + player.Luck * 0.0005;
            double eliteChance = options.ForceElite ? 1 : 0.05 + player.Luck * 0.001;
            if (rng.NextDouble() < bossChance) { var m = db.Mutations.Values.FirstOrDefault(x => x.Type == "boss"); if (m != null) applied.Add(m); }
            else if (rng.NextDouble() < eliteChance) { var m = db.Mutations.Values.FirstOrDefault(x => x.Type == "elite"); if (m != null) applied.Add(m); }
            var normal = db.Mutations.Values.Where(m => m.Type != "elite" && m.Type != "boss" && (m.Weight ?? 0) > 0 && (m.MinRealm == null || realm >= m.MinRealm.Value) && (m.MaxRealm == null || realm <= m.MaxRealm.Value) && (tags == null || m.RegionTags == null || m.RegionTags.Count == 0 || m.RegionTags.Any(tags.Contains)) && (template.AllowedMutations == null || template.AllowedMutations.Count == 0 || template.AllowedMutations.Contains(m.Id))).ToList();
            if (normal.Count > 0)
            {
                int mutCount = rng.NextDouble() < 0.3 ? (rng.NextDouble() < 0.4 ? 2 : 1) : 0; var picked = new HashSet<string>();
                for (int i = 0; i < mutCount; i++)
                {
                    var mut = ProceduralSeed.WeightedPick(normal, m => m.Weight ?? 1, rng);
                    if (picked.Contains(mut.Id)) continue;
                    if ((mut.Exclusive ?? false) && applied.Any(m => m.Type == mut.Type)) continue;
                    applied.Add(mut); picked.Add(mut.Id);
                }
            }
            double hp = Get(template.BaseStats, "hp") * statScale, atk = Get(template.BaseStats, "atk") * statScale, def = Get(template.BaseStats, "def") * statScale, speed = Get(template.BaseStats, "speed") * statScale, move = Get(template.BaseStats, "moveSpeed") * statScale;
            double crit = Get(template.BaseStats, "critRate"), resist = Get(template.BaseStats, "critResist"), critDmg = template.BaseStats != null && template.BaseStats.TryGetValue("critDmgMultiplier", out var cdm) ? cdm : 1.5;
            foreach (var m in applied)
            {
                var mods = m.StatModifiers ?? new Dictionary<string, double>();
                if (mods.TryGetValue("hp", out var v)) hp *= v; if (mods.TryGetValue("atk", out v)) atk *= v; if (mods.TryGetValue("def", out v)) def *= v; if (mods.TryGetValue("speed", out v)) speed *= v; if (mods.TryGetValue("moveSpeed", out v)) move *= v; if (mods.TryGetValue("critRate", out v)) crit *= v; if (mods.TryGetValue("critResist", out v)) resist *= v; if (mods.TryGetValue("critDmgMultiplier", out v)) critDmg *= v;
            }
            int finalHp = (int)Math.Floor(hp * Jitter(rng, 0.9, 1.1)), finalAtk = (int)Math.Floor(atk * Jitter(rng, 0.9, 1.1)), finalDef = (int)Math.Floor(def * Jitter(rng, 0.9, 1.1)), finalSpeed = (int)Math.Floor(speed * Jitter(rng, 0.95, 1.05)), finalMove = (int)Math.Floor(move * Jitter(rng, 0.95, 1.05));
            double expBonus = 1, goldBonus = 1; string element = template.Element; var resists = template.ElementResists == null ? null : new Dictionary<string, double>(template.ElementResists); string prefix = "", suffix = "", emoji = template.Emoji;
            foreach (var m in applied) { if (m.ExpBonus.HasValue) expBonus *= m.ExpBonus.Value; if (m.GoldBonus.HasValue) goldBonus *= m.GoldBonus.Value; if (!string.IsNullOrEmpty(m.Element)) element = m.Element; if (m.ElementResists != null) { resists ??= new Dictionary<string, double>(); foreach (var kv in m.ElementResists) resists[kv.Key] = kv.Value; } if (!string.IsNullOrEmpty(m.NamePrefix)) prefix += m.NamePrefix; if (!string.IsNullOrEmpty(m.NameSuffix)) suffix += m.NameSuffix; if (!string.IsNullOrEmpty(m.EmojiOverride)) emoji = m.EmojiOverride; }
            var seedHex = ((uint)seed).ToString("x"); if (seedHex.Length > 6) seedHex = seedHex.Substring(0, 6);
            var monster = new MonsterDef { Id = $"proc-mon:{template.Id}:{seedHex}", Name = !string.IsNullOrEmpty(prefix) ? $"{prefix}·{template.BaseName}{suffix}" : $"{template.BaseName}{suffix}", Emoji = emoji, RealmIndex = realm, Hp = Math.Max(1, finalHp), Atk = Math.Max(1, finalAtk), Def = Math.Max(0, finalDef), Speed = Math.Max(1, finalSpeed), MoveSpeed = Math.Max(1, finalMove), CritRate = Math.Max(0, Math.Floor(crit)), CritResist = Math.Max(0, Math.Floor(resist)), CritDmgMultiplier = critDmg, ExpReward = Math.Max(1, (int)Math.Floor((template.BaseExpReward ?? 0) * expScale * expBonus)), GoldReward = Math.Max(0, (int)Math.Floor((template.BaseGoldReward ?? 0) * goldScale * goldBonus)), Element = element, RegionTags = template.RegionTags };
            return new GeneratedMonsterResult { Player = player, Monster = monster };
        }
        private static double Get(Dictionary<string, double> d, string k) => d != null && d.TryGetValue(k, out var v) ? v : 0;
        private static double Jitter(IRng rng, double min, double max) => min + rng.NextDouble() * (max - min);
    }
}
