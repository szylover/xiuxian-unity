// ============================================================
// GameContext.cs — runtime app state shared by uGUI shell
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.App
{
    public sealed class GameContext
    {
        private readonly IDataSource dataSource;
        private readonly SystemRandomRng rng = new();
        private readonly System.Random effectRandom = new();

        public GameDatabase Database { get; private set; } = new();
        public SaveSystem SaveSystem { get; }
        public Player CurrentPlayer { get; private set; }
        public int CurrentSlot { get; private set; }
        public IReadOnlyList<string> AvailablePackIds { get; }
        public HashSet<string> SelectedPackIds { get; } = new();
        public List<string> LogEntries { get; } = new();
        public GameEventBus Bus { get; } = new();

        public GameContext(IDataSource dataSource, SaveSystem saveSystem)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            SaveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            AvailablePackIds = dataSource.ListPackages().OrderBy(PackageSortKey, StringComparer.Ordinal).ToArray();
            SelectedPackIds.Add("core");
            LoadDatabase(SelectedPackIds);
        }

        public PreviewRoll RollPreview() => PlayerFactory.RollPreview(rng);

        public void LoadDatabase(IEnumerable<string> packageIds)
        {
            var requested = new HashSet<string>(packageIds ?? Array.Empty<string>());
            requested.Add("core");
            Database = new GameDatabase();
            var loader = new DlcLoader(dataSource, Database);
            foreach (var id in AvailablePackIds.Where(id => requested.Contains(id)).OrderBy(PackageSortKey, StringComparer.Ordinal))
                loader.LoadPackage(id);
            SelectedPackIds.Clear();
            foreach (var id in Database.EnabledPackIds) SelectedPackIds.Add(id);
            Publish(GameEventType.DatabaseLoaded, Database);
        }

        public Player CreateNewPlayer(int slotIndex, string playerName, string gender, int appearance, PreviewRoll preview, IEnumerable<string> enabledPacks)
        {
            CurrentSlot = slotIndex;
            LoadDatabase(enabledPacks);
            CurrentPlayer = PlayerFactory.CreatePlayer(Database, new CreatePlayerOptions
            {
                Name = playerName,
                Gender = gender,
                Appearance = appearance,
                Preview = preview,
                EnabledDLCs = SelectedPackIds.ToArray(),
            }, rng);
            AddLog(UiTexts.LogNewGame(CurrentPlayer.Name));
            SaveCurrent();
            Publish(GameEventType.PlayerCreated, CurrentPlayer);
            PublishPlayerChanges(GameEventType.PlayerStatsChanged, GameEventType.InventoryChanged, GameEventType.EquipmentChanged, GameEventType.TechniquesChanged, GameEventType.MapChanged, GameEventType.QuestChanged, GameEventType.SectChanged);
            return CurrentPlayer;
        }

        public bool LoadSlot(int slotIndex)
        {
            var player = SaveSystem.LoadSlot(slotIndex);
            if (player == null) return false;
            CurrentSlot = slotIndex;
            CurrentPlayer = player;
            LoadDatabase(player.EnabledDLCs.Count > 0 ? player.EnabledDLCs : new[] { "core" });
            AddLog(UiTexts.LogLoaded(player.Name));
            Publish(GameEventType.PlayerLoaded, player);
            PublishPlayerChanges(GameEventType.PlayerStatsChanged, GameEventType.InventoryChanged, GameEventType.EquipmentChanged, GameEventType.TechniquesChanged, GameEventType.MapChanged, GameEventType.QuestChanged, GameEventType.SectChanged);
            return true;
        }

        public void SaveCurrent()
        {
            if (CurrentPlayer == null) return;
            SaveSystem.SaveSlot(CurrentSlot, CurrentPlayer);
            AddLog(UiTexts.LogSaved(CurrentSlot + 1));
            Publish(GameEventType.PlayerSaved, CurrentPlayer);
        }

        public void ExitToStart()
        {
            CurrentPlayer = null;
            LogEntries.Clear();
            Publish(GameEventType.ExitToStart, null);
        }

        public void AddLog(string message)
        {
            LogEntries.Add(message);
            Publish(GameEventType.LogAppended, message);
        }

        public void Publish(GameEvent gameEvent)
            => Bus.Publish(gameEvent);

        public void Publish(GameEventType type, object payload = null)
            => Bus.Publish(type, payload);

        public void PublishPlayerChanges(params GameEventType[] changeTypes)
        {
            if (changeTypes == null || changeTypes.Length == 0)
            {
                Publish(GameEventType.PlayerChanged, CurrentPlayer);
                return;
            }

            for (var i = 0; i < changeTypes.Length; i++)
                Publish(changeTypes[i], CurrentPlayer);
            Publish(GameEventType.PlayerChanged, CurrentPlayer);
        }

        public void Cultivate()
        {
            if (CurrentPlayer == null) return;
            var result = CultivationSystem.GainCultivation(Database, CurrentPlayer);
            CurrentPlayer = result.Player;
            AdvanceTime(1);
            AddLog(UiTexts.CultivateGain(result.ExpGain));
            foreach (var log in result.Logs) if (!string.IsNullOrEmpty(log)) AddLog(log);
            var insight = EnlightenmentSystem.TryTrigger(CurrentPlayer, result.ExpGain, rng);
            if (insight.Triggered) AddLog(UiTexts.ActiveName(insight.Message));
            PublishPlayerChanges(GameEventType.CultivationChanged, GameEventType.EnlightenmentChanged, GameEventType.PlayerStatsChanged);
        }

        public void AttemptBreakthrough()
        {
            if (CurrentPlayer == null) return;
            var beforeRealm = CurrentPlayer.RealmIndex;
            var result = BreakthroughSystem.AttemptBreakthrough(Database, CurrentPlayer, rng);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(log);
            PublishPlayerChanges(beforeRealm == CurrentPlayer.RealmIndex ? GameEventType.BreakthroughChanged : GameEventType.RealmChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged);
        }

        public void BodyCultivate()
        {
            if (CurrentPlayer == null) return;
            var result = BodyCultivationSystem.GainBodyRealmExp(Database, CurrentPlayer, SystemBalance.BaseCultivateExp);
            CurrentPlayer = result.Player;
            AdvanceTime(1);
            AddLog(UiTexts.BodyGain(result.ActualGain));
            if (!string.IsNullOrEmpty(result.Message)) AddLog(result.Message);
            PublishPlayerChanges(GameEventType.BodyCultivationChanged, GameEventType.PlayerStatsChanged);
        }

        public void AttemptBodyBreakthrough()
        {
            if (CurrentPlayer == null) return;
            var before = CurrentPlayer.BodyRealmIndex;
            var result = BodyCultivationSystem.TryBodyRealmBreakthrough(Database, CurrentPlayer);
            CurrentPlayer = result.Player;
            if (!string.IsNullOrEmpty(result.Message)) AddLog(result.Message);
            PublishPlayerChanges(before == CurrentPlayer.BodyRealmIndex ? GameEventType.BreakthroughChanged : GameEventType.BodyCultivationChanged, GameEventType.PlayerStatsChanged);
        }

        public void Rest()
        {
            if (CurrentPlayer == null) return;
            CurrentPlayer.Stamina = CurrentPlayer.MaxStamina;
            CurrentPlayer.Hp = CurrentPlayer.MaxHp;
            CurrentPlayer.Mp = CurrentPlayer.MaxMp;
            CurrentPlayer.MentalPower = CurrentPlayer.MaxMentalPower;
            CurrentPlayer.Mood = System.Math.Min(100, CurrentPlayer.Mood + 8);
            CurrentPlayer.Health = System.Math.Min(100, CurrentPlayer.Health + 5);
            CurrentPlayer.Tracking.ConsecutiveRests += 1;
            CurrentPlayer.Tracking.ConsecutiveCultivates = 0;
            BodyCultivationSystem.RestorePhysique(Database, CurrentPlayer);
            AdvanceTime(1);
            AddLog(UiTexts.LogRest);
            PublishPlayerChanges(GameEventType.PlayerStatsChanged, GameEventType.BodyCultivationChanged);
        }

        public void SetActiveTechnique(string techniqueId)
        {
            if (CurrentPlayer == null) return;
            CurrentPlayer.ActiveTechniqueId = CurrentPlayer.ActiveTechniqueId == techniqueId ? null : techniqueId;
            PlayerStatsSystem.RecalcStats(Database, CurrentPlayer);
            AddLog(UiTexts.LogTechniqueActivated);
            PublishPlayerChanges(GameEventType.TechniquesChanged, GameEventType.PlayerStatsChanged);
        }

        public void PracticeTechnique(string techniqueId)
        {
            if (CurrentPlayer == null) return;
            var slot = CurrentPlayer.Techniques.FirstOrDefault(t => t.TechniqueId == techniqueId);
            if (slot == null || !Database.Techniques.TryGetValue(techniqueId, out var def)) return;
            var maxLevel = System.Math.Max(1, def.MaxLevel ?? 1);
            if (slot.Level >= maxLevel) return;
            var expPerLevel = System.Math.Max(1, def.ExpPerLevel ?? 100);
            var rootBoost = 1.0;
            if (!string.IsNullOrEmpty(def.SpiritRootElement))
            {
                var root = CurrentPlayer.SpiritRoots.Roots.FirstOrDefault(r => r.Type == def.SpiritRootElement);
                if (root != null) rootBoost += root.Affinity / 100.0;
            }
            var gain = System.Math.Max(1, (int)System.Math.Floor(expPerLevel * 0.2 * rootBoost * (1 + CurrentPlayer.Comprehension / 150.0)));
            slot.Exp += gain;
            while (slot.Level < maxLevel && slot.Exp >= expPerLevel)
            {
                slot.Exp -= expPerLevel;
                slot.Level++;
            }
            AdvanceTime(1);
            AddLog(UiTexts.CultivateGain(gain));
            PublishPlayerChanges(GameEventType.TechniquesChanged, GameEventType.PlayerStatsChanged);
        }

        public void ActivateDivineArt(string artId)
        {
            if (CurrentPlayer == null) return;
            var state = DivineArtSystem.GetState(CurrentPlayer);
            state.ActiveArtId = state.ActiveArtId == artId ? null : artId;
            AddLog(UiTexts.LogDivineArtActivated);
            PublishPlayerChanges(GameEventType.DivineArtsChanged, GameEventType.PlayerStatsChanged);
        }

        public void EquipItem(string itemId)
        {
            if (CurrentPlayer == null) return;
            var result = EquipmentSystem.EquipItem(Database, CurrentPlayer, itemId);
            CurrentPlayer = result.Player;
            AddLog(string.IsNullOrEmpty(result.Message) ? UiTexts.OperationFailed : result.Message);
            PublishPlayerChanges(GameEventType.InventoryChanged, GameEventType.EquipmentChanged, GameEventType.PlayerStatsChanged);
        }

        public void UnequipItem(string slot)
        {
            if (CurrentPlayer == null) return;
            var result = EquipmentSystem.UnequipItem(Database, CurrentPlayer, slot);
            CurrentPlayer = result.Player;
            AddLog(string.IsNullOrEmpty(result.Message) ? UiTexts.OperationFailed : result.Message);
            PublishPlayerChanges(GameEventType.InventoryChanged, GameEventType.EquipmentChanged, GameEventType.PlayerStatsChanged);
        }

        public void UseItem(string itemId)
        {
            if (CurrentPlayer == null || !Database.Items.TryGetValue(itemId, out var def) || !InventorySystem.HasItem(CurrentPlayer, itemId)) return;
            if (def.Category == "scroll")
            {
                StartStudy(itemId);
                return;
            }
            if (!def.Usable || def.Effects == null || def.Effects.Count == 0) return;
            foreach (var effect in def.Effects) ApplyItemEffect(effect.Key, effect.Value);
            InventorySystem.RemoveItem(CurrentPlayer, itemId, 1);
            PlayerStatsSystem.RecalcStats(Database, CurrentPlayer);
            AddLog(string.IsNullOrEmpty(def.EffectMessage) ? UiTexts.LogItemUsed : def.EffectMessage);
            PublishPlayerChanges(GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged);
        }

        public void DropItem(string itemId)
        {
            if (CurrentPlayer == null) return;
            if (!InventorySystem.RemoveItem(CurrentPlayer, itemId, 1)) return;
            AddLog(UiTexts.LogItemDropped);
            PublishPlayerChanges(GameEventType.InventoryChanged);
        }

        public void EnsureDestiny()
        {
            if (CurrentPlayer == null) return;
            DestinySystem.EnsureDestiny(CurrentPlayer, rng);
            PublishPlayerChanges(GameEventType.TalentChanged, GameEventType.PlayerStatsChanged);
        }

        public void UnlockTalentNode(string nodeId)
        {
            if (CurrentPlayer == null) return;
            var result = DestinySystem.UnlockTalentNode(CurrentPlayer, nodeId);
            if (!result.Success) return;
            AddLog(UiTexts.ActiveName(result.Message));
            PublishPlayerChanges(GameEventType.TalentChanged, GameEventType.PlayerStatsChanged);
        }

        public void ContemplateInsight()
        {
            if (CurrentPlayer == null) return;
            var result = EnlightenmentSystem.ContemplateInsight(CurrentPlayer);
            if (!result.Success) return;
            AddLog(UiTexts.ActiveName(result.Message));
            PublishPlayerChanges(GameEventType.EnlightenmentChanged, GameEventType.TalentChanged, GameEventType.PlayerStatsChanged);
        }

        public void TriggerEnlightenment()
        {
            if (CurrentPlayer == null) return;
            var result = EnlightenmentSystem.TryTrigger(CurrentPlayer, SystemBalance.BaseCultivateExp, rng, true);
            if (result.Triggered) AddLog(UiTexts.ActiveName(result.Message));
            PublishPlayerChanges(GameEventType.EnlightenmentChanged, GameEventType.CultivationChanged, GameEventType.PlayerStatsChanged);
        }

        public void StartStudy(string itemId)
        {
            if (CurrentPlayer == null) return;
            var result = LearningSystem.StartStudy(Database, CurrentPlayer, itemId);
            if (!result.Success) return;
            AddLog(UiTexts.ActiveName(result.Message));
            PublishPlayerChanges(GameEventType.InventoryChanged, GameEventType.TechniquesChanged, GameEventType.DivineArtsChanged);
        }

        public void TickStudy()
        {
            if (CurrentPlayer == null) return;
            var result = LearningSystem.TickStudy(Database, CurrentPlayer, 1);
            AdvanceTime(1);
            AddLog(result.Completed ? UiTexts.ActiveName(result.Message) : UiTexts.LogStudyTick);
            PublishPlayerChanges(GameEventType.InventoryChanged, GameEventType.TechniquesChanged, GameEventType.DivineArtsChanged, GameEventType.PlayerStatsChanged);
        }

        public void CancelStudy()
        {
            if (CurrentPlayer == null) return;
            LearningSystem.GetState(CurrentPlayer).ActiveStudy = null;
            AddLog(UiTexts.LogStudyCancel);
            PublishPlayerChanges(GameEventType.TechniquesChanged, GameEventType.DivineArtsChanged);
        }

        public void BrewAlchemy(string recipeId)
        {
            if (CurrentPlayer == null) return;
            var result = AlchemySystem.PerformAlchemy(Database, CurrentPlayer, recipeId, rng);
            CurrentPlayer = result.Player;
            if (result.Success && Database.Recipes.TryGetValue(recipeId, out var recipe))
                QuestSystem.TickQuestObjectives(Database, CurrentPlayer, new QuestTrigger { Type = "craft_item", RecipeId = recipeId, OutputItemId = recipe.OutputItemId });
            AddLog(UiTexts.LogOperation(UiTexts.Alchemy, result.Message ?? UiTexts.OperationFailed));
            PublishPlayerChanges(GameEventType.AlchemyChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged, GameEventType.QuestChanged);
        }

        public void ForgeSmithing(string recipeId)
        {
            if (CurrentPlayer == null) return;
            var result = SmithingSystem.PerformSmithing(Database, CurrentPlayer, recipeId, rng);
            CurrentPlayer = result.Player;
            if (result.Success && Database.SmithingRecipes.TryGetValue(recipeId, out var recipe))
                QuestSystem.TickQuestObjectives(Database, CurrentPlayer, new QuestTrigger { Type = "craft_item", RecipeId = recipeId, OutputItemId = recipe.OutputItemId });
            AddLog(UiTexts.LogOperation(UiTexts.Smithing, result.Message ?? UiTexts.OperationFailed));
            PublishPlayerChanges(GameEventType.SmithingChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged, GameEventType.PlayerStatsChanged, GameEventType.QuestChanged);
        }

        public void BuyShopItem(string itemId)
        {
            if (CurrentPlayer == null) return;
            var result = ShopSystem.BuyItem(Database, CurrentPlayer, itemId, 1);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Shop, result.Message ?? UiTexts.OperationFailed));
            PublishPlayerChanges(GameEventType.ShopChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void SellShopItem(string itemId)
        {
            if (CurrentPlayer == null) return;
            var result = ShopSystem.SellItem(Database, CurrentPlayer, itemId, 1);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Shop, result.Message ?? UiTexts.OperationFailed));
            PublishPlayerChanges(GameEventType.ShopChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void EnsureAuctionHouse()
        {
            if (CurrentPlayer == null) return;
            var result = AuctionSystem.EnsureAuctionHouse(Database, CurrentPlayer, rng);
            CurrentPlayer = result.Player;
            if (result.Logs.Count == 0) return;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Auction, UiTexts.AuctionLog(log)));
            PublishPlayerChanges(GameEventType.AuctionChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void RefreshAuctionHouse()
        {
            if (CurrentPlayer == null) return;
            var result = AuctionSystem.RefreshAuctionHouse(Database, CurrentPlayer, rng);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Auction, UiTexts.AuctionLog(log)));
            PublishPlayerChanges(GameEventType.AuctionChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void BidAuctionLot(string lotId)
        {
            if (CurrentPlayer == null) return;
            var result = AuctionSystem.PlaceAuctionBid(Database, CurrentPlayer, lotId, rng);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Auction, UiTexts.AuctionLog(log)));
            PublishPlayerChanges(GameEventType.AuctionChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void SettleAuctionHouse()
        {
            if (CurrentPlayer == null) return;
            var result = AuctionSystem.SettleDueAuctions(Database, CurrentPlayer, rng, true);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Auction, UiTexts.AuctionLog(log)));
            PublishPlayerChanges(GameEventType.AuctionChanged, GameEventType.InventoryChanged, GameEventType.CurrencyChanged);
        }

        public void ConsignAuctionItem(string itemId, int count, int askPrice)
        {
            if (CurrentPlayer == null) return;
            var result = AuctionSystem.ConsignAuctionItem(Database, CurrentPlayer, itemId, count, askPrice, rng);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Auction, UiTexts.AuctionLog(log)));
            PublishPlayerChanges(GameEventType.AuctionChanged, GameEventType.InventoryChanged);
        }

        public void MineSite(string siteId)
        {
            if (CurrentPlayer == null) return;
            var result = MiningSystem.PerformMining(Database, CurrentPlayer, siteId, rng);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Mining, log));
            Publish(GameEventType.TimeAdvanced, CurrentPlayer);
            PublishPlayerChanges(GameEventType.MiningChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged, GameEventType.QuestChanged);
        }

        public void TravelToRegion(string regionId)
        {
            if (CurrentPlayer == null) return;
            var result = MapSystem.TravelTo(Database, CurrentPlayer, regionId);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Map, UiTexts.WorldActionMessage(result.Message)));
            if (result.Success)
            {
                QuestSystem.CheckQuestDiscovery(Database, CurrentPlayer, new QuestTrigger { Type = "reach_region", RegionId = regionId }, rng);
                QuestSystem.TickQuestObjectives(Database, CurrentPlayer, new QuestTrigger { Type = "reach_region", RegionId = regionId });
            }
            Publish(GameEventType.TimeAdvanced, CurrentPlayer);
            PublishPlayerChanges(GameEventType.MapChanged, GameEventType.RegionChanged, GameEventType.PlayerStatsChanged, GameEventType.QuestChanged, GameEventType.ShopChanged, GameEventType.MiningChanged, GameEventType.NpcChanged);
        }

        public void AcceptQuest(string questId)
        {
            if (CurrentPlayer == null) return;
            var result = QuestSystem.AcceptQuest(Database, CurrentPlayer, questId);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Quest, UiTexts.QuestLog(log)));
            PublishPlayerChanges(GameEventType.QuestChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged, GameEventType.CurrencyChanged);
        }

        public void DeliverQuestItem(string questId, int objectiveIndex)
        {
            if (CurrentPlayer == null) return;
            var result = QuestSystem.DeliverQuestItem(Database, CurrentPlayer, questId, objectiveIndex);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Quest, UiTexts.QuestLog(log)));
            PublishPlayerChanges(GameEventType.QuestChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged, GameEventType.CurrencyChanged);
        }

        public void TurnInQuest(string questId)
        {
            if (CurrentPlayer == null) return;
            var result = QuestSystem.TurnInQuest(Database, CurrentPlayer, questId);
            CurrentPlayer = result.Player;
            foreach (var log in result.Logs) AddLog(UiTexts.LogOperation(UiTexts.Quest, UiTexts.QuestLog(log)));
            PublishPlayerChanges(GameEventType.QuestChanged, GameEventType.InventoryChanged, GameEventType.PlayerStatsChanged, GameEventType.CurrencyChanged);
        }

        public void TrackQuest(string questId)
        {
            if (CurrentPlayer == null) return;
            var state = QuestSystem.GetQuestState(CurrentPlayer);
            state.TrackedQuestId = state.TrackedQuestId == questId ? null : questId;
            PublishPlayerChanges(GameEventType.QuestChanged);
        }

        public void MeetNpc(string npcId)
        {
            if (CurrentPlayer == null) return;
            var result = NpcSystem.MeetNpc(Database, CurrentPlayer, npcId);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Npc, UiTexts.WorldActionMessage(result.Message)));
            QuestSystem.CheckQuestDiscovery(Database, CurrentPlayer, new QuestTrigger { Type = "talk_npc", NpcId = npcId }, rng);
            QuestSystem.TickQuestObjectives(Database, CurrentPlayer, new QuestTrigger { Type = "talk_npc", NpcId = npcId });
            PublishPlayerChanges(GameEventType.NpcChanged, GameEventType.QuestChanged);
        }

        public void ChatNpc(string npcId)
        {
            if (CurrentPlayer == null) return;
            var result = NpcSystem.ChangeAffinity(Database, CurrentPlayer, npcId, 1);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Npc, UiTexts.WorldActionMessage(result.Message)));
            QuestSystem.CheckQuestDiscovery(Database, CurrentPlayer, new QuestTrigger { Type = "talk_npc", NpcId = npcId }, rng);
            QuestSystem.TickQuestObjectives(Database, CurrentPlayer, new QuestTrigger { Type = "talk_npc", NpcId = npcId });
            PublishPlayerChanges(GameEventType.NpcChanged, GameEventType.QuestChanged);
        }

        public void GiveNpcGift(string npcId, string itemId)
        {
            if (CurrentPlayer == null) return;
            var result = NpcSystem.GiveGift(Database, CurrentPlayer, npcId, itemId, rng);
            CurrentPlayer = result.Player;
            AddLog(UiTexts.LogOperation(UiTexts.Npc, UiTexts.WorldActionMessage(result.Message)));
            PublishPlayerChanges(GameEventType.NpcChanged, GameEventType.InventoryChanged);
        }

        private void AdvanceTime(int months)
        {
            if (CurrentPlayer == null || months <= 0) return;
            CurrentPlayer.Age += months;
            CurrentPlayer.GameMonth += months;
            while (CurrentPlayer.GameMonth > 12)
            {
                CurrentPlayer.GameMonth -= 12;
                CurrentPlayer.GameYear += 1;
            }
            foreach (var buff in EnlightenmentSystem.GetState(CurrentPlayer).ActiveBuffs)
                buff.RemainingMonths -= months;
            EnlightenmentSystem.GetState(CurrentPlayer).ActiveBuffs.RemoveAll(b => b.RemainingMonths <= 0);
            Publish(GameEventType.TimeAdvanced, CurrentPlayer);
        }

        private void ApplyItemEffect(string key, EffectValue value)
        {
            double Resolve(double current, double? maxValue) => value.Resolve(current, maxValue, effectRandom);
            switch (key)
            {
                case "hp": CurrentPlayer.Hp = ClampInt(Resolve(CurrentPlayer.Hp, CurrentPlayer.MaxHp), 0, CurrentPlayer.MaxHp); break;
                case "mp": CurrentPlayer.Mp = ClampInt(Resolve(CurrentPlayer.Mp, CurrentPlayer.MaxMp), 0, CurrentPlayer.MaxMp); break;
                case "stamina": CurrentPlayer.Stamina = ClampInt(Resolve(CurrentPlayer.Stamina, CurrentPlayer.MaxStamina), 0, CurrentPlayer.MaxStamina); break;
                case "mentalPower": CurrentPlayer.MentalPower = ClampInt(Resolve(CurrentPlayer.MentalPower, CurrentPlayer.MaxMentalPower), 0, CurrentPlayer.MaxMentalPower); break;
                case "mood": CurrentPlayer.Mood = ClampInt(Resolve(CurrentPlayer.Mood, 100), 0, 100); break;
                case "health": CurrentPlayer.Health = ClampInt(Resolve(CurrentPlayer.Health, 100), 0, 100); break;
                case "exp": CurrentPlayer.Exp = System.Math.Max(0, (int)Resolve(CurrentPlayer.Exp, null)); break;
                case "gold": CurrentPlayer.Gold = System.Math.Max(0, (int)Resolve(CurrentPlayer.Gold, null)); break;
                case "physique": CurrentPlayer.Physique = ClampInt(Resolve(CurrentPlayer.Physique, CurrentPlayer.MaxPhysique), 0, CurrentPlayer.MaxPhysique); break;
            }
        }

        private static int ClampInt(double value, int min, int max)
            => System.Math.Max(min, System.Math.Min(max, (int)System.Math.Round(value)));

        private static string PackageSortKey(string id)
            => id == "core" ? string.Empty : id ?? string.Empty;
    }
}
