// ============================================================
// TechniqueDefs.cs — 功法、词条与神通 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class TechniqueDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("type")] public string Type;
        [JsonProperty("rarity")] public string Rarity;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("maxLevel")] public int? MaxLevel;
        [JsonProperty("expPerLevel")] public int? ExpPerLevel;
        [JsonProperty("aptitudeKey")] public string AptitudeKey;
        [JsonProperty("spiritRootElement")] public string SpiritRootElement;
        [JsonProperty("requiredSpiritRoot")] public string RequiredSpiritRoot;
        [JsonProperty("requiredAlignment")] public string RequiredAlignment;
        [JsonProperty("karmaShift")] public int? KarmaShift;
        [JsonProperty("statBonusPerLevel")] public Dictionary<string, double> StatBonusPerLevel;
        [JsonProperty("passiveEffects")] public List<JToken> PassiveEffects;
        [JsonProperty("activeSkill")] public JToken ActiveSkill;
        [JsonProperty("bodyExpRate")] public double? BodyExpRate;
    }

    public sealed class DivineArtDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("element")] public string Element;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("minAptitude")] public int? MinAptitude;
        [JsonProperty("mpCost")] public int? MpCost;
        [JsonProperty("cooldown")] public int? Cooldown;
        [JsonProperty("dmgMultiplier")] public double? DmgMultiplier;
        [JsonProperty("hitCount")] public int? HitCount;
        [JsonProperty("triggerRate")] public double? TriggerRate;
        [JsonProperty("defPenetration")] public double? DefPenetration;
        [JsonProperty("aptitudeScaling")] public double? AptitudeScaling;
        [JsonProperty("requiredAlignment")] public string RequiredAlignment;
        [JsonProperty("effects")] public List<JToken> Effects;
    }

    public sealed class TechniqueTraitDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("tier")] public string Tier;
        [JsonProperty("stat")] public string Stat;
        [JsonProperty("baseValue")] public double? BaseValue;
        [JsonProperty("qualityScaling")] public double? QualityScaling;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("maxRealm")] public int? MaxRealm;
        [JsonProperty("typeRestriction")] public JToken TypeRestriction;
        [JsonProperty("excludeTraits")] public List<string> ExcludeTraits;
        [JsonProperty("weight")] public int? Weight;
    }
}
