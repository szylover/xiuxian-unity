using System;
using System.Collections.Generic;
using System.Linq;

namespace Xiuxian.Systems.Procedural
{
    public sealed class QualityTier
    {
        public string Quality, DisplayName, Rarity;
        public double StatMultiplier, PriceMultiplier, DropWeight;
        public int PrefixSlots, SuffixSlots;
    }

    public static class ProceduralRarity
    {
        private static readonly double[] LuckScaling = { 0, 0.005, 0.01, 0.015, 0.02 };
        public static readonly QualityTier[] QualityTiers =
        {
            new QualityTier { Quality = "mortal", DisplayName = ProceduralTexts.QualityMortal, Rarity = "common", StatMultiplier = 1.0, PrefixSlots = 0, SuffixSlots = 0, PriceMultiplier = 1.0, DropWeight = 50 },
            new QualityTier { Quality = "spirit", DisplayName = ProceduralTexts.QualitySpirit, Rarity = "uncommon", StatMultiplier = 1.5, PrefixSlots = 1, SuffixSlots = 0, PriceMultiplier = 2.0, DropWeight = 30 },
            new QualityTier { Quality = "treasure", DisplayName = ProceduralTexts.QualityTreasure, Rarity = "rare", StatMultiplier = 2.5, PrefixSlots = 1, SuffixSlots = 1, PriceMultiplier = 5.0, DropWeight = 12 },
            new QualityTier { Quality = "immortal", DisplayName = ProceduralTexts.QualityImmortal, Rarity = "epic", StatMultiplier = 4.0, PrefixSlots = 2, SuffixSlots = 1, PriceMultiplier = 12.0, DropWeight = 6 },
            new QualityTier { Quality = "ancient", DisplayName = ProceduralTexts.QualityAncient, Rarity = "legendary", StatMultiplier = 6.0, PrefixSlots = 2, SuffixSlots = 2, PriceMultiplier = 25.0, DropWeight = 2 },
        };
        public static QualityTier GetQualityTier(string quality) => QualityTiers.FirstOrDefault(x => x.Quality == quality) ?? QualityTiers[0];
        public static QualityTier RollRarity(IRng rng, int luck = 0)
        {
            var weighted = QualityTiers.Select((t, i) => (Tier: t, Weight: t.DropWeight * (1 + luck * LuckScaling[i]))).ToList();
            return ProceduralSeed.WeightedPick(weighted, x => x.Weight, rng).Tier;
        }
    }
}
