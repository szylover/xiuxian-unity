// ============================================================
// DivineArtSystem.cs — divine art state and elemental damage
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class DivineArtSlot { public string ArtId; }
    public sealed class DivineArtsSystemState
    {
        public readonly List<DivineArtSlot> Learned = new();
        public string ActiveArtId;
    }

    public static class DivineArtSystem
    {
        public static DivineArtsSystemState GetState(Player player)
        {
            if (!player.Systems.TryGetValue("divineArts", out var existing))
            {
                var state = new DivineArtsSystemState();
                player.Systems["divineArts"] = state;
                return state;
            }
            if (existing is DivineArtsSystemState typed) return typed;
            if (existing is JObject jo)
            {
                var state = new DivineArtsSystemState { ActiveArtId = (string)jo["activeArtId"] };
                var learned = jo["learned"] as JArray;
                if (learned != null)
                    foreach (var item in learned)
                        state.Learned.Add(new DivineArtSlot { ArtId = (string)item["artId"] });
                player.Systems["divineArts"] = state;
                return state;
            }
            return new DivineArtsSystemState();
        }

        public static bool Learn(GameDatabase db, Player player, string artId)
        {
            if (!db.DivineArts.TryGetValue(artId, out var art)) return false;
            if (player.RealmIndex < (art.MinRealm ?? 0)) return false;
            if (GetAptitude(player, art.Element) < (art.MinAptitude ?? 30)) return false;
            var state = GetState(player);
            if (state.Learned.Exists(x => x.ArtId == artId)) return false;
            state.Learned.Add(new DivineArtSlot { ArtId = artId });
            return true;
        }

        public static bool Activate(GameDatabase db, Player player, string artId)
        {
            if (!db.DivineArts.ContainsKey(artId)) return false;
            var state = GetState(player);
            if (!state.Learned.Exists(x => x.ArtId == artId)) return false;
            state.ActiveArtId = artId;
            return true;
        }

        public static double CalcAptitudePower(Player player, DivineArtDef art)
        {
            int aptitude = GetAptitude(player, art.Element);
            return 1.0 + Math.Max(0, aptitude - 30) / 140.0 * (art.AptitudeScaling ?? 1.0);
        }

        public static bool IsElementCountered(string attackElement, string defendElement)
        {
            return !string.IsNullOrEmpty(attackElement) && !string.IsNullOrEmpty(defendElement)
                && ElementTable.Counter.TryGetValue(attackElement, out var list)
                && Array.IndexOf(list, defendElement) >= 0;
        }

        public static DamageResult CalcDamage(Player player, Combatant playerCombatant, DivineArtDef art, Combatant monster, int currentMonsterHp, IRng rng)
        {
            var result = new DamageResult();
            double aptitudePower = CalcAptitudePower(player, art);
            double effectiveDef = monster.Def * (1 - (art.DefPenetration ?? 0));
            bool countered = IsElementCountered(art.Element, monster.Element);
            double elementMultiplier = countered ? ElementTable.CounterMultiplier : 1.0;
            double resistMultiplier = 1 - (monster.ElementResists.TryGetValue(art.Element ?? string.Empty, out var r) ? r : 0);
            int hitCount = art.HitCount ?? 1;
            for (int hit = 0; hit < hitCount; hit++)
            {
                if (currentMonsterHp - result.Damage <= 0) break;
                double baseDmg = Math.Max(1, playerCombatant.Atk * (art.DmgMultiplier ?? 1) * aptitudePower - effectiveDef);
                double dmg = baseDmg * elementMultiplier * resistMultiplier;
                bool crit = false, dodge = false;
                if (rng.NextDouble() * 100 < Math.Max(0, playerCombatant.CritRate - monster.CritResist))
                {
                    dmg *= playerCombatant.CritDmgMultiplier <= 0 ? 1.5 : playerCombatant.CritDmgMultiplier;
                    crit = true;
                }
                if (rng.NextDouble() < monster.MoveSpeed / (monster.MoveSpeed + 100.0))
                {
                    dmg = 0;
                    dodge = true;
                }
                int finalDmg = (int)Math.Floor(dmg);
                result.Damage += finalDmg;
                if (hitCount > 1) result.Logs.Add(dodge ? CombatTexts.SegmentDodge(hit + 1) : crit ? CombatTexts.SegmentCrit(hit + 1, finalDmg) : CombatTexts.SegmentHit(hit + 1, finalDmg));
            }
            if (countered) result.Logs.Add(CombatTexts.ElementCounter(ElementTable.CounterMultiplier));
            return result;
        }

        public static bool TryUse(DivineArtDef art, SkillState state, int availableMp, IRng rng)
        {
            return state.CooldownLeft <= 0 && availableMp >= (art.MpCost ?? 0) && rng.NextDouble() < (art.TriggerRate ?? 0);
        }

        public static int GetAptitude(Player p, string element)
        {
            switch (element)
            {
                case ElementType.Fire: return p.Aptitudes.Fire;
                case ElementType.Water: return p.Aptitudes.Water;
                case ElementType.Thunder: return p.Aptitudes.Thunder;
                case ElementType.Wind: return p.Aptitudes.Wind;
                case ElementType.Earth: return p.Aptitudes.Earth;
                case ElementType.Wood: return p.Aptitudes.Wood;
                case ElementType.Metal: return p.Aptitudes.Metal;
                default: return 0;
            }
        }
    }
}
