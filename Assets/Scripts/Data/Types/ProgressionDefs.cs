// ============================================================
// ProgressionDefs.cs — 境界、突破、渡劫、飞升、体修与瓶颈 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class RealmDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("index")] public int Index;
        [JsonProperty("name")] public string Name;
        [JsonProperty("tier")] public string Tier;
        [JsonProperty("expReq")] public int? ExpReq;
        [JsonProperty("hpBase")] public int? HpBase;
        [JsonProperty("mpBase")] public int? MpBase;
        [JsonProperty("atkBase")] public int? AtkBase;
        [JsonProperty("defBase")] public int? DefBase;
        [JsonProperty("speedBase")] public int? SpeedBase;
        [JsonProperty("mentalBase")] public int? MentalBase;
        [JsonProperty("lifespanBonus")] public int? LifespanBonus;
        [JsonProperty("tierTransition")] public JToken TierTransition;
        [JsonProperty("ascensionRequired")] public JToken AscensionRequired;
    }

    public sealed class BreakthroughReqDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("fromRealmIndex")] public int FromRealmIndex;
        [JsonProperty("toRealmIndex")] public int ToRealmIndex;
        [JsonProperty("itemCosts")] public List<ItemCountDef> ItemCosts;
        [JsonProperty("conditions")] public List<JToken> Conditions;
        [JsonProperty("requiresTribulation")] public bool RequiresTribulation;
        [JsonProperty("baseSuccessRate")] public double? BaseSuccessRate;
        [JsonProperty("description")] public string Description;
    }

    public sealed class TribulationDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("forRealmIndex")] public int ForRealmIndex;
        [JsonProperty("waves")] public List<JToken> Waves;
        [JsonProperty("rewards")] public JToken Rewards;
        [JsonProperty("failureType")] public string FailureType;
        [JsonProperty("failureDescription")] public string FailureDescription;
    }

    public sealed class BreakthroughDataDef
    {
        [JsonProperty("breakthroughReqs")] public List<BreakthroughReqDef> BreakthroughReqs;
        [JsonProperty("tribulations")] public List<TribulationDef> Tribulations;
    }

    public sealed class AscensionDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("fromTier")] public string FromTier;
        [JsonProperty("toTier")] public string ToTier;
        [JsonProperty("fromRealmIndex")] public int? FromRealmIndex;
        [JsonProperty("toRealmIndex")] public int? ToRealmIndex;
        [JsonProperty("minExp")] public int? MinExp;
        [JsonProperty("itemCosts")] public List<ItemCountDef> ItemCosts;
        [JsonProperty("conditions")] public List<JToken> Conditions;
        [JsonProperty("tribulationId")] public string TribulationId;
        [JsonProperty("rewards")] public JToken Rewards;
        [JsonProperty("statReset")] public JToken StatReset;
    }

    public sealed class BodyRealmDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("index")] public int Index;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("expReq")] public int? ExpReq;
        [JsonProperty("maxPhysique")] public int? MaxPhysique;
        [JsonProperty("hpBonus")] public int? HpBonus;
        [JsonProperty("atkBonus")] public int? AtkBonus;
        [JsonProperty("defBonus")] public int? DefBonus;
        [JsonProperty("physiqueDmgReduce")] public double? PhysiqueDmgReduce;
    }

    public sealed class SpiritRootBodyBonusDef
    {
        [JsonProperty("rootType")] public string RootType;
        [JsonProperty("bodyExpMultiplier")] public double? BodyExpMultiplier;
        [JsonProperty("hpBonusRate")] public double? HpBonusRate;
        [JsonProperty("physiqueRegenRate")] public double? PhysiqueRegenRate;
        [JsonProperty("dmgReduceBonus")] public double? DmgReduceBonus;
    }

    public sealed class BodyConfigDef
    {
        [JsonProperty("bodyRealms")] public List<BodyRealmDef> BodyRealms;
        [JsonProperty("spiritRootBodyBonuses")] public List<SpiritRootBodyBonusDef> SpiritRootBodyBonuses;
    }

    public sealed class BottleneckDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("targetType")] public string TargetType;
        [JsonProperty("fromRealmIndex")] public int? FromRealmIndex;
        [JsonProperty("fromBodyRealmIndex")] public int? FromBodyRealmIndex;
        [JsonProperty("atLevel")] public int? AtLevel;
        [JsonProperty("hint")] public string Hint;
        [JsonProperty("unlockMethods")] public List<JToken> UnlockMethods;
        [JsonProperty("unlockBonus")] public JToken UnlockBonus;
        [JsonProperty("techniqueId")] public string TechniqueId;
    }
}
