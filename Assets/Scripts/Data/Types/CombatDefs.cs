// ============================================================
// CombatDefs.cs — 妖兽、模板与变异 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class MonsterDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("emoji")] public string Emoji;
        [JsonProperty("realmIndex")] public int? RealmIndex;
        [JsonProperty("hp")] public int? Hp;
        [JsonProperty("atk")] public int? Atk;
        [JsonProperty("def")] public int? Def;
        [JsonProperty("speed")] public int? Speed;
        [JsonProperty("critRate")] public double? CritRate;
        [JsonProperty("critDmgMultiplier")] public double? CritDmgMultiplier;
        [JsonProperty("critResist")] public double? CritResist;
        [JsonProperty("moveSpeed")] public int? MoveSpeed;
        [JsonProperty("element")] public string Element;
        [JsonProperty("expReward")] public int? ExpReward;
        [JsonProperty("goldReward")] public int? GoldReward;
        [JsonProperty("regionTags")] public List<string> RegionTags;
    }

    public sealed class MonsterTemplateDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("baseName")] public string BaseName;
        [JsonProperty("emoji")] public string Emoji;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("maxRealm")] public int? MaxRealm;
        [JsonProperty("element")] public string Element;
        [JsonProperty("baseStats")] public Dictionary<string, double> BaseStats;
        [JsonProperty("elementResists")] public Dictionary<string, double> ElementResists;
        [JsonProperty("baseExpReward")] public int? BaseExpReward;
        [JsonProperty("baseGoldReward")] public int? BaseGoldReward;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("allowedMutations")] public List<string> AllowedMutations;
    }

    public sealed class MutationDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("type")] public string Type;
        [JsonProperty("name")] public string Name;
        [JsonProperty("namePrefix")] public string NamePrefix;
        [JsonProperty("nameSuffix")] public string NameSuffix;
        [JsonProperty("emojiOverride")] public string EmojiOverride;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("maxRealm")] public int? MaxRealm;
        [JsonProperty("weight")] public int? Weight;
        [JsonProperty("exclusive")] public bool? Exclusive;
        [JsonProperty("element")] public string Element;
        [JsonProperty("statModifiers")] public Dictionary<string, double> StatModifiers;
        [JsonProperty("elementResists")] public Dictionary<string, double> ElementResists;
        [JsonProperty("expBonus")] public double? ExpBonus;
        [JsonProperty("goldBonus")] public double? GoldBonus;
        [JsonProperty("lootBonus")] public JToken LootBonus;
        [JsonProperty("regionTags")] public List<string> RegionTags;
    }
}
