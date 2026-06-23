using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Systems.Procedural
{
    public sealed class ProceduralEventState { public int MasterSeed = 1234567; public int EventCounter; }
    public sealed class GeneratedEventResult { public GameEventDef Event; public Player Player; }

    public static class EventGenerator
    {
        public static ProceduralEventState GetState(Player player)
        {
            if (player.Systems.TryGetValue("procedural", out var existing))
            {
                if (existing is ProceduralEventState s) return s;
                if (existing is JObject jo) return jo.ToObject<ProceduralEventState>() ?? new ProceduralEventState();
            }
            var state = new ProceduralEventState(); player.Systems["procedural"] = state; return state;
        }

        public static GeneratedEventResult GenerateEvent(GameDatabase db, Player player, string category = null, IEnumerable<string> regionTags = null, int? seed = null)
        {
            var state = GetState(player);
            int subSeed = seed ?? ProceduralSeed.HashSeed(state.MasterSeed, state.EventCounter);
            state.EventCounter++;
            var rng = ProceduralSeed.CreateRng(subSeed);
            var tags = regionTags?.ToList();
            var eligible = db.EventTemplates.Values.Where(t =>
                (category == null || t.Category == category) &&
                WorldConditions.CheckBasic(db, player, t.Condition) &&
                (tags == null || t.RegionTags == null || t.RegionTags.Count == 0 || t.RegionTags.Any(tags.Contains))).ToList();
            if (eligible.Count == 0) return null;
            var template = ProceduralSeed.WeightedPick(eligible, t => t.Weight ?? 1, rng);
            var vars = new Dictionary<string, string>();
            foreach (var slot in template.VariableSlots ?? new List<string>())
            {
                if (vars.ContainsKey(slot)) continue;
                var pool = FindPool(db, template, slot, rng);
                if (pool == null || pool.Entries == null || pool.Entries.Count == 0) { vars[slot] = "{" + slot + "}"; continue; }
                var filtered = FilterEntries(pool.Entries, player.RealmIndex, tags).ToList();
                JToken entry = filtered.Count == 0 ? pool.Entries[0] : ProceduralSeed.WeightedPick(filtered, e => (double?)e["weight"] ?? 1, rng);
                vars[slot] = (string)entry["value"] ?? "";
                if (entry["linkedValues"] is JObject linked)
                    foreach (var prop in linked.Properties()) vars[prop.Name] = prop.Value.ToString();
            }
            var effects = new Dictionary<string, EffectValue>();
            foreach (var kv in template.EffectsPattern ?? new Dictionary<string, string>())
            {
                string resolved = ProceduralInterpolate.Interpolate(kv.Value, vars);
                if (double.TryParse(resolved, out var num)) effects[kv.Key] = EffectValue.FromScalar(num);
            }
            var ev = new GameEventDef
            {
                Id = $"proc:{template.Id}:{((uint)subSeed).ToString("x").Substring(0, Math.Min(6, ((uint)subSeed).ToString("x").Length))}",
                Category = template.Category,
                Tone = template.Tone,
                Name = ProceduralInterpolate.Interpolate(template.NamePattern, vars),
                Weight = template.Weight,
                Effects = effects,
                Message = ProceduralInterpolate.Interpolate(template.MessagePattern, vars),
                Once = false,
                RegionTags = template.RegionTags,
            };
            return new GeneratedEventResult { Event = ev, Player = player };
        }

        private static VariablePoolDef FindPool(GameDatabase db, EventTemplateDef t, string slot, IRng rng)
        {
            string poolId = (string)t.VarConstraints?[slot]?["pool"];
            if (!string.IsNullOrEmpty(poolId) && db.VariablePools.TryGetValue(poolId, out var constrained)) return constrained;
            var pools = db.VariablePools.Values.Where(p => p.Variable == slot).ToList();
            return pools.Count == 0 ? null : pools[Math.Min(pools.Count - 1, (int)Math.Floor(rng.NextDouble() * pools.Count))];
        }
        private static IEnumerable<JToken> FilterEntries(IEnumerable<JToken> entries, int realm, List<string> tags)
        {
            foreach (var e in entries)
            {
                if (((int?)e["minRealm"]).HasValue && realm < (int)e["minRealm"]) continue;
                if (((int?)e["maxRealm"]).HasValue && realm > (int)e["maxRealm"]) continue;
                var rtags = e["regionTags"]?.Values<string>().ToList();
                if (tags != null && rtags != null && rtags.Count > 0 && !rtags.Any(tags.Contains)) continue;
                yield return e;
            }
        }
    }
}
