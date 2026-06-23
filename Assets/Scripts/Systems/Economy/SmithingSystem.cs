// ============================================================
// SmithingSystem.cs — forge equipment/items from recipes
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class SmithingResult
    {
        public Player Player;
        public bool Success;
        public int Added;
        public string OutputItemId;
        public string Message;
    }

    public static class SmithingSystem
    {
        public static IEnumerable<SmithingRecipeDef> GetAvailableSmithingRecipes(GameDatabase db, Player player) => db.SmithingRecipes.Values.Where(r => CanSmith(db, player, r.Id));

        public static bool CanSmith(GameDatabase db, Player player, string recipeId)
        {
            if (!db.SmithingRecipes.TryGetValue(recipeId, out var recipe)) return false;
            if (!EconomyStateHelpers.HasLearnedSmithingRecipe(player, recipeId)) return false;
            if (player.RealmIndex < (recipe.MinRealm ?? 0) || player.MentalPower < (recipe.MentalCost ?? 0) || player.Gold < (recipe.GoldCost ?? 0)) return false;
            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>())
                if (!InventorySystem.HasItem(player, input.ItemId, input.Count)) return false;
            return true;
        }

        public static double CalcSmithingSuccessRate(Player player, SmithingRecipeDef recipe)
        {
            double smithingBonus = player.Aptitudes.Smithing * 0.005;
            return Math.Min(0.95, (recipe.BaseSuccessRate ?? 0) + smithingBonus);
        }

        public static SmithingResult PerformSmithing(GameDatabase db, Player player, string recipeId, IRng rng)
        {
            if (!db.SmithingRecipes.TryGetValue(recipeId, out var recipe)) return new SmithingResult { Player = player, Message = EconomyTexts.RecipeNotFound };
            if (!EconomyStateHelpers.HasLearnedSmithingRecipe(player, recipeId)) return new SmithingResult { Player = player, Message = EconomyTexts.RecipeNotFound };
            if (player.RealmIndex < (recipe.MinRealm ?? 0)) return new SmithingResult { Player = player, Message = EconomyTexts.RealmInsufficient };
            if (player.MentalPower < (recipe.MentalCost ?? 0)) return new SmithingResult { Player = player, Message = EconomyTexts.MentalInsufficient };
            if (player.Gold < (recipe.GoldCost ?? 0)) return new SmithingResult { Player = player, Message = EconomyTexts.GoldInsufficient };
            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>())
                if (!InventorySystem.HasItem(player, input.ItemId, input.Count)) return new SmithingResult { Player = player, Message = EconomyTexts.MaterialInsufficient };

            foreach (var input in recipe.Inputs ?? new List<ItemCountDef>()) InventorySystem.RemoveItem(player, input.ItemId, input.Count);
            player.MentalPower = Math.Max(0, player.MentalPower - (recipe.MentalCost ?? 0));
            player.Gold -= recipe.GoldCost ?? 0;
            if (rng.NextDouble() >= CalcSmithingSuccessRate(player, recipe)) return new SmithingResult { Player = player, Success = false, OutputItemId = recipe.OutputItemId, Message = EconomyTexts.Failed };

            var added = InventorySystem.AddItem(db, player, recipe.OutputItemId, 1);
            return new SmithingResult { Player = player, Success = true, Added = added.Added, OutputItemId = recipe.OutputItemId, Message = added.Added > 0 ? EconomyTexts.Success : EconomyTexts.InventoryFull };
        }
    }
}
