// ============================================================
// EventSystem.cs — weighted data-driven event engine
// UnityEngine-free
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class EventResult { public Player Player; public string Message, EventId, Tone, Category; public bool Success; }
    public static class EventSystem
    {
        public static EventRuntimeState GetEventState(Player p) => WorldState.Get(p, "events", () => new EventRuntimeState());
        public static List<GameEventDef> GetAvailableEvents(GameDatabase db, Player p, string category, IEnumerable<string> regionTags = null)
        {
            var state = GetEventState(p); var tags = regionTags?.ToList();
            return db.Events.Values.Where(e => e.Category == category)
                .Where(e => !(e.Once ?? false) || !state.TriggeredOnce.Contains(e.Id))
                .Where(e => !(e.Cooldown.HasValue && state.Cooldowns.TryGetValue(e.Id, out var last) && p.Age - last < e.Cooldown.Value))
                .Where(e => WorldConditions.CheckBasic(db, p, e.Condition))
                .Where(e => tags == null || e.RegionTags == null || e.RegionTags.Count == 0 || e.RegionTags.Any(tags.Contains))
                .ToList();
        }
        public static EventResult TriggerEvent(GameDatabase db, Player p, string category, IRng rng, IEnumerable<string> regionTags = null)
        {
            var available = GetAvailableEvents(db, p, category, regionTags); if (available.Count == 0) return null;
            var picked = WeightedPick(available.Select(e => (Event: e, Weight: AdjustWeight(e, p.Luck))).ToList(), rng);
            var state = GetEventState(p); if (picked.Once ?? false) state.TriggeredOnce.Add(picked.Id); if (picked.Cooldown.HasValue) state.Cooldowns[picked.Id] = p.Age;
            ApplyEffects(p, picked.Effects, rng);
            return new EventResult { Player = p, Success = true, EventId = picked.Id, Category = picked.Category, Tone = picked.Tone, Message = picked.Message };
        }
        public static EventResult TriggerExploreEvent(GameDatabase db, Player p, IRng rng)
        {
            var region = MapSystem.GetCurrentRegion(db, p); var tags = region?.RegionTags;
            if (rng.NextDouble() < 0.10) { var adv = TriggerEvent(db, p, "adventure", rng, tags); if (adv != null) return adv; }
            return TriggerEvent(db, p, "explore", rng, tags) ?? new EventResult { Player = p, Success = false, Message = "noEvent" };
        }
        public static EventResult TriggerDailyEvent(GameDatabase db, Player p, IRng rng) => TriggerEvent(db, p, "daily", rng);
        private static double AdjustWeight(GameEventDef e, int luck)
        {
            double weight = Math.Max(0, e.Weight ?? 1), luckFactor = luck / 50.0;
            return e.Tone == "good" ? weight * (0.5 + luckFactor * 0.5) : e.Tone == "bad" ? weight * (1.5 - luckFactor * 0.5) : weight;
        }
        private static GameEventDef WeightedPick(List<(GameEventDef Event, double Weight)> items, IRng rng)
        {
            double total = items.Sum(i => Math.Max(0, i.Weight)); if (total <= 0) return items.FirstOrDefault().Event;
            double roll = rng.NextDouble() * total; foreach (var item in items) { roll -= Math.Max(0, item.Weight); if (roll <= 0) return item.Event; } return items.Last().Event;
        }
        public static void ApplyEffects(Player p, Dictionary<string, EffectValue> effects, IRng rng)
        {
            if (effects == null) return;
            foreach (var kv in effects)
            {
                int cur = GetNumeric(p, kv.Key); int? max = MaxFor(p, kv.Key); int next = Resolve(cur, max, kv.Value, rng);
                if (max.HasValue) next = Math.Min(next, max.Value); if (kv.Key == "mood" || kv.Key == "health") next = Math.Min(100, next);
                next = kv.Key == "lifespan" ? next : Math.Max(kv.Key == "hp" ? 1 : 0, next);
                SetNumeric(p, kv.Key, next);
            }
        }
        private static int Resolve(int cur, int? max, EffectValue v, IRng rng)
        {
            return v.Kind switch { EffectValueKind.Scalar => cur + (int)Math.Round(v.Scalar), EffectValueKind.Range => cur + rng.NextIntInclusive((int)v.RangeMin, (int)v.RangeMax), EffectValueKind.Expr when v.Expr == "max" => max ?? cur, EffectValueKind.Expr when v.Expr != null && v.Expr.StartsWith("=") => (int)Math.Floor(double.Parse(v.Expr.Substring(1))), EffectValueKind.Expr when v.Expr != null && v.Expr.StartsWith("*") => (int)Math.Floor(cur * double.Parse(v.Expr.Substring(1))), _ => cur };
        }
        private static int GetNumeric(Player p, string f) => f switch { "hp" => p.Hp, "mp" => p.Mp, "stamina" => p.Stamina, "exp" => p.Exp, "gold" => p.Gold, "mood" => p.Mood, "health" => p.Health, "mentalPower" => p.MentalPower, "lifespan" => p.Lifespan, "karmaChange" => 0, _ => 0 };
        private static void SetNumeric(Player p, string f, int v) { switch (f) { case "hp": p.Hp = v; break; case "mp": p.Mp = v; break; case "stamina": p.Stamina = v; break; case "exp": p.Exp = v; break; case "gold": p.Gold = v; break; case "mood": p.Mood = v; break; case "health": p.Health = v; break; case "mentalPower": p.MentalPower = v; break; case "lifespan": p.Lifespan = v; break; case "karmaChange": p.Karma = Math.Max(-100, Math.Min(100, p.Karma + v)); break; } }
        private static int? MaxFor(Player p, string f) => f switch { "hp" => p.MaxHp, "mp" => p.MaxMp, "stamina" => p.MaxStamina, "mentalPower" => p.MaxMentalPower, _ => null };
    }
}
