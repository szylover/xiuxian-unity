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
using Xiuxian.Systems;

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
            failures += ValidateCoreCultivationSystems(db);
            failures += ValidateCombatSystems(db);
            failures += ValidateEconomySystems(db);

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

        private static int ValidateCoreCultivationSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions
            {
                Name = "Test",
                Gender = "male",
                Appearance = 1,
            }, new SystemRandomRng(1234));

            var realm0 = db.Realms[0];
            failures += Expect("player.start.realm", player.RealmIndex == 0);
            failures += Expect("player.start.hp", player.MaxHp == (realm0.HpBase ?? 0) && player.Hp == player.MaxHp);
            failures += Expect("player.start.atk", player.Atk == (realm0.AtkBase ?? 0));
            failures += Expect("player.start.spiritRoots", player.SpiritRoots != null && player.SpiritRoots.Combo != null);

            int expBefore = player.Exp;
            var gain = CultivationSystem.GainCultivation(db, player);
            failures += Expect("cultivation.gain", gain.ExpGain > 0 && gain.Player.Exp > expBefore);
            var bodyGain = BodyCultivationSystem.GainBodyRealmExp(db, player, 100);
            failures += Expect("bodyCultivation.gain", bodyGain.ActualGain > 0 && bodyGain.Player.BodyRealmExp > 0);

            var successPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Success", Gender = "male", Appearance = 1 }, new SystemRandomRng(7));
            successPlayer.Exp = db.Realms[1].ExpReq ?? 100;
            var success = BreakthroughSystem.AttemptBreakthrough(db, successPlayer, new FixedRng(0.0));
            failures += Expect("breakthrough.success.lowRoll", success.Success && success.Player.RealmIndex == 1);

            var failPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Fail", Gender = "female", Appearance = 2 }, new SystemRandomRng(8));
            failPlayer.Exp = db.Realms[1].ExpReq ?? 100;
            var fail = BreakthroughSystem.AttemptBreakthrough(db, failPlayer, new FixedRng(0.999));
            failures += Expect("breakthrough.fail.highRoll", !fail.Success && fail.Player.RealmIndex == 0 && fail.Player.Breakthrough.FailedAttempts.ContainsKey(0));

            var bottleneckPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "BN", Gender = "male", Appearance = 1 }, new SystemRandomRng(9));
            bottleneckPlayer.RealmIndex = 3;
            var check = BottleneckSystem.CheckBottleneck(db, bottleneckPlayer, "realm", bottleneckPlayer.RealmIndex, null);
            failures += Expect("bottleneck.check", check.Blocked && check.IsNewlyActivated);
            BottleneckSystem.ActivateBottleneck(db, bottleneckPlayer, check.Def.Id);
            failures += Expect("bottleneck.activate", bottleneckPlayer.Bottleneck.Active.ContainsKey(check.Def.Id));
            BottleneckSystem.UnlockBottleneck(db, bottleneckPlayer, check.Def.Id, "persistence");
            failures += Expect("bottleneck.unlock", bottleneckPlayer.Bottleneck.Unlocked.ContainsKey(check.Def.Id) && !bottleneckPlayer.Bottleneck.Active.ContainsKey(check.Def.Id));

            var tribPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Trib", Gender = "female", Appearance = 1 }, new SystemRandomRng(10));
            tribPlayer.RealmIndex = 5;
            PlayerStatsSystem.RecalcStats(db, tribPlayer);
            tribPlayer.Hp = tribPlayer.MaxHp;
            var trib = TribulationSystem.RunTribulation(db, tribPlayer, new RealTribulationCombatResolver(db), new FixedRng(0.5));
            failures += Expect("tribulation.realResolver", trib.TotalWaves > 0 && (trib.Success || trib.WavesCleared < trib.TotalWaves));

            Console.WriteLine("[OK] 核心修炼系统校验完成");
            return failures;
        }

        private static int ValidateCombatSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Combat", Gender = "male", Appearance = 1 }, new SystemRandomRng(11));
            player.RealmIndex = 1;
            PlayerStatsSystem.RecalcStats(db, player);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.Stamina = player.MaxStamina;

            var combat = CombatEngine.RunCombat(db, player, db.Monsters["cp-01:ink_jiao"], new FixedRng(0.5));
            failures += Expect("combat.knownMonster.winner", combat.Winner == "player" && combat.PlayerHpLeft < combat.PlayerMaxHp && combat.ExpGained > 0);

            var waterArt = db.DivineArts["cp-01:divine_ice_cone"];
            player.Aptitudes.Water = 100;
            var pc = CombatantFactory.FromPlayer(db, player);
            var fireMonster = CombatantFactory.FromMonster(db.Monsters["cp-01:flame_bird"]);
            fireMonster.Element = ElementType.Fire;
            var neutralMonster = CombatantFactory.FromMonster(db.Monsters["cp-01:flame_bird"]);
            neutralMonster.Element = ElementType.Earth;
            int counterDamage = DivineArtSystem.CalcDamage(player, pc, waterArt, fireMonster, fireMonster.Hp, new FixedRng(0.5)).Damage;
            int neutralDamage = DivineArtSystem.CalcDamage(player, pc, waterArt, neutralMonster, neutralMonster.Hp, new FixedRng(0.5)).Damage;
            failures += Expect("combat.elementCounter.damage", counterDamage > neutralDamage);

            DivineArtSystem.Learn(db, player, "cp-01:divine_ice_cone");
            DivineArtSystem.Activate(db, player, "cp-01:divine_ice_cone");
            player.Hp = player.MaxHp - 30;
            player.Mp = player.MaxMp;
            var artResult = CombatEngine.RunCombat(db, player, CombatantFactory.FromMonster(db.Monsters["cp-01:giant_scorpion"]), new SequenceRng(0.1, 0.9, 0.9, 0.9, 0.9, 0.9), false);
            failures += Expect("combat.divineArt.effect", artResult.MpUsed >= waterArt.MpCost && artResult.Logs.Any(l => l.Contains("攻击降低") || l.Contains("回复体力")));

            var equipPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Equip", Gender = "female", Appearance = 1 }, new SystemRandomRng(12));
            var generated = EquipmentSystem.GenerateEquip(db, equipPlayer, new FixedRng(0.1), new GenerateEquipOptions { SlotFilter = "weapon", ForcedQuality = "treasure", Seed = 99 });
            InventorySystem.AddItem(equipPlayer, generated.InstanceId, 1);
            int atkBefore = equipPlayer.Atk;
            var eq = EquipmentSystem.EquipItem(db, equipPlayer, generated.InstanceId);
            var baseTemplate = db.EquipTemplates[generated.BaseTemplateId];
            failures += Expect("equipment.affix.aggregate", eq.Success && equipPlayer.Atk > atkBefore + (int)Math.Floor(baseTemplate.BaseStats.GetValueOrDefault("atk") * 2.5));

            var deathPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Death", Gender = "male", Appearance = 1 }, new SystemRandomRng(13));
            deathPlayer.Exp = 1000; deathPlayer.Gold = 1000; deathPlayer.Hp = 0;
            var deathCheck = DeathSystem.CheckDeathTriggers(db, deathPlayer, new DeathContext { Source = "combat" });
            var death = DeathSystem.ApplyDeath(db, deathPlayer, deathCheck.Trigger, new FixedRng(0.5));
            failures += Expect("death.penalty.light", deathCheck.Triggered && !death.GameOver && death.Player.Exp == 900 && death.Player.Gold == 900 && death.Player.Health <= 80);

            var tribPlayer = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "RealTrib", Gender = "female", Appearance = 2 }, new SystemRandomRng(14));
            tribPlayer.RealmIndex = 5;
            PlayerStatsSystem.RecalcStats(db, tribPlayer);
            tribPlayer.Hp = tribPlayer.MaxHp;
            var trib = TribulationSystem.RunTribulation(db, tribPlayer, null, new FixedRng(0.5));
            failures += Expect("tribulation.default.realResolver", trib.TotalWaves > 0 && (trib.Success || trib.WavesCleared < trib.TotalWaves));

            Console.WriteLine($"[OK] 战斗摘要: winner={combat.Winner}, hp={combat.PlayerHpLeft}/{combat.PlayerMaxHp}, exp={combat.ExpGained}, element={counterDamage}>{neutralDamage}, artMp={artResult.MpUsed}, generatedEquip={generated.FinalName}, tribSuccess={trib.Success}, waves={trib.WavesCleared}/{trib.TotalWaves}");
            return failures;
        }

        private static int ValidateEconomySystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Economy", Gender = "male", Appearance = 1 }, new SystemRandomRng(21));
            player.Gold = 500;
            player.MentalPower = player.MaxMentalPower = 100;
            player.Aptitudes.Alchemy = 0;
            player.Aptitudes.Smithing = 100;
            player.Aptitudes.Mining = 0;
            player.Aptitudes.Fengshui = 0;
            player.Luck = 0;
            player.Charisma = 0;
            player.Systems["learning"] = new LearningState
            {
                LearnedRecipes = new[] { "core:recipe_hp_pill" },
                LearnedSmithingRecipes = new[] { "core:smith_iron_sword" }
            };

            var add1 = InventorySystem.AddItem(db, player, "core:herb_lingzhi", 150);
            var add2 = InventorySystem.AddItem(db, player, "core:herb_lingzhi", 60);
            failures += Expect("economy.inventory.stack", add1.Added == 150 && add2.Added == 60 && InventorySystem.CountItem(player, "core:herb_lingzhi") == 210 && InventorySystem.GetUsedSlots(player) >= 2);
            InventorySystem.RemoveItem(player, "core:herb_lingzhi", 10);
            failures += Expect("economy.inventory.remove", InventorySystem.CountItem(player, "core:herb_lingzhi") == 200);
            InventorySystem.AddGold(player, 25);
            var spend = InventorySystem.SpendGold(player, 30);
            failures += Expect("economy.currency", spend.Success && player.Gold == 495);

            int herbBefore = InventorySystem.CountItem(player, "core:herb_lingzhi");
            int mentalBefore = player.MentalPower;
            var alcOk = AlchemySystem.PerformAlchemy(db, player, "core:recipe_hp_pill", new SequenceRng(0.0, 0.9));
            failures += Expect("economy.alchemy.success", alcOk.Success && alcOk.Quality == "normal" && InventorySystem.CountItem(player, "core:hp_pill") == 2 && InventorySystem.CountItem(player, "core:herb_lingzhi") == herbBefore - 2 && player.MentalPower == mentalBefore - 5);
            InventorySystem.AddItem(db, player, "core:herb_lingzhi", 2);
            mentalBefore = player.MentalPower;
            int pillBefore = InventorySystem.CountItem(player, "core:hp_pill");
            var alcFail = AlchemySystem.PerformAlchemy(db, player, "core:recipe_hp_pill", new FixedRng(0.999));
            failures += Expect("economy.alchemy.failConsumes", !alcFail.Success && InventorySystem.CountItem(player, "core:hp_pill") == pillBefore && player.MentalPower == mentalBefore - 5);

            InventorySystem.AddItem(db, player, "core:iron_ore", 5);
            int goldBeforeSmith = player.Gold;
            var smith = SmithingSystem.PerformSmithing(db, player, "core:smith_iron_sword", new FixedRng(0.0));
            failures += Expect("economy.smithing.forge", smith.Success && InventorySystem.CountItem(player, "core:iron_sword") == 1 && InventorySystem.CountItem(player, "core:iron_ore") == 0 && player.Gold == goldBeforeSmith - 10);
            var equip = EquipmentSystem.EquipItem(db, player, "core:iron_sword");
            failures += Expect("economy.smithing.equip", equip.Success && player.Equipped.Weapon == "core:iron_sword");

            int goldBeforeBuy = player.Gold;
            var buy = ShopSystem.BuyItem(db, player, "core:hp_pill", 1);
            failures += Expect("economy.shop.buy", buy.Success && player.Gold == goldBeforeBuy - ShopSystem.CalcKarmaBuyPrice(20, player.Charisma, player.Karma) && InventorySystem.CountItem(player, "core:hp_pill") == pillBefore + 1);
            int sellPrice = db.Items["core:hp_pill"].SellPrice;
            var sell = ShopSystem.SellItem(db, player, "core:hp_pill", 1);
            failures += Expect("economy.shop.sell", sell.Success && player.Gold == goldBeforeBuy - 20 + sellPrice);

            player.Gold = 1000;
            var auctionReady = AuctionSystem.RefreshAuctionHouse(db, player, new FixedRng(0.99));
            var lot = AuctionSystem.GetAuctionState(player).Lots.First();
            int bid = AuctionSystem.GetNextBid(lot.CurrentBid);
            int goldBeforeBid = player.Gold;
            var bidResult = AuctionSystem.PlaceAuctionBid(db, player, lot.Id, new FixedRng(0.99), bid);
            player.Age += AuctionSystem.RefreshMonths;
            var settle = AuctionSystem.SettleDueAuctions(db, player, new FixedRng(0.99));
            failures += Expect("economy.auction.settle", bidResult.Logs.Contains("playerBid") && settle.Logs.Contains("winLot") && InventorySystem.CountItem(player, lot.ItemId) >= lot.Count && player.Gold == goldBeforeBid - bid);

            player.Stamina = player.MaxStamina = 100;
            player.InventoryCapacity = 50;
            var mining = MiningSystem.PerformMining(db, player, "core:qingyun_backhill_vein", new SequenceRng(0.0, 0.9, 0.0, 0.0, 0.9, 0.0, 0.0, 0.9, 0.0));
            failures += Expect("economy.mining.yields", mining.Yields.TryGetValue("core:iron_ore", out var ore) && ore == 3 && player.Stamina == 88 && MiningSystem.GetMiningState(player).MinedCount == 1);

            Console.WriteLine($"[OK] 经济摘要: pills={InventorySystem.CountItem(player, "core:hp_pill")}, gold={player.Gold}, auctionLot={lot.ItemId}x{lot.Count}, minedOre={ore}, slots={InventorySystem.GetUsedSlots(player)}");
            return failures;
        }

        private static int Expect(string label, bool condition)
        {
            if (condition)
            {
                Console.WriteLine($"[OK] {label}");
                return 0;
            }
            Console.Error.WriteLine($"[FAIL] {label}");
            return 1;
        }

        private sealed class SequenceRng : IRng
        {
            private readonly double[] values;
            private int index;
            public SequenceRng(params double[] values) => this.values = values;
            public double NextDouble()
            {
                if (values.Length == 0) return 0.5;
                double value = values[Math.Min(index, values.Length - 1)];
                index++;
                return value;
            }
            public int NextIntInclusive(int min, int max) => min + (int)Math.Floor(NextDouble() * (max - min + 1));
        }
    }
}
