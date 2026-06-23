// ============================================================
// EquipmentDefs.cs — 装备、炼丹、炼器、商店与经济 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class ItemCountDef
    {
        [JsonProperty("itemId")] public string ItemId;
        [JsonProperty("count")] public int Count;
    }

    public sealed class EquipDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("slot")] public string Slot;
        [JsonProperty("rarity")] public string Rarity;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("stats")] public Dictionary<string, double> Stats;
        [JsonProperty("techType")] public JToken TechType;
        [JsonProperty("physiqueBonusRate")] public double? PhysiqueBonusRate;
        [JsonProperty("sellPrice")] public int? SellPrice;
    }

    public sealed class EquipBaseTemplateDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("baseName")] public string BaseName;
        [JsonProperty("slot")] public string Slot;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("techType")] public JToken TechType;
        [JsonProperty("baseStats")] public Dictionary<string, double> BaseStats;
        [JsonProperty("baseSellPrice")] public int? BaseSellPrice;
        [JsonProperty("descriptionPattern")] public string DescriptionPattern;
    }

    public sealed class AffixDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("position")] public string Position;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("statBonus")] public Dictionary<string, double> StatBonus;
        [JsonProperty("weight")] public int? Weight;
        [JsonProperty("slotRestriction")] public List<string> SlotRestriction;
        [JsonProperty("excludeAffixes")] public List<string> ExcludeAffixes;
        [JsonProperty("minRealm")] public int? MinRealm;
    }

    public sealed class RecipeDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("inputs")] public List<ItemCountDef> Inputs;
        [JsonProperty("outputItemId")] public string OutputItemId;
        [JsonProperty("outputCount")] public int? OutputCount;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("mentalCost")] public int? MentalCost;
        [JsonProperty("baseSuccessRate")] public double? BaseSuccessRate;
        [JsonProperty("qualityBonusMultipliers")] public Dictionary<string, double> QualityBonusMultipliers;
    }

    public sealed class SmithingRecipeDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("inputs")] public List<ItemCountDef> Inputs;
        [JsonProperty("outputItemId")] public string OutputItemId;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("mentalCost")] public int? MentalCost;
        [JsonProperty("goldCost")] public int? GoldCost;
        [JsonProperty("baseSuccessRate")] public double? BaseSuccessRate;
    }

    public sealed class ShopEntryDef
    {
        [JsonProperty("itemId")] public string ItemId;
        [JsonProperty("npcId")] public string NpcId;
        [JsonProperty("stock")] public int? Stock;
        [JsonProperty("buyPrice")] public int? BuyPrice;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("requiredAlignment")] public string RequiredAlignment;
    }

    public sealed class AuctionLotDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("itemId")] public string ItemId;
        [JsonProperty("count")] public int? Count;
        [JsonProperty("basePrice")] public int? BasePrice;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("weight")] public int? Weight;
        [JsonProperty("regionTags")] public List<string> RegionTags;
    }
}
