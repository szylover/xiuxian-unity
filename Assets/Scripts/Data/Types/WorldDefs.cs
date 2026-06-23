// ============================================================
// WorldDefs.cs — 区域、NPC、任务、对话、秘境、门派、悬赏、采矿 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class RegionDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("emoji")] public string Emoji;
        [JsonProperty("parentId")] public string ParentId;
        [JsonProperty("isContainer")] public bool? IsContainer;
        [JsonProperty("safeZone")] public bool? SafeZone;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("travelTimeMonths")] public int? TravelTimeMonths;
        [JsonProperty("travelCostBase")] public int? TravelCostBase;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("lootTable")] public List<JToken> LootTable;
        [JsonProperty("combatBonus")] public JToken CombatBonus;
        [JsonProperty("explorationBonus")] public JToken ExplorationBonus;
        [JsonProperty("shopDiscount")] public double? ShopDiscount;
    }

    public sealed class NpcDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("title")] public string Title;
        [JsonProperty("description")] public string Description;
        [JsonProperty("emoji")] public string Emoji;
        [JsonProperty("gender")] public string Gender;
        [JsonProperty("personality")] public string Personality;
        [JsonProperty("disposition")] public string Disposition;
        [JsonProperty("alignment")] public string Alignment;
        [JsonProperty("sectId")] public string SectId;
        [JsonProperty("homeRegionId")] public string HomeRegionId;
        [JsonProperty("realmIndex")] public int? RealmIndex;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("roles")] public List<string> Roles;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("giftPreferences")] public JToken GiftPreferences;
        [JsonProperty("maxAffinity")] public int? MaxAffinity;
        [JsonProperty("affinityDecayRate")] public double? AffinityDecayRate;
        [JsonProperty("karmaAffinityModifier")] public JToken KarmaAffinityModifier;
        [JsonProperty("hp")] public int? Hp;
        [JsonProperty("atk")] public int? Atk;
        [JsonProperty("def")] public int? Def;
        [JsonProperty("speed")] public int? Speed;
        [JsonProperty("critRate")] public double? CritRate;
        [JsonProperty("critDmgMultiplier")] public double? CritDmgMultiplier;
        [JsonProperty("critResist")] public double? CritResist;
        [JsonProperty("charisma")] public int? Charisma;
    }

    public sealed class QuestChainDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("icon")] public string Icon;
        [JsonProperty("category")] public string Category;
        [JsonProperty("condition")] public JToken Condition;
        [JsonProperty("steps")] public List<JToken> Steps;
        [JsonProperty("rewards")] public JToken Rewards;
        [JsonProperty("repeatable")] public bool? Repeatable;
        [JsonProperty("repeatCooldown")] public int? RepeatCooldown;
        [JsonProperty("discoverSource")] public JToken DiscoverSource;
        [JsonProperty("turnInNpcId")] public string TurnInNpcId;
    }

    public sealed class DialogueChainDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("npcId")] public string NpcId;
        [JsonProperty("name")] public string Name;
        [JsonProperty("priority")] public int? Priority;
        [JsonProperty("condition")] public JToken Condition;
        [JsonProperty("nodes")] public List<JToken> Nodes;
        [JsonProperty("startNodeId")] public string StartNodeId;
        [JsonProperty("once")] public bool? Once;
        [JsonProperty("cooldown")] public int? Cooldown;
        [JsonProperty("tags")] public List<string> Tags;
    }

    public sealed class SecretRealmDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("icon")] public string Icon;
        [JsonProperty("regionId")] public string RegionId;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("cooldownMonths")] public int? CooldownMonths;
        [JsonProperty("entryCost")] public JToken EntryCost;
        [JsonProperty("risk")] public JToken Risk;
        [JsonProperty("stages")] public List<JToken> Stages;
        [JsonProperty("completionReward")] public JToken CompletionReward;
    }

    public sealed class SectDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("alignment")] public string Alignment;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("minKarma")] public int? MinKarma;
        [JsonProperty("entryGold")] public int? EntryGold;
        [JsonProperty("founderNpcIds")] public List<string> FounderNpcIds;
        [JsonProperty("ranks")] public List<JToken> Ranks;
        [JsonProperty("facilities")] public List<JToken> Facilities;
        [JsonProperty("missions")] public List<JToken> Missions;
        [JsonProperty("store")] public List<JToken> Store;
    }

    public sealed class BountyTemplateDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("title")] public string Title;
        [JsonProperty("description")] public string Description;
        [JsonProperty("icon")] public string Icon;
        [JsonProperty("issuer")] public string Issuer;
        [JsonProperty("objective")] public JToken Objective;
        [JsonProperty("rewards")] public JToken Rewards;
        [JsonProperty("reputation")] public JToken Reputation;
        [JsonProperty("durationMonths")] public int? DurationMonths;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("maxRealm")] public int? MaxRealm;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("weight")] public int? Weight;
    }

    public sealed class MiningSiteDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("icon")] public string Icon;
        [JsonProperty("regionId")] public string RegionId;
        [JsonProperty("regionTags")] public List<string> RegionTags;
        [JsonProperty("minRealm")] public int? MinRealm;
        [JsonProperty("months")] public int? Months;
        [JsonProperty("staminaCost")] public int? StaminaCost;
        [JsonProperty("baseYields")] public JToken BaseYields;
        [JsonProperty("yields")] public JToken Yields;
        [JsonProperty("fengShui")] public JToken FengShui;
    }
}
