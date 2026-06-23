// ============================================================
// Program.cs — 数据层校验入口（dotnet 运行，无需 Unity 编辑器）
// 加载 Assets/StreamingAssets/dlc 下全部 JSON，校验反序列化与计数。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xiuxian.Core;
using Xiuxian.Data;

namespace Xiuxian.LogicTests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            string streaming = args.Length > 0
                ? args[0]
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "StreamingAssets"));

            if (!Directory.Exists(streaming))
            {
                Console.Error.WriteLine($"[FAIL] StreamingAssets 未找到: {streaming}");
                return 1;
            }

            int failures = 0;

            var source = new FileSystemDataSource(streaming);
            var db = new GameDatabase();
            var loader = new DlcLoader(source, db);

            try
            {
                loader.LoadAll();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] 加载异常: {ex}");
                return 1;
            }

            Console.WriteLine($"[OK] 已加载包数: {db.Packs.Count}  ({string.Join(", ", db.EnabledPackIds)})");
            if (db.Packs.Count != 10)
            {
                Console.Error.WriteLine($"[FAIL] 期望加载 10 个包，实际 {db.Packs.Count}");
                failures++;
            }

            PrintTotals(db);

            if (!db.Items.TryGetValue("core:hp_pill", out var hpPill))
            {
                Console.Error.WriteLine("[FAIL] 缺少 core:hp_pill");
                failures++;
            }
            else
            {
                var rng = new Random(1);
                double newHp = hpPill.Effects["hp"].Resolve(10, 200, rng);
                if (Math.Abs(newHp - 60) > 0.001)
                {
                    Console.Error.WriteLine($"[FAIL] core:hp_pill 效果解析错误: 期望 60, 实得 {newHp}");
                    failures++;
                }
                else
                {
                    Console.WriteLine($"[OK] core:hp_pill 效果解析: 10 + 50 = {newHp}");
                }
            }

            failures += ValidateIds(db);
            failures += ValidateDuplicateKeys(db);

            if (failures == 0)
            {
                Console.WriteLine("\n✅ 数据层校验全部通过");
                return 0;
            }

            Console.Error.WriteLine($"\n❌ 校验失败：{failures} 项");
            return 1;
        }

        private static void PrintTotals(GameDatabase db)
        {
            Console.WriteLine("[OK] 实体总数:");
            Console.WriteLine($"    packs={db.Packs.Count}, events={db.Events.Count}, items={db.Items.Count}, monsters={db.Monsters.Count}, monsterTemplates={db.MonsterTemplates.Count}, mutations={db.Mutations.Count}");
            Console.WriteLine($"    techniques={db.Techniques.Count}, techniqueTraits={db.TechniqueTraits.Count}, divineArts={db.DivineArts.Count}, regions={db.Regions.Count}, npcs={db.Npcs.Count}");
            Console.WriteLine($"    quests={db.QuestChains.Count}, dialogues={db.Dialogues.Count}, idleChats={db.IdleChats.Count}, realms={db.Realms.Count}, breakthroughReqs={db.BreakthroughReqs.Count}, tribulations={db.Tribulations.Count}");
            Console.WriteLine($"    equips={db.Equips.Count}, equipTemplates={db.EquipTemplates.Count}, affixes={db.Affixes.Count}, recipes={db.Recipes.Count}, smithing={db.SmithingRecipes.Count}, shop={db.ShopEntries.Count}");
            Console.WriteLine($"    bottlenecks={db.Bottlenecks.Count}, ascensions={db.Ascensions.Count}, bodyRealms={db.BodyRealms.Count}, spiritRootBodyBonuses={db.SpiritRootBodyBonuses.Count}");
            Console.WriteLine($"    eventTemplates={db.EventTemplates.Count}, eventVocab={db.VariablePools.Count}, secretRealms={db.SecretRealms.Count}, sects={db.Sects.Count}, bounties={db.Bounties.Count}, miningSites={db.MiningSites.Count}, auctionLots={db.AuctionLots.Count}");
        }

        private static int ValidateIds(GameDatabase db)
        {
            int failures = 0;
            failures += CheckIds("events", db.Events.Values, x => x.Id);
            failures += CheckIds("items", db.Items.Values, x => x.Id);
            failures += CheckIds("monsters", db.Monsters.Values, x => x.Id);
            failures += CheckIds("monster-templates", db.MonsterTemplates.Values, x => x.Id);
            failures += CheckIds("mutations", db.Mutations.Values, x => x.Id);
            failures += CheckIds("techniques", db.Techniques.Values, x => x.Id);
            failures += CheckIds("technique-traits", db.TechniqueTraits.Values, x => x.Id);
            failures += CheckIds("divine-arts", db.DivineArts.Values, x => x.Id);
            failures += CheckIds("regions", db.Regions.Values, x => x.Id);
            failures += CheckIds("npcs", db.Npcs.Values, x => x.Id);
            failures += CheckIds("quests", db.QuestChains.Values, x => x.Id);
            failures += CheckIds("dialogues", db.Dialogues.Values, x => x.Id);
            failures += CheckIds("realms", db.Realms.Values, x => x.Id);
            failures += CheckIds("breakthrough", db.BreakthroughReqs.Values, x => x.Id);
            failures += CheckIds("tribulations", db.Tribulations.Values, x => x.Id);
            failures += CheckIds("equips", db.Equips.Values, x => x.Id);
            failures += CheckIds("equip-templates", db.EquipTemplates.Values, x => x.Id);
            failures += CheckIds("affixes", db.Affixes.Values, x => x.Id);
            failures += CheckIds("recipes", db.Recipes.Values, x => x.Id);
            failures += CheckIds("smithing", db.SmithingRecipes.Values, x => x.Id);
            failures += CheckIds("bottlenecks", db.Bottlenecks.Values, x => x.Id);
            failures += CheckIds("ascensions", db.Ascensions.Values, x => x.Id);
            failures += CheckIds("body-realms", db.BodyRealms.Values, x => x.Id);
            failures += CheckIds("event-templates", db.EventTemplates.Values, x => x.Id);
            failures += CheckIds("event-vocab", db.VariablePools.Values, x => x.Id);
            failures += CheckIds("secret-realms", db.SecretRealms.Values, x => x.Id);
            failures += CheckIds("sects", db.Sects.Values, x => x.Id);
            failures += CheckIds("bounties", db.Bounties.Values, x => x.Id);
            failures += CheckIds("mining-sites", db.MiningSites.Values, x => x.Id);
            failures += CheckIds("auction-lots", db.AuctionLots.Values, x => x.Id);
            failures += CheckIds("spirit-root-body-bonuses", db.SpiritRootBodyBonuses.Values, x => x.RootType);
            return failures;
        }

        private static int CheckIds<T>(string label, IEnumerable<T> values, Func<T, string> idOf)
        {
            int bad = values.Count(x => string.IsNullOrWhiteSpace(idOf(x)));
            if (bad > 0)
            {
                Console.Error.WriteLine($"[FAIL] {label}: {bad} 条 id 为空");
                return 1;
            }
            Console.WriteLine($"[OK] {label}: id 非空 ({values.Count()})");
            return 0;
        }

        private static int ValidateDuplicateKeys(GameDatabase db)
        {
            var ignored = new HashSet<string> { "spirit-root-body-bonuses", "realms", "breakthrough", "tribulations", "body-realms" };
            foreach (var kv in db.DuplicateKeyCounts.Where(kv => ignored.Contains(kv.Key)))
                Console.WriteLine($"[OK] {kv.Key}: index/key override {kv.Value} 次（与 TS Map.set 一致）");
            var relevant = db.DuplicateKeyCounts.Where(kv => !ignored.Contains(kv.Key)).ToList();
            if (relevant.Count == 0)
            {
                Console.WriteLine("[OK] 未发现实体注册 key 冲突");
                return 0;
            }

            foreach (var kv in relevant)
                Console.Error.WriteLine($"[FAIL] {kv.Key}: 注册 key 冲突 {kv.Value} 次");
            return relevant.Count;
        }
    }
}
