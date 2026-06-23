// ============================================================
// TechniqueSystem.cs — active technique bonuses used by combat
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public static class TechniqueSystem
    {
        public static ActiveSkillInfo GetActiveSkillInfo(GameDatabase db, Player player)
        {
            if (string.IsNullOrEmpty(player.ActiveTechniqueId)) return null;
            var slot = player.Techniques.FirstOrDefault(x => x.TechniqueId == player.ActiveTechniqueId);
            if (slot == null || !db.Techniques.TryGetValue(slot.TechniqueId, out var def) || def.ActiveSkill == null) return null;
            return new ActiveSkillInfo { Def = def, Slot = slot, Skill = def.ActiveSkill, AptitudeBonus = CalcAptitudeBonus(player, def) };
        }

        public static Dictionary<string, double> GetActiveTechniqueBonus(GameDatabase db, Player player)
        {
            var result = new Dictionary<string, double>();
            if (string.IsNullOrEmpty(player.ActiveTechniqueId)) return result;
            var slot = player.Techniques.FirstOrDefault(x => x.TechniqueId == player.ActiveTechniqueId);
            if (slot == null || !db.Techniques.TryGetValue(slot.TechniqueId, out var def)) return result;
            AddScaled(result, def.StatBonusPerLevel, slot.Level);
            foreach (var passive in def.PassiveEffects ?? new List<JToken>())
            {
                int minLevel = (int?)passive["minLevel"] ?? 0;
                if (slot.Level >= minLevel)
                {
                    string stat = (string)passive["stat"];
                    double value = (double?)passive["value"] ?? 0;
                    result[stat] = result.Get(stat) + value;
                }
            }
            return result;
        }

        public static double CalcAptitudeBonus(Player player, TechniqueDef def)
        {
            int aptitude = GetAptitude(player, def.AptitudeKey ?? def.Type);
            return 1.0 + System.Math.Max(0, aptitude - 30) / 140.0;
        }

        public static int GetPhysiqueWeaponAttackBonus(GameDatabase db, Player player)
        {
            string weaponId = player.Equipped?.Weapon;
            if (string.IsNullOrEmpty(weaponId)) return 0;
            var def = EquipmentSystem.GetEquipDef(db, player, weaponId);
            if (def?.PhysiqueBonusRate == null || def.TechType == null) return 0;
            if (string.IsNullOrEmpty(player.ActiveTechniqueId) || !db.Techniques.TryGetValue(player.ActiveTechniqueId, out var tech)) return 0;
            foreach (var type in TokenStrings(def.TechType))
                if (type == tech.Type) return (int)System.Math.Floor(player.Physique * def.PhysiqueBonusRate.Value);
            return 0;
        }

        public static bool EquippedWeaponHasTechType(GameDatabase db, Player player, params string[] types)
        {
            string weaponId = player.Equipped?.Weapon;
            if (string.IsNullOrEmpty(weaponId)) return false;
            var def = EquipmentSystem.GetEquipDef(db, player, weaponId);
            if (def?.TechType == null) return false;
            var set = new HashSet<string>(types);
            return TokenStrings(def.TechType).Any(set.Contains);
        }

        private static void AddScaled(Dictionary<string, double> target, Dictionary<string, double> stats, int level)
        {
            foreach (var kv in stats ?? new Dictionary<string, double>()) target[kv.Key] = target.Get(kv.Key) + kv.Value * level;
        }

        private static IEnumerable<string> TokenStrings(JToken token)
        {
            if (token is JArray arr) foreach (var item in arr) yield return item.ToString();
            else if (token != null) yield return token.ToString();
        }

        private static int GetAptitude(Player p, string key)
        {
            switch (key)
            {
                case "blade": return p.Aptitudes.Blade;
                case "spear": return p.Aptitudes.Spear;
                case "sword": return p.Aptitudes.Sword;
                case "fist": return p.Aptitudes.Fist;
                case "palm": return p.Aptitudes.Palm;
                case "finger": return p.Aptitudes.Finger;
                case ElementType.Fire: return p.Aptitudes.Fire;
                case ElementType.Water: return p.Aptitudes.Water;
                case ElementType.Thunder: return p.Aptitudes.Thunder;
                case ElementType.Wind: return p.Aptitudes.Wind;
                case ElementType.Earth: return p.Aptitudes.Earth;
                case ElementType.Wood: return p.Aptitudes.Wood;
                case ElementType.Metal: return p.Aptitudes.Metal;
                default: return 50;
            }
        }
    }
}
