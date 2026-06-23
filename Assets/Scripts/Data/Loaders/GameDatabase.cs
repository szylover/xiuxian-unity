// ============================================================
// GameDatabase.cs — 全局注册表（移植自 registry/stores.ts 的各 Map）
// UnityEngine-free
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Data
{
    /// <summary>数据注册表。内容包通过 DlcLoader 注册；后注册覆盖（TS Map.set）。</summary>
    public sealed class GameDatabase
    {
        public readonly Dictionary<string, DlcPackMeta> Packs = new();
        public readonly Dictionary<string, GameEventDef> Events = new();
        public readonly Dictionary<string, ItemDef> Items = new();
        public readonly Dictionary<string, RecipeDef> Recipes = new();
        public readonly Dictionary<string, EquipDef> Equips = new();
        public readonly Dictionary<string, SmithingRecipeDef> SmithingRecipes = new();
        public readonly Dictionary<int, BreakthroughReqDef> BreakthroughReqs = new();
        public readonly Dictionary<int, TribulationDef> Tribulations = new();
        public readonly Dictionary<string, TechniqueDef> Techniques = new();
        public readonly Dictionary<string, MonsterDef> Monsters = new();
        public readonly Dictionary<string, DivineArtDef> DivineArts = new();
        public readonly Dictionary<int, BodyRealmDef> BodyRealms = new();
        public readonly Dictionary<string, SpiritRootBodyBonusDef> SpiritRootBodyBonuses = new();
        public readonly Dictionary<int, RealmDef> Realms = new();
        public readonly Dictionary<string, RegionDef> Regions = new();
        public readonly Dictionary<string, BottleneckDef> Bottlenecks = new();
        public readonly Dictionary<string, NpcDef> Npcs = new();
        public readonly Dictionary<string, QuestChainDef> QuestChains = new();
        public readonly Dictionary<string, DialogueChainDef> Dialogues = new();
        public readonly Dictionary<string, Newtonsoft.Json.Linq.JToken> IdleChats = new();
        public readonly Dictionary<string, EventTemplateDef> EventTemplates = new();
        public readonly Dictionary<string, VariablePoolDef> VariablePools = new();
        public readonly Dictionary<string, EquipBaseTemplateDef> EquipTemplates = new();
        public readonly Dictionary<string, AffixDef> Affixes = new();
        public readonly Dictionary<string, MonsterTemplateDef> MonsterTemplates = new();
        public readonly Dictionary<string, MutationDef> Mutations = new();
        public readonly Dictionary<string, TechniqueTraitDef> TechniqueTraits = new();
        public readonly Dictionary<string, AscensionDef> Ascensions = new();
        public readonly Dictionary<string, BountyTemplateDef> Bounties = new();
        public readonly Dictionary<string, SecretRealmDef> SecretRealms = new();
        public readonly Dictionary<string, SectDef> Sects = new();
        public readonly Dictionary<string, MiningSiteDef> MiningSites = new();
        public readonly Dictionary<string, AuctionLotDef> AuctionLots = new();
        public readonly Dictionary<string, ShopEntryDef> ShopEntries = new();

        /// <summary>已启用的包 id 集合（按加载顺序）。</summary>
        public readonly List<string> EnabledPackIds = new();

        /// <summary>每种实体加载过程中遇到的重复注册 key 数。</summary>
        public readonly Dictionary<string, int> DuplicateKeyCounts = new();

        public void Clear()
        {
            Packs.Clear(); Events.Clear(); Items.Clear(); Recipes.Clear(); Equips.Clear(); SmithingRecipes.Clear();
            BreakthroughReqs.Clear(); Tribulations.Clear(); Techniques.Clear(); Monsters.Clear(); DivineArts.Clear();
            BodyRealms.Clear(); SpiritRootBodyBonuses.Clear(); Realms.Clear(); Regions.Clear(); Bottlenecks.Clear();
            Npcs.Clear(); QuestChains.Clear(); Dialogues.Clear(); IdleChats.Clear(); EventTemplates.Clear(); VariablePools.Clear();
            EquipTemplates.Clear(); Affixes.Clear(); MonsterTemplates.Clear(); Mutations.Clear(); TechniqueTraits.Clear();
            Ascensions.Clear(); Bounties.Clear(); SecretRealms.Clear(); Sects.Clear(); MiningSites.Clear(); AuctionLots.Clear();
            ShopEntries.Clear(); EnabledPackIds.Clear(); DuplicateKeyCounts.Clear();
        }

        public void RegisterEvent(GameEventDef value) => Register("events", Events, value?.Id, value);
        public void RegisterItem(ItemDef value) => Register("items", Items, value?.Id, value);
        public void RegisterRecipe(RecipeDef value) => Register("recipes", Recipes, value?.Id, value);
        public void RegisterEquip(EquipDef value) => Register("equips", Equips, value?.Id, value);
        public void RegisterSmithingRecipe(SmithingRecipeDef value) => Register("smithing", SmithingRecipes, value?.Id, value);
        public void RegisterTechnique(TechniqueDef value) => Register("techniques", Techniques, value?.Id, value);
        public void RegisterMonster(MonsterDef value) => Register("monsters", Monsters, value?.Id, value);
        public void RegisterDivineArt(DivineArtDef value) => Register("divine-arts", DivineArts, value?.Id, value);
        public void RegisterRealm(RealmDef value) { if (value != null) Register("realms", Realms, value.Index, value); }
        public void RegisterBodyRealm(BodyRealmDef value) { if (value != null) Register("body-realms", BodyRealms, value.Index, value); }
        public void RegisterSpiritRootBodyBonus(SpiritRootBodyBonusDef value) => Register("spirit-root-body-bonuses", SpiritRootBodyBonuses, value?.RootType, value);
        public void RegisterRegion(RegionDef value) => Register("regions", Regions, value?.Id, value);
        public void RegisterBottleneck(BottleneckDef value) => Register("bottlenecks", Bottlenecks, value?.Id, value);
        public void RegisterNpc(NpcDef value) => Register("npcs", Npcs, value?.Id, value);
        public void RegisterQuestChain(QuestChainDef value) => Register("quests", QuestChains, value?.Id, value);
        public void RegisterDialogue(DialogueChainDef value) => Register("dialogues", Dialogues, value?.Id, value);
        public void RegisterIdleChat(string key, Newtonsoft.Json.Linq.JToken value) { if (value != null && !string.IsNullOrEmpty(key)) IdleChats[key] = value; }
        public void RegisterEventTemplate(EventTemplateDef value) => Register("event-templates", EventTemplates, value?.Id, value);
        public void RegisterVariablePool(VariablePoolDef value) => Register("event-vocab", VariablePools, value?.Id, value);
        public void RegisterEquipTemplate(EquipBaseTemplateDef value) => Register("equip-templates", EquipTemplates, value?.Id, value);
        public void RegisterAffix(AffixDef value) => Register("affixes", Affixes, value?.Id, value);
        public void RegisterMonsterTemplate(MonsterTemplateDef value) => Register("monster-templates", MonsterTemplates, value?.Id, value);
        public void RegisterMutation(MutationDef value) => Register("mutations", Mutations, value?.Id, value);
        public void RegisterTechniqueTrait(TechniqueTraitDef value) => Register("technique-traits", TechniqueTraits, value?.Id, value);
        public void RegisterAscension(AscensionDef value) => Register("ascensions", Ascensions, value?.Id, value);
        public void RegisterBounty(BountyTemplateDef value) => Register("bounties", Bounties, value?.Id, value);
        public void RegisterSecretRealm(SecretRealmDef value) => Register("secret-realms", SecretRealms, value?.Id, value);
        public void RegisterSect(SectDef value) => Register("sects", Sects, value?.Id, value);
        public void RegisterMiningSite(MiningSiteDef value) => Register("mining-sites", MiningSites, value?.Id, value);
        public void RegisterAuctionLot(AuctionLotDef value) => Register("auction-lots", AuctionLots, value?.Id, value);
        public void RegisterShopEntry(ShopEntryDef value)
        {
            if (value == null || string.IsNullOrEmpty(value.ItemId)) return;
            Register("shop", ShopEntries, $"{value.NpcId ?? "*"}|{value.ItemId}", value);
        }
        public void RegisterBreakthroughReq(BreakthroughReqDef value) { if (value != null) Register("breakthrough", BreakthroughReqs, value.ToRealmIndex, value); }
        public void RegisterTribulation(TribulationDef value) { if (value != null) Register("tribulations", Tribulations, value.ForRealmIndex, value); }

        private void Register<T>(string type, Dictionary<string, T> registry, string key, T value) where T : class
        {
            if (value == null || string.IsNullOrEmpty(key)) return;
            if (registry.ContainsKey(key)) CountDuplicate(type);
            registry[key] = value;
        }

        private void Register<T>(string type, Dictionary<int, T> registry, int key, T value) where T : class
        {
            if (value == null) return;
            if (registry.ContainsKey(key)) CountDuplicate(type);
            registry[key] = value;
        }

        private void CountDuplicate(string type)
        {
            DuplicateKeyCounts.TryGetValue(type, out var count);
            DuplicateKeyCounts[type] = count + 1;
        }
    }
}
