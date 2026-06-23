// ============================================================
// EconomyStateHelpers.cs — shared economy state helpers
// UnityEngine-free
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class LearningState
    {
        public ActiveStudy ActiveStudy;
        public string[] LearnedRecipes = new string[0];
        public string[] LearnedSmithingRecipes = new string[0];
        public int MigrationVersion = 1;
    }

    internal static class EconomyStateHelpers
    {
        public static bool HasLearnedRecipe(Player player, string id) => HasLearned(player, "learnedRecipes", "LearnedRecipes", id);
        public static bool HasLearnedSmithingRecipe(Player player, string id) => HasLearned(player, "learnedSmithingRecipes", "LearnedSmithingRecipes", id);

        private static bool HasLearned(Player player, string jsonName, string clrName, string id)
        {
            if (!player.Systems.TryGetValue("learning", out var state) || state == null) return false;
            if (state is LearningState typed)
            {
                var values = jsonName == "learnedRecipes" ? typed.LearnedRecipes : typed.LearnedSmithingRecipes;
                return values != null && values.Contains(id);
            }
            if (state is JObject jo)
            {
                var arr = jo[jsonName] ?? jo[clrName];
                return arr != null && arr.Values<string>().Contains(id);
            }
            if (state is IDictionary<string, object> dict && dict.TryGetValue(jsonName, out var value)) return EnumerableContains(value, id);
            var prop = state.GetType().GetProperty(jsonName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? state.GetType().GetProperty(clrName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop != null && EnumerableContains(prop.GetValue(state), id);
        }

        private static bool EnumerableContains(object value, string id)
        {
            if (value is string s) return s == id;
            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                    if ((item as string) == id) return true;
            }
            return false;
        }

        public static RegionDef GetCurrentRegion(GameDatabase db, Player player)
        {
            string regionId = null;
            if (player.Systems.TryGetValue("map", out var map) && map != null)
            {
                if (map is JObject jo) regionId = jo.Value<string>("currentRegionId") ?? jo.Value<string>("CurrentRegionId");
                else
                {
                    var prop = map.GetType().GetProperty("currentRegionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                        ?? map.GetType().GetProperty("CurrentRegionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    regionId = prop?.GetValue(map) as string;
                }
            }
            if (!string.IsNullOrEmpty(regionId) && db.Regions.TryGetValue(regionId, out var region)) return region;
            return db.Regions.Values.FirstOrDefault(r => (r.SafeZone ?? false) && (r.MinRealm ?? 0) == 0)
                ?? db.Regions.Values.FirstOrDefault(r => (r.MinRealm ?? 0) == 0)
                ?? db.Regions.Values.FirstOrDefault();
        }
    }
}
