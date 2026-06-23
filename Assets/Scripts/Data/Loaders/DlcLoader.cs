// ============================================================
// DlcLoader.cs — DLC 加载器（移植自各 loader.ts + index.ts 注册流程）
// 数据驱动：扫描 dlc/ 下各包，加载存在的实体 JSON 文件。
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xiuxian.Core;

namespace Xiuxian.Data
{
    public sealed class DlcLoader
    {
        private readonly IDataSource _source;
        private readonly GameDatabase _db;

        public DlcLoader(IDataSource source, GameDatabase db)
        {
            _source = source;
            _db = db;
        }

        /// <summary>加载全部存在的包（core 优先，其余按名称）。</summary>
        public void LoadAll()
        {
            var packages = new List<string>(_source.ListPackages());
            packages.Sort(ComparePackages);
            foreach (var pkg in packages)
                LoadPackage(pkg);
        }

        /// <summary>加载单个包目录下已知的实体文件。</summary>
        public void LoadPackage(string package)
        {
            var meta = new DlcPackMeta(package) { Id = package };
            _db.Packs[package] = meta;
            _db.EnabledPackIds.Add(package);

            LoadList<GameEventDef>(package, "events.json", _db.RegisterEvent);
            LoadList<ItemDef>(package, "items.json", _db.RegisterItem);
            LoadList<MonsterDef>(package, "monsters.json", _db.RegisterMonster);
            LoadList<MonsterTemplateDef>(package, "monster-templates.json", _db.RegisterMonsterTemplate);
            LoadList<MutationDef>(package, "mutations.json", _db.RegisterMutation);
            LoadList<TechniqueDef>(package, "techniques.json", _db.RegisterTechnique);
            LoadList<TechniqueTraitDef>(package, "technique-traits.json", _db.RegisterTechniqueTrait);
            LoadList<DivineArtDef>(package, "divine-arts.json", _db.RegisterDivineArt);
            LoadList<RegionDef>(package, "regions.json", _db.RegisterRegion);
            LoadList<NpcDef>(package, "npcs.json", _db.RegisterNpc);
            LoadList<QuestChainDef>(package, "quests.json", _db.RegisterQuestChain);
            LoadList<RealmDef>(package, "realms.json", _db.RegisterRealm);
            LoadBreakthrough(package);
            LoadList<RecipeDef>(package, "recipes.json", _db.RegisterRecipe);
            LoadList<SmithingRecipeDef>(package, "smithing.json", _db.RegisterSmithingRecipe);
            LoadList<EquipDef>(package, "equips.json", _db.RegisterEquip);
            LoadList<EquipBaseTemplateDef>(package, "equip-templates.json", _db.RegisterEquipTemplate);
            LoadList<AffixDef>(package, "affixes.json", _db.RegisterAffix);
            LoadList<BottleneckDef>(package, "bottlenecks.json", _db.RegisterBottleneck);
            LoadList<AscensionDef>(package, "ascensions.json", _db.RegisterAscension);
            LoadList<SecretRealmDef>(package, "secret-realms.json", _db.RegisterSecretRealm);
            LoadList<SectDef>(package, "sects.json", _db.RegisterSect);
            LoadList<BountyTemplateDef>(package, "bounties.json", _db.RegisterBounty);
            LoadList<MiningSiteDef>(package, "mining-sites.json", _db.RegisterMiningSite);
            LoadList<AuctionLotDef>(package, "auction-lots.json", _db.RegisterAuctionLot);
            LoadList<EventTemplateDef>(package, "event-templates.json", _db.RegisterEventTemplate);
            LoadList<VariablePoolDef>(package, "event-vocab.json", _db.RegisterVariablePool);
            LoadList<ShopEntryDef>(package, "shop.json", _db.RegisterShopEntry);
            LoadBodyConfig(package);
            LoadDialogues(package);
        }

        private void LoadList<T>(string package, string fileName, Action<T> register)
        {
            if (!_source.Exists(package, fileName)) return;
            var values = Deserialize<List<T>>(_source.ReadText(package, fileName));
            if (values == null) return;
            foreach (var value in values) register(value);
        }

        private void LoadBreakthrough(string package)
        {
            if (!_source.Exists(package, "breakthrough.json")) return;
            var data = Deserialize<BreakthroughDataDef>(_source.ReadText(package, "breakthrough.json"));
            if (data?.BreakthroughReqs != null)
                foreach (var value in data.BreakthroughReqs) _db.RegisterBreakthroughReq(value);
            if (data?.Tribulations != null)
                foreach (var value in data.Tribulations) _db.RegisterTribulation(value);
        }

        private void LoadBodyConfig(string package)
        {
            if (!_source.Exists(package, "body-config.json")) return;
            var data = Deserialize<BodyConfigDef>(_source.ReadText(package, "body-config.json"));
            if (data?.BodyRealms != null)
                foreach (var value in data.BodyRealms) _db.RegisterBodyRealm(value);
            if (data?.SpiritRootBodyBonuses != null)
                foreach (var value in data.SpiritRootBodyBonuses) _db.RegisterSpiritRootBodyBonus(value);
        }

        private void LoadDialogues(string package)
        {
            var files = new List<string>(_source.ListFiles(package, "dialogues"));
            files.Sort(StringComparer.Ordinal);
            foreach (var file in files)
            {
                var text = _source.ReadText(package, Path.Combine("dialogues", file));
                var token = JToken.Parse(text);
                if (token.Type == JTokenType.Array)
                {
                    var values = token.ToObject<List<DialogueChainDef>>();
                    if (values == null) continue;
                    foreach (var value in values) _db.RegisterDialogue(value);
                }
                else if (token.Type == JTokenType.Object)
                {
                    _db.RegisterIdleChat(package + ":" + file, token);
                }
            }
        }

        private static T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);

        // core 永远最先加载（对应 index.ts 中 core 为必需基底包）
        private static int ComparePackages(string a, string b)
        {
            if (a == "core") return -1;
            if (b == "core") return 1;
            return string.CompareOrdinal(a, b);
        }
    }
}
