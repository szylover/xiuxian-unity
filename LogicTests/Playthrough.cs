using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xiuxian.Data;
using Xiuxian.Systems;
using Xiuxian.Systems.Procedural;

namespace Xiuxian.LogicTests
{
    public static class Playthrough
    {
        private const int Seed = 17017;

        public static int Run(GameDatabase db)
        {
            Console.WriteLine("\n=== PLAYTHROUGH ===");
            int failures = 0;
            var rng = new SystemRandomRng(Seed);
            var player = PlayerFactory.CreatePlayer(db, new CreatePlayerOptions
            {
                Name = "PlaythroughHero",
                Gender = "male",
                Appearance = 1,
            }, rng);

            player.InventoryCapacity = 300;
            player.Gold = 2000;
            player.Stamina = player.MaxStamina = 500;
            player.MentalPower = player.MaxMentalPower = 200;
            player.Aptitudes.Alchemy = 80;
            player.Aptitudes.Smithing = 80;
            player.Aptitudes.Water = 100;
            player.Luck = 80;
            player.Comprehension = 80;
            player.Charisma = 30;
            PlayerStatsSystem.RecalcStats(db, player);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;

            int startRealm = player.RealmIndex;
            int cultivatedTicks = 0;
            int bottlenecksCleared = 0;
            int breakthroughs = 0;
            var realmTrail = new List<int> { player.RealmIndex };

            while (player.RealmIndex < 7)
            {
                int targetRealm = player.RealmIndex + 1;
                int expReq = db.Realms[targetRealm].ExpReq ?? player.Exp;
                while (player.Exp < expReq && cultivatedTicks < 160)
                {
                    CultivationSystem.GainCultivation(db, player);
                    cultivatedTicks++;
                }

                if (player.Exp < expReq)
                    player.Exp = expReq;

                bool advanced = false;
                for (int attemptIndex = 0; attemptIndex < 4 && !advanced; attemptIndex++)
                {
                    PrepareBreakthrough(db, player, targetRealm);
                    var attempt = BreakthroughSystem.AttemptBreakthrough(db, player, new FixedRng(0.0));
                    if (attempt.BlockedByBottleneck)
                    {
                        var active = BottleneckSystem.GetActiveBottlenecks(db, player).FirstOrDefault();
                        if (active.Entry != null)
                        {
                            BottleneckSystem.UnlockBottleneck(db, player, active.Entry.BottleneckId, "playthrough");
                            bottlenecksCleared++;
                        }

                        continue;
                    }

                    if (attempt.TriggerTribulation)
                    {
                        var trib = TribulationSystem.RunTribulation(db, player, new StubTribulationCombatResolver(), new FixedRng(0.0));
                        advanced = trib.Success && player.RealmIndex == targetRealm;
                    }
                    else
                    {
                        advanced = attempt.Success && player.RealmIndex == targetRealm;
                    }

                    if (!advanced)
                        player.Exp = Math.Max(player.Exp, expReq);
                }

                failures += Expect($"playthrough.advanceRealm.{targetRealm}", advanced);
                breakthroughs++;
                realmTrail.Add(player.RealmIndex);
                player.Hp = player.MaxHp;
                player.Mp = player.MaxMp;
                player.Stamina = player.MaxStamina;
            }

            DivineArtSystem.Learn(db, player, "cp-01:divine_ice_cone");
            DivineArtSystem.Activate(db, player, "cp-01:divine_ice_cone");
            var knownMonster = db.Monsters.Values.Where(m => m.RealmIndex <= player.RealmIndex).OrderBy(m => m.RealmIndex).First();
            var knownCombat = CombatEngine.RunCombat(db, player, knownMonster, new FixedRng(0.2));
            ApplyCombatRewards(player, knownCombat);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            int realmForProceduralMonster = player.RealmIndex;
            player.RealmIndex = 1;
            var generatedMonster = MonsterGenerator.GenerateMonsterVariant(db, player, new GenerateMonsterOptions { Seed = Seed + 1 }).Monster;
            player.RealmIndex = realmForProceduralMonster;
            player.Atk = Math.Max(player.Atk, generatedMonster.Hp ?? 1);
            player.Def = Math.Max(player.Def, generatedMonster.Atk ?? 1);
            player.Speed = Math.Max(player.Speed, generatedMonster.Speed ?? 1);
            player.Hp = player.MaxHp;
            var generatedCombatant = CombatantFactory.FromMonster(generatedMonster);
            generatedCombatant.Hp = 1;
            generatedCombatant.MaxHp = 1;
            generatedCombatant.Atk = 1;
            generatedCombatant.Def = 0;
            generatedCombatant.Speed = 0;
            generatedCombatant.MoveSpeed = 0;
            generatedCombatant.CritResist = 0;
            var procCombat = CombatEngine.RunCombat(db, player, generatedCombatant, new FixedRng(0.0), false);
            ApplyCombatRewards(player, procCombat);
            failures += Expect("playthrough.combat.rewards", knownCombat.Winner == "player" && procCombat.Winner == "player" && player.Tracking.KillCount >= 2);

            var generatedEquip = EquipmentSystem.GenerateEquip(db, player, new FixedRng(0.1), new GenerateEquipOptions { SlotFilter = "weapon", ForcedQuality = "treasure", Seed = Seed + 2 });
            EquipmentSystem.GetProceduralItemState(player).GeneratedEquips.Add(generatedEquip);
            InventorySystem.AddItem(player, generatedEquip.InstanceId, 1);
            int atkBeforeEquip = player.Atk;
            var equipResult = EquipmentSystem.EquipItem(db, player, generatedEquip.InstanceId);
            failures += Expect("playthrough.equipment.statGain", equipResult.Success && player.Atk > atkBeforeEquip);

            player.Systems["learning"] = new LearningState
            {
                LearnedRecipes = new[] { "core:recipe_hp_pill" },
                LearnedSmithingRecipes = new[] { "core:smith_iron_sword" }
            };
            InventorySystem.AddItem(db, player, "core:herb_lingzhi", 20);
            int herbsBefore = InventorySystem.CountItem(player, "core:herb_lingzhi");
            var brew = AlchemySystem.PerformAlchemy(db, player, "core:recipe_hp_pill", new FixedRng(0.0));
            int goldBeforeShop = player.Gold;
            var buy = ShopSystem.BuyItem(db, player, "core:hp_pill", 1);
            var sell = ShopSystem.SellItem(db, player, "core:hp_pill", 1);
            failures += Expect("playthrough.economy.inventory", brew.Success && buy.Success && sell.Success && InventorySystem.CountItem(player, "core:herb_lingzhi") < herbsBefore && player.Gold != goldBeforeShop);

            db.Events["test:playthrough_blessing"] = new GameEventDef
            {
                Id = "test:playthrough_blessing",
                Category = "playthrough",
                Tone = "good",
                Name = "playthrough blessing",
                Weight = 1,
                Effects = new Dictionary<string, EffectValue> { ["gold"] = EffectValue.FromScalar(10), ["karma"] = EffectValue.FromScalar(5) },
                Message = "playthrough blessing",
                Condition = JObject.FromObject(new { minRealm = 0 })
            };
            int goldBeforeEvent = player.Gold;
            var worldEvent = EventSystem.TriggerEvent(db, player, "playthrough", new FixedRng(0.0));
            NpcSystem.MeetNpc(db, player, "core:npc_qingyun_elder");
            var relation = NpcSystem.ChangeAffinity(db, player, "core:npc_qingyun_elder", 8);
            player.Stamina = player.MaxStamina;
            var travel = MapSystem.TravelTo(db, player, "core:beast_mountain");
            QuestSystem.CheckQuestDiscovery(db, player, new QuestTrigger { Type = "talk_npc", NpcId = "core:npc_beast_hunter" });
            var questAccept = QuestSystem.AcceptQuest(db, player, "core:quest_wolf_bounty");
            for (int i = 0; i < 5; i++)
                QuestSystem.TickQuestObjectives(db, player, new QuestTrigger { Type = "kill_monster", MonsterId = "core:wild_wolf" });
            var questTurnIn = QuestSystem.TurnInQuest(db, player, "core:quest_wolf_bounty");
            var joinSect = SectSystem.JoinSect(db, player, "core:qingyun_sect");
            var sectMission = SectSystem.CompleteSectMission(db, player, "qingyun_patrol");
            failures += Expect("playthrough.world.questSect", worldEvent != null && player.Gold > goldBeforeEvent && travel.Success && relation.AffinityChange == 8 && questAccept.Success && questTurnIn.Success && joinSect.Success && sectMission.Success);

            var karma = KarmaSystem.ChangeKarma(player, 45, "playthrough", "core-loop");
            player.Tracking.KillCount = Math.Max(player.Tracking.KillCount, 1);
            var achievement = AchievementSystem.CheckAchievements(player);
            var chronicle = ChronicleSystem.CreateIncarnation(db, player, ChronicleSystem.Empty());
            ChronicleSystem.AddEvent(chronicle, new ChronicleEvent { Type = "core_loop", Year = player.GameYear, Month = player.GameMonth, Description = "playthrough" });
            player.Systems["chronicle"] = chronicle;

            player.Exp = Math.Max(player.Exp, 500000);
            player.Tracking.DefeatedHigherRealm = true;
            InventorySystem.AddItem(db, player, "cp-03:ascension_token", 1);
            InventorySystem.AddItem(db, player, "cp-03:immortal_dew", 2);
            var ascReady = AscensionSystem.GetStatus(db, player);
            var ascDone = ascReady.CanAscend ? AscensionSystem.ApplySuccess(db, player, ascReady.AscDef) : null;
            failures += Expect("playthrough.progression.ascension", karma.Success && achievement.Success && chronicle.Current.Events.Count == 1 && ascReady.CanAscend && ascDone != null && ascDone.Success && AscensionSystem.GetState(player).HasAscended);

            var storage = new InMemorySaveStorage();
            var saves = new SaveSystem(storage, () => 170170001L);
            saves.SaveSlot(3, player);
            var loaded = saves.LoadSlot(3);
            failures += Expect("playthrough.save.notNull", loaded != null);
            if (loaded != null)
            {
                var originalJson = JToken.Parse(SaveSystem.ToCanonicalJson(player));
                var loadedJson = JToken.Parse(SaveSystem.ToCanonicalJson(loaded));
                failures += Expect("playthrough.save.deepRoundTrip", JToken.DeepEquals(originalJson, loadedJson));
                failures += Expect("playthrough.invariants", loaded.Hp >= 0 && loaded.RealmIndex > startRealm && AchievementSystem.GetState(loaded).UnlockedIds.Count > 0 && AscensionSystem.GetState(loaded).HasAscended);
            }

            string milestones = string.Join(", ", new[]
            {
                $"realmTrail={string.Join("->", realmTrail)}",
                $"bottlenecks={bottlenecksCleared}",
                $"battlesWon={(knownCombat.Winner == "player" ? 1 : 0) + (procCombat.Winner == "player" ? 1 : 0)}",
                $"equip={generatedEquip.FinalName}",
                $"event={worldEvent?.EventId}",
                $"quest=core:quest_wolf_bounty",
                $"sect={SectSystem.GetSectState(player).SectId}",
                $"karma={player.Karma}/{karma.Alignment}",
                $"achievements={AchievementSystem.GetState(player).UnlockedIds.Count}",
                $"chronicleEvents={chronicle.Current.Events.Count}",
                $"ascended={AscensionSystem.GetState(player).HasAscended}"
            });
            Console.WriteLine($"[OK] PLAYTHROUGH SUMMARY: seed={Seed}, cultivatedTicks={cultivatedTicks}, breakthroughs={breakthroughs}, {milestones}, finalRealm={player.RealmIndex}, hp={player.Hp}/{player.MaxHp}, gold={player.Gold}, saveRoundTrip={(loaded != null)}");
            return failures;
        }

        private static void PrepareBreakthrough(GameDatabase db, Player player, int targetRealm)
        {
            if (targetRealm >= 4) player.Tracking.KillCount = Math.Max(player.Tracking.KillCount, 50);
            if (targetRealm >= 5) player.Tracking.DefeatedHigherRealm = true;
            var status = BreakthroughSystem.GetBreakthroughStatus(db, player);
            foreach (var item in status.ItemsReady.Where(x => !x.Ready))
                InventorySystem.AddItem(db, player, item.ItemId, item.Required - item.Have);
        }

        private static void ApplyCombatRewards(Player player, CombatResult combat)
        {
            if (combat.Winner != "player") return;
            player.Exp += combat.ExpGained;
            player.Gold += combat.GoldGained;
            player.Hp = Math.Max(1, combat.PlayerHpLeft);
            player.Tracking.KillCount++;
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
    }
}
