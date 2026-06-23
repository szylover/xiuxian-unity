// ============================================================
// AlchemySystem.cs — recipe checks, success and quality rolls
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class AlchemyResult
    {
        public Player Player;
        public bool Success;
        public string Quality;
        public int Added;
        public string Message;
    }

    public static class AlchemySystem
    {
        public static IEnumerable<RecipeDef> GetAvailableRecipes(GameDatabase db, Player player) => db.Recipes.Values.Where(r => CanCraft(db, player, r.Id));

        public static bool CanCraft(GameDatabase db, Player player, string recipeId)
        {
            if (!db.Recipes.TryGetValue(recipeId, out var recipe)) return false;
            if (!EconomyStateHelpers.HasLearnedRecipe(player, recipeId)) return false;
            if (player.RealmIndex < (recipe.MinRealm ?? 0)) return false;
            if (player.MentalPower < (recipe.MentalCost ?? 0)) return false;
            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>())
                if (!InventorySystem.HasItem(player, input.ItemId, input.Count)) return false;
            return true;
        }

        public static double CalcSuccessRate(Player player, RecipeDef recipe)
        {
            double alchemyBonus = player.Aptitudes.Alchemy * 0.005;
            return Math.Min(0.95, (recipe.BaseSuccessRate ?? 0) + alchemyBonus);
        }

        public static AlchemyResult PerformAlchemy(GameDatabase db, Player player, string recipeId, IRng rng)
        {
            if (!db.Recipes.TryGetValue(recipeId, out var recipe)) return new AlchemyResult { Player = player, Message = EconomyTexts.RecipeNotFound };
            if (!EconomyStateHelpers.HasLearnedRecipe(player, recipeId)) return new AlchemyResult { Player = player, Message = EconomyTexts.RecipeNotFound };
            if (player.RealmIndex < (recipe.MinRealm ?? 0)) return new AlchemyResult { Player = player, Message = EconomyTexts.RealmInsufficient };
            if (player.MentalPower < (recipe.MentalCost ?? 0)) return new AlchemyResult { Player = player, Message = EconomyTexts.MentalInsufficient };
            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>())
                if (!InventorySystem.HasItem(player, input.ItemId, input.Count)) return new AlchemyResult { Player = player, Message = EconomyTexts.MaterialInsufficient };

            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>()) InventorySystem.RemoveItem(player, input.ItemId, input.Count);
            player.MentalPower = Math.Max(0, player.MentalPower - (recipe.MentalCost ?? 0));

            double successRate = CalcSuccessRate(player, recipe);
            if (rng.NextDouble() >= successRate) return new AlchemyResult { Player = player, Success = false, Message = EconomyTexts.Failed };

            string quality = RollQuality(player, rng);
            double multiplier = recipe.QualityBonusMultipliers != null && recipe.QualityBonusMultipliers.TryGetValue(quality, out var m) ? m : 1;
            int outputCount = Math.Max(1, (int)Math.Floor((recipe.OutputCount ?? 1) * multiplier));
            var added = InventorySystem.AddItem(db, player, recipe.OutputItemId, outputCount);
            var state = GetAlchemyState(player);
            state.TotalCrafted += 1;
            if (quality == "excellent") state.ExcellentCount += 1;
            player.Systems["alchemy"] = state;
            return new AlchemyResult { Player = player, Success = true, Quality = quality, Added = added.Added, Message = EconomyTexts.Success };
        }

        private static string RollQuality(Player player, IRng rng)
        {
            double excellentChance = player.Aptitudes.Alchemy / 500.0;
            double goodChance = player.Aptitudes.Alchemy / 200.0;
            double roll = rng.NextDouble();
            if (roll < excellentChance) return "excellent";
            if (roll < goodChance) return "good";
            return "normal";
        }

        private sealed class AlchemyCounterState { public int TotalCrafted; public int ExcellentCount; }
        private static AlchemyCounterState GetAlchemyState(Player player)
        {
            if (player.Systems.TryGetValue("alchemy", out var existing))
            {
                if (existing is AlchemyCounterState typed) return typed;
                if (existing is Newtonsoft.Json.Linq.JObject jo) return jo.ToObject<AlchemyCounterState>() ?? new AlchemyCounterState();
            }
            return new AlchemyCounterState();
        }
    }
}
