// ============================================================
// Program.cs — 数据层校验入口（dotnet 运行，无需 Unity 编辑器）
// 加载 Assets/StreamingAssets/dlc 下全部 JSON，校验反序列化与计数。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;
using Xiuxian.Systems.Procedural;

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
            failures += ValidateWorldSocialSystems(db);
            failures += ValidateAdvancedProgressionSystems(db);
            failures += ValidateProceduralSystems(db);
            failures += ValidateSaveLoadSystems(db);

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

        private static int ValidateWorldSocialSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "World", Gender = "female", Appearance = 2 }, new SystemRandomRng(31));
            player.Gold = 500;
            player.Stamina = player.MaxStamina = 200;
            player.InventoryCapacity = 50;

            db.Events["test:eligible_daily"] = new GameEventDef { Id = "test:eligible_daily", Category = "test", Tone = "good", Name = "eligible", Weight = 1, Effects = new Dictionary<string, EffectValue> { ["gold"] = EffectValue.FromScalar(10) }, Message = "eligible", Condition = JObject.FromObject(new { minRealm = 0 }) };
            db.Events["test:gated_daily"] = new GameEventDef { Id = "test:gated_daily", Category = "test", Tone = "good", Name = "gated", Weight = 999, Effects = new Dictionary<string, EffectValue> { ["gold"] = EffectValue.FromScalar(999) }, Message = "gated", Condition = JObject.FromObject(new { minRealm = 99 }) };
            int goldBeforeEvent = player.Gold;
            var available = EventSystem.GetAvailableEvents(db, player, "test");
            var ev = EventSystem.TriggerEvent(db, player, "test", new FixedRng(0));
            failures += Expect("world.event.gatedExcluded", available.Any(e => e.Id == "test:eligible_daily") && available.All(e => e.Id != "test:gated_daily"));
            failures += Expect("world.event.applyEffect", ev != null && ev.EventId == "test:eligible_daily" && player.Gold == goldBeforeEvent + 10);

            var meet = NpcSystem.MeetNpc(db, player, "core:npc_qingyun_elder");
            var dlgStart = DialogueSystem.StartDialogue(db, player, "core:dlg_elder_first_meet");
            int pillsBeforeDialogue = InventorySystem.CountItem(player, "core:hp_pill");
            var dlgChoice = DialogueSystem.SelectChoice(db, player, "core:dlg_elder_first_meet", "n1", "c1");
            failures += Expect("world.dialogue.branchEffect", dlgStart.Node != null && dlgChoice.Node != null && NpcSystem.GetRelation(player, "core:npc_qingyun_elder").Affinity >= 10 && InventorySystem.CountItem(player, "core:hp_pill") == pillsBeforeDialogue + 2);
            failures += Expect("world.npc.relation", meet.Success && NpcSystem.ChangeAffinity(db, player, "core:npc_qingyun_elder", 3).AffinityChange == 3);

            player.RealmIndex = 1;
            PlayerStatsSystem.RecalcStats(db, player);
            player.Stamina = player.MaxStamina;
            var travel = MapSystem.TravelTo(db, player, "core:beast_mountain");
            failures += Expect("world.map.travel", travel.Success && MapSystem.GetMapState(db, player).CurrentRegionId == "core:beast_mountain");

            var discover = QuestSystem.CheckQuestDiscovery(db, player, new QuestTrigger { Type = "talk_npc", NpcId = "core:npc_beast_hunter" });
            var accept = QuestSystem.AcceptQuest(db, player, "core:quest_wolf_bounty");
            int goldBeforeQuest = player.Gold;
            for (int i = 0; i < 5; i++) QuestSystem.TickQuestObjectives(db, player, new QuestTrigger { Type = "kill_monster", MonsterId = "core:wild_wolf" });
            var turnIn = QuestSystem.TurnInQuest(db, player, "core:quest_wolf_bounty");
            failures += Expect("world.quest.completeReward", discover.Logs.Count > 0 && accept.Success && turnIn.Success && QuestSystem.GetQuestState(player).CompletedQuests.ContainsKey("core:quest_wolf_bounty") && player.Gold > goldBeforeQuest);

            player.Gold = 500;
            player.Stamina = player.MaxStamina = 200;
            var join = SectSystem.JoinSect(db, player, "core:qingyun_sect");
            var mission = SectSystem.CompleteSectMission(db, player, "qingyun_patrol");
            failures += Expect("world.sect.joinContribution", join.Success && mission.Success && SectSystem.GetSectState(player).Contribution >= 25);

            player.RealmIndex = 6;
            PlayerStatsSystem.RecalcStats(db, player);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.Stamina = player.MaxStamina = 300;
            player.Gold = 1000;
            MapSystem.GetMapState(db, player).CurrentRegionId = "core:desolate_waste";
            int expBeforeRealm = player.Exp;
            var realmRun = SecretRealmSystem.RunToEnd(db, player, "core:waste_sand_palace", new FixedRng(0.1));
            failures += Expect("world.secretRealm.completeReward", realmRun.Success && player.Exp > expBeforeRealm && SecretRealmSystem.GetSecretRealmState(player).CompletedRuns.ContainsKey("core:waste_sand_palace"));

            MapSystem.GetMapState(db, player).CurrentRegionId = "core:qingyun_town";
            BountySystem.EnsureBountyBoard(db, player, new FixedRng(0), true);
            var bounty = BountySystem.GetBountyState(player).Available.First();
            var bountyAccept = BountySystem.AcceptBounty(db, player, bounty.Id, new FixedRng(0));
            var obj = bounty.Objective;
            string objectiveType = obj.Value<string>("type");
            string target = obj.Value<string>("targetId");
            int count = obj.Value<int?>("count") ?? 1;
            if (objectiveType == "collect_item") InventorySystem.AddItem(db, player, target, count);
            else if (objectiveType == "reach_region") MapSystem.GetMapState(db, player).CurrentRegionId = target;
            for (int i = 0; i < count; i++)
                BountySystem.TickBountyObjectives(db, player, new QuestTrigger { Type = objectiveType == "kill_monster" ? "kill_monster" : objectiveType == "reach_region" ? "reach_region" : "item_change", MonsterId = target, RegionId = target });
            int goldBeforeBounty = player.Gold;
            var claim = BountySystem.ClaimBounty(db, player, bounty.Id);
            failures += Expect("world.bounty.acceptCompleteReward", bountyAccept.Success && claim.Success && player.Gold >= goldBeforeBounty && BountySystem.GetBountyState(player).Completed.ContainsKey(bounty.Id));

            Console.WriteLine($"[OK] 世界社交摘要: event={ev.EventId}, questGold={player.Gold}, region={MapSystem.GetMapState(db, player).CurrentRegionId}, sectContribution={SectSystem.GetSectState(player).Contribution}, realmRuns={SecretRealmSystem.GetSecretRealmState(player).CompletedRuns.Count}, bountyRep={BountySystem.GetBountyState(player).Reputation}");
            return failures;
        }

        private static int ValidateAdvancedProgressionSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Progression", Gender = "female", Appearance = 2 }, new SystemRandomRng(41));
            player.InventoryCapacity = 200;
            PlayerStatsSystem.RecalcStats(db, player);
            int atkBefore = player.Atk;
            var destiny = DestinySystem.EnsureDestiny(player, new FixedRng(0.0));
            failures += Expect("progression.destiny.passive", destiny.Success && player.DestinyId == "core:lone_star" && player.Atk > atkBefore && player.Passives.ContainsKey("progression.destiny"));

            var insightGain = EnlightenmentSystem.GainComprehension(player, 120);
            var insight = EnlightenmentSystem.ContemplateInsight(player);
            failures += Expect("progression.enlightenment.insight", insightGain.Success && insight.Success && EnlightenmentSystem.GetState(player).UnlockedInsightIds.Count >= 1);

            var karma = KarmaSystem.ChangeKarma(player, 45, "test", "major");
            failures += Expect("progression.karma.band", karma.Alignment == "righteous" && karma.Title == "righteousCultivator" && KarmaSystem.GetState(player).MajorEvents.Contains("major"));

            player.RealmIndex = 4; PlayerStatsSystem.RecalcStats(db, player); player.Hp = player.MaxHp; player.Mp = player.MaxMp; player.Stamina = player.MaxStamina;
            HeartDemonSystem.AddHeartDemon(player, 95, "test");
            var demon = HeartDemonSystem.TryHeartDemonTribulation(db, player, new FixedRng(0.1), true);
            failures += Expect("progression.heartDemon.combat", demon.Triggered && demon.Logs.Count > 0 && (HeartDemonSystem.GetState(player).ConqueredCount + HeartDemonSystem.GetState(player).FailedCount) >= 1);

            RankingSystem.Refresh(db, player, true);
            var rank = RankingSystem.GetState(player).Snapshots["core:combat_power"];
            failures += Expect("progression.ranking.sort", rank.Entries.Count > 1 && rank.Entries.SequenceEqual(rank.Entries.OrderBy(e => e.Rank)));

            player.Age += 2; player.Hp = player.MaxHp; player.Mp = player.MaxMp; player.Stamina = player.MaxStamina;
            var candidate = PvpSystem.GetCandidates(db, player).First();
            var pvp = PvpSystem.Challenge(db, player, candidate.Id, new FixedRng(0.1));
            failures += Expect("progression.pvp.resolves", pvp.CombatResult != null && !string.IsNullOrEmpty(pvp.CombatResult.Winner) && PvpSystem.GetState(player).Records.Count == 1);

            var old = player;
            old.RealmIndex = 7; InventorySystem.AddItem(db, old, ReincarnationSystem.ReincarnationOrbId, 1);
            var reinc = ReincarnationSystem.Perform(db, old, "voluntary", new SystemRandomRng(42));
            failures += Expect("progression.reincarnation.carry", reinc.NewPlayer != old && ReincarnationSystem.GetState(reinc.NewPlayer).Count == 1 && reinc.NewPlayer.DestinyId == old.DestinyId);

            var ascBlocked = AscensionSystem.GetStatus(db, player);
            player.RealmIndex = 7; player.Exp = 500000; player.Tracking.DefeatedHigherRealm = true; InventorySystem.AddItem(db, player, "cp-03:ascension_token", 1); InventorySystem.AddItem(db, player, "cp-03:immortal_dew", 2);
            var ascReady = AscensionSystem.GetStatus(db, player);
            var ascDone = AscensionSystem.ApplySuccess(db, player, ascReady.AscDef);
            failures += Expect("progression.ascension.blockReadyDone", !ascBlocked.CanAscend && ascReady.CanAscend && ascDone.Success && AscensionSystem.GetState(player).HasAscended);

            player.RealmIndex = 15; player.Exp = 520000000; player.Karma = 0; InventorySystem.AddItem(db, player, "cp-04:primordial_jade", 3);
            var endgame = PrimordialEndgameSystem.Attempt(db, player, new FixedRng(0.1), true);
            failures += Expect("progression.endgame.complete", endgame.Completed && PrimordialEndgameSystem.GetState(player).CompletedId != null);

            InventorySystem.AddItem(db, player, "core:scroll_technique_basic_sword", 1);
            var studyStart = LearningSystem.StartStudy(db, player, "core:scroll_technique_basic_sword");
            var studyTick = LearningSystem.TickStudy(db, player, 99);
            failures += Expect("progression.learning.technique", studyStart.Success && studyTick.Completed && player.Techniques.Any(t => t.TechniqueId == studyStart.Message));

            player.Tracking.KillCount = 1;
            var ach = AchievementSystem.CheckAchievements(player);
            failures += Expect("progression.achievement.unlock", ach.Success && AchievementSystem.GetState(player).UnlockedIds.Contains("core:first_blood"));

            var chronicle = ChronicleSystem.CreateIncarnation(db, player, ChronicleSystem.Empty());
            ChronicleSystem.AddEvent(chronicle, new ChronicleEvent { Type = "achievement_unlocked", Year = player.GameYear, Month = player.GameMonth, Description = "test" });
            failures += Expect("progression.chronicle.entry", chronicle.Current != null && chronicle.Current.Events.Count == 1);

            Console.WriteLine($"[OK] 进阶机制摘要: destiny={player.DestinyId}, insight={EnlightenmentSystem.GetState(player).UnlockedInsightIds.Count}, karma={player.Karma}/{karma.Alignment}, heartDemon={HeartDemonSystem.GetState(player).Value}, rank={rank.PlayerRank}, pvp={pvp.CombatResult.Winner}, reinc={ReincarnationSystem.GetState(reinc.NewPlayer).Count}, ascended={AscensionSystem.GetState(player).HasAscended}, endgame={PrimordialEndgameSystem.GetState(player).CompletedId}, techniques={player.Techniques.Count}, achievements={AchievementSystem.GetState(player).UnlockedIds.Count}, chronicleEvents={chronicle.Current.Events.Count}");
            return failures;
        }

        private static int ValidateProceduralSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "Procedural", Gender = "male", Appearance = 1 }, new SystemRandomRng(51));
            player.RealmIndex = 2;
            player.Luck = 30;
            player.Gold = 100;
            player.InventoryCapacity = 50;
            PlayerStatsSystem.RecalcStats(db, player);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.Stamina = player.MaxStamina;

            var a = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "A", Gender = "male", Appearance = 1 }, new SystemRandomRng(52));
            var b = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions { Name = "B", Gender = "male", Appearance = 1 }, new SystemRandomRng(52));
            a.RealmIndex = b.RealmIndex = 2; a.Luck = b.Luck = 30;

            var ev1 = EventGenerator.GenerateEvent(db, a, "explore", null, 7001)?.Event;
            var ev2 = EventGenerator.GenerateEvent(db, b, "explore", null, 7001)?.Event;
            failures += Expect("procedural.event.sameSeed", SameJson(ev1, ev2));
            ev1.Category = "proc-test";
            db.Events[ev1.Id] = ev1;
            player.Hp = Math.Max(1, player.MaxHp - 40);
            player.Mp = Math.Max(0, player.MaxMp - 20);
            player.Mood = 50;
            int goldBefore = player.Gold, expBefore = player.Exp, hpBefore = player.Hp, mpBefore = player.Mp;
            var available = EventSystem.GetAvailableEvents(db, player, "proc-test");
            var evApply = EventSystem.TriggerEvent(db, player, "proc-test", new FixedRng(0));
            failures += Expect("procedural.event.engine", available.Any(x => x.Id == ev1.Id) && evApply != null && evApply.EventId == ev1.Id && (player.Gold != goldBefore || player.Exp != expBefore || player.Hp != hpBefore || player.Mp != mpBefore));

            var mon1 = MonsterGenerator.GenerateMonsterVariant(db, a, new Xiuxian.Systems.Procedural.GenerateMonsterOptions { Seed = 7002 })?.Monster;
            var mon2 = MonsterGenerator.GenerateMonsterVariant(db, b, new Xiuxian.Systems.Procedural.GenerateMonsterOptions { Seed = 7002 })?.Monster;
            failures += Expect("procedural.monster.sameSeed", SameJson(mon1, mon2));
            failures += Expect("procedural.monster.scaled", mon1 != null && mon1.RealmIndex == 2 && mon1.Hp > 50 && mon1.Atk > 10 && mon1.ExpReward > 20);
            var combat = CombatEngine.RunCombat(db, player, CombatantFactory.FromMonster(mon1), new FixedRng(0.4), false);
            failures += Expect("procedural.monster.combatUsable", combat.MonsterMaxHp == mon1.Hp && !string.IsNullOrEmpty(combat.Winner));

            var eq1 = EquipGenerator.GenerateEquip(db, a, new GenerateEquipOptions { SlotFilter = "weapon", ForcedQuality = "treasure", Seed = 7003 });
            var eq2 = EquipGenerator.GenerateEquip(db, b, new GenerateEquipOptions { SlotFilter = "weapon", ForcedQuality = "treasure", Seed = 7003 });
            failures += Expect("procedural.equip.sameSeed", SameJson(eq1, eq2));
            EquipmentSystem.GetProceduralItemState(player).GeneratedEquips.Add(eq1);
            InventorySystem.AddItem(player, eq1.InstanceId, 1);
            int atkBefore = player.Atk;
            var equipResult = EquipmentSystem.EquipItem(db, player, eq1.InstanceId);
            var bonus = EquipmentSystem.GetEquipmentStatBonus(db, player);
            failures += Expect("procedural.equip.usable", equipResult.Success && eq1.PrefixIds.Count + eq1.SuffixIds.Count > 0 && eq1.FinalStats.Count > 0 && bonus.Atk > 0 && player.Atk > atkBefore);

            var baseTech = db.Techniques.Values.First(t => !string.IsNullOrEmpty(t.Type));
            var tech1 = TechniqueGenerator.GenerateTechniqueInstance(db, a, baseTech.Id, baseTech.Type, new Xiuxian.Systems.Procedural.GenerateTechniqueOptions { ForcedQuality = "rare", Seed = 7004 });
            var tech2 = TechniqueGenerator.GenerateTechniqueInstance(db, b, baseTech.Id, baseTech.Type, new Xiuxian.Systems.Procedural.GenerateTechniqueOptions { ForcedQuality = "rare", Seed = 7004 });
            failures += Expect("procedural.technique.sameSeed", SameJson(tech1, tech2));
            player.Techniques.Add(new TechniqueSlot { TechniqueId = baseTech.Id, Level = 1, Exp = 0, InstanceId = tech1.InstanceId });
            player.ActiveTechniqueId = baseTech.Id;
            TechniqueGenerator.GetState(player).Instances.Add(tech1);
            var traitBonus = TechniqueGenerator.GetTraitBonus(player, tech1.InstanceId);
            var activeBonus = TechniqueSystem.GetActiveTechniqueBonus(db, player);
            failures += Expect("procedural.technique.traits", tech1.Traits.Count > 0 && traitBonus.Count > 0 && traitBonus.Keys.Any(k => activeBonus.ContainsKey(k)));

            Console.WriteLine($"[OK] 程序化摘要: event={ev1.Name}/{evApply.Message}, monster={mon1.Name} hp={mon1.Hp} atk={mon1.Atk} combat={combat.Winner}, equip={eq1.FinalName} affixes={eq1.PrefixIds.Count + eq1.SuffixIds.Count} atkBonus={bonus.Atk}, technique={baseTech.Name} traits={tech1.Traits.Count}");
            return failures;
        }

        private static bool SameJson(object a, object b) =>
            Newtonsoft.Json.JsonConvert.SerializeObject(a) == Newtonsoft.Json.JsonConvert.SerializeObject(b);

        private static int ValidateSaveLoadSystems(GameDatabase db)
        {
            int failures = 0;
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions
            {
                Name = "SaveHero",
                Gender = "female",
                Appearance = 2,
            }, new SystemRandomRng(61));
            player.Gold = 1000;
            player.RealmIndex = 2;
            player.Age = 245;
            player.GameYear = 5;
            player.GameMonth = 6;
            player.InventoryCapacity = 80;
            player.Stamina = player.MaxStamina = 200;
            player.Mp = player.MaxMp;
            player.Hp = player.MaxHp;
            player.Aptitudes.Water = 100;
            player.Tracking.KillCount = 1;
            PlayerStatsSystem.RecalcStats(db, player);

            InventorySystem.AddItem(db, player, "core:hp_pill", 3);
            InventorySystem.AddItem(db, player, "core:iron_sword", 1);
            EquipmentSystem.EquipItem(db, player, "core:iron_sword");
            InventorySystem.AddItem(db, player, "core:scroll_technique_basic_sword", 1);
            LearningSystem.StartStudy(db, player, "core:scroll_technique_basic_sword");
            LearningSystem.TickStudy(db, player, 99);
            DivineArtSystem.Learn(db, player, "cp-01:divine_ice_cone");
            DivineArtSystem.Activate(db, player, "cp-01:divine_ice_cone");

            MapSystem.GetMapState(db, player);
            MapSystem.TravelTo(db, player, "core:beast_mountain");
            SectSystem.JoinSect(db, player, "core:qingyun_sect");
            SectSystem.CompleteSectMission(db, player, "qingyun_patrol");
            KarmaSystem.ChangeKarma(player, 45, "save-test", "save-major");
            RankingSystem.Refresh(db, player, true);
            AchievementSystem.CheckAchievements(player);
            var chronicle = ChronicleSystem.CreateIncarnation(db, player, ChronicleSystem.Empty());
            ChronicleSystem.AddEvent(chronicle, new ChronicleEvent { Type = "save_test", Year = player.GameYear, Month = player.GameMonth, Description = "roundtrip" });
            player.Systems["chronicle"] = chronicle;

            var generated = EquipmentSystem.GenerateEquip(db, player, new FixedRng(0.1), new GenerateEquipOptions { SlotFilter = "weapon", ForcedQuality = "treasure", Seed = 6101 });
            EquipmentSystem.GetProceduralItemState(player).GeneratedEquips.Add(generated);
            InventorySystem.AddItem(player, generated.InstanceId, 1);
            TechniqueGenerator.GetState(player).Instances.Add(TechniqueGenerator.GenerateTechniqueInstance(db, player, player.Techniques.First().TechniqueId, "sword", new Xiuxian.Systems.Procedural.GenerateTechniqueOptions { ForcedQuality = "rare", Seed = 6102 }));

            var storage = new InMemorySaveStorage();
            var saves = new SaveSystem(storage, () => 1234567890L);
            failures += Expect("saveLoad.emptyLoad", saves.LoadSlot(4) == null);
            saves.SaveSlot(2, player);
            var loaded = saves.LoadSlot(2);
            failures += Expect("saveLoad.notNull", loaded != null);
            if (loaded != null)
            {
                var originalJson = JToken.Parse(SaveSystem.ToCanonicalJson(player));
                var loadedJson = JToken.Parse(SaveSystem.ToCanonicalJson(loaded));
                failures += Expect("saveLoad.roundTrip.deepJson", JToken.DeepEquals(originalJson, loadedJson));
                failures += Expect("saveLoad.meaningfulState", loaded.RealmIndex == player.RealmIndex
                    && loaded.Age == player.Age
                    && loaded.GameYear == player.GameYear
                    && loaded.GameMonth == player.GameMonth
                    && loaded.Inventory.Any(x => x.ItemId == "core:hp_pill" && x.Count == 3)
                    && loaded.Equipped.Weapon == "core:iron_sword"
                    && loaded.Techniques.Count == player.Techniques.Count
                    && loaded.Karma == player.Karma
                    && SectSystem.GetSectState(loaded).SectId == "core:qingyun_sect"
                    && AchievementSystem.GetState(loaded).UnlockedIds.Contains("core:first_blood")
                    && RankingSystem.GetState(loaded).Snapshots.ContainsKey("core:combat_power")
                    && ((CultivationChronicle)loaded.Systems["chronicle"]).Current.Events.Count == 1);
            }

            var slots = saves.ListSlots();
            var slot2 = slots[2];
            failures += Expect("saveLoad.preview", slots.Count == SaveSystem.SaveSlotCount
                && !slot2.IsEmpty
                && slot2.Name == "SaveHero"
                && slot2.RealmIndex == 2
                && slot2.Age == player.Age
                && slot2.GameYear == player.GameYear
                && slot2.GameMonth == player.GameMonth
                && slot2.SavedAt == 1234567890L);
            saves.DeleteSlot(2);
            failures += Expect("saveLoad.delete", saves.LoadSlot(2) == null && saves.ListSlots()[2].IsEmpty);

            Console.WriteLine($"[OK] 存档摘要: slot=2 name={slot2.Name}, realm={slot2.RealmIndex}, age={slot2.Age}, savedAt={slot2.SavedAt}, systems={player.Systems.Count}, deleted={saves.ListSlots()[2].IsEmpty}");
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
