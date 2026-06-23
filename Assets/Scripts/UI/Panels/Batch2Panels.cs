// ============================================================
// Batch2Panels.cs — issue #10 batch 2 concrete uGUI panels
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class AlchemyPanel : PanelBase
    {
        public AlchemyPanel() : base(PanelId.Alchemy, UiTexts.Alchemy) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.AlchemyChanged || type == GameEventType.InventoryChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;
        protected override void BuildContent(Transform parent)
        {
            PanelUi.Page(parent, Title, out var content);
            Batch2Ui.AddMentalHeader(content, Context.CurrentPlayer);
            Batch2Ui.AddAlchemyRecipes(content, Context, UiTexts.RecipeList);
            PanelUi.Text(content, UiTexts.LearnedRecipeHint, 20);
        }
    }

    public sealed class SmithingPanel : PanelBase
    {
        public SmithingPanel() : base(PanelId.Smithing, UiTexts.Smithing) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.SmithingChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;
        protected override void BuildContent(Transform parent)
        {
            PanelUi.Page(parent, Title, out var content);
            Batch2Ui.AddMentalHeader(content, Context.CurrentPlayer);
            Batch2Ui.AddSmithingRecipes(content, Context, UiTexts.SmithingRecipeList);
            PanelUi.Text(content, UiTexts.LearnedRecipeHint, 20);
        }
    }

    public sealed class CraftingPanel : PanelBase
    {
        public CraftingPanel() : base(PanelId.Crafting, UiTexts.Crafting) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.AlchemyChanged || type == GameEventType.SmithingChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;
        protected override void BuildContent(Transform parent)
        {
            PanelUi.Page(parent, Title, out var content);
            Batch2Ui.AddMentalHeader(content, Context.CurrentPlayer);
            Batch2Ui.AddAlchemyRecipes(content, Context, UiTexts.Alchemy);
            Batch2Ui.AddSmithingRecipes(content, Context, UiTexts.Smithing);
        }
    }

    public sealed class ShopPanel : PanelBase
    {
        public ShopPanel() : base(PanelId.Shop, UiTexts.Shop) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.ShopChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            UIBuilder.StatRow(content, UiTexts.Gold, p.Gold.ToString());
            var buy = PanelUi.Card(content, UiTexts.ShopBuyList);
            var goods = ShopSystem.GetShopGoodsForRegion(Context.Database, p).ToList();
            if (goods.Count == 0) PanelUi.Text(buy, UiTexts.MerchantAbsent, 21);
            foreach (var good in goods)
            {
                if (!Context.Database.Items.TryGetValue(good.ItemId, out var def)) continue;
                var card = PanelUi.Card(buy, def.Name);
                var price = ShopSystem.CalcKarmaBuyPrice(good.BuyPrice ?? 0, p.Charisma, p.Karma);
                UIBuilder.StatRow(card, UiTexts.UnitPrice, price.ToString());
                UIBuilder.StatRow(card, UiTexts.OriginalPrice, (good.BuyPrice ?? 0).ToString());
                UIBuilder.StatRow(card, UiTexts.Stock, (good.Stock ?? 0).ToString());
                UIBuilder.StatRow(card, UiTexts.Rarity, UiTexts.RarityName(def.Rarity));
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(card, def.Description, 18);
                PanelUi.Button(card, UiTexts.Buy, () => Context.BuyShopItem(good.ItemId), p.Gold >= price);
            }

            var sell = PanelUi.Card(content, UiTexts.ShopSellList);
            if (p.Inventory.Count == 0) PanelUi.Text(sell, UiTexts.EmptyBagShort, 21);
            foreach (var slot in p.Inventory)
            {
                Context.Database.Items.TryGetValue(slot.ItemId, out var def);
                var card = PanelUi.Card(sell, def?.Name ?? slot.ItemId);
                UIBuilder.StatRow(card, UiTexts.Count, slot.Count.ToString());
                UIBuilder.StatRow(card, UiTexts.SellPrice, (def?.SellPrice ?? 0).ToString());
                if (!string.IsNullOrEmpty(def?.Description)) PanelUi.Text(card, def.Description, 18);
                PanelUi.Button(card, UiTexts.Sell, () => Context.SellShopItem(slot.ItemId), def != null && slot.Count > 0);
            }
        }
    }

    public sealed class AuctionPanel : PanelBase
    {
        public AuctionPanel() : base(PanelId.Auction, UiTexts.Auction) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.AuctionChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.TimeAdvanced || type == GameEventType.PlayerChanged;
        protected override void BuildContent(Transform parent)
        {
            Context.EnsureAuctionHouse();
            var p = Context.CurrentPlayer;
            var state = AuctionSystem.GetAuctionState(p);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.AuctionIntro, 21);
            UIBuilder.StatRow(content, UiTexts.Gold, p.Gold.ToString());
            UIBuilder.StatRow(content, UiTexts.TimeLeft, UiTexts.TimeLeftMonths(AuctionSystem.GetAuctionTimeLeft(p)));
            var actions = UIBuilder.Rect("AuctionActions", content);
            UIBuilder.Horizontal(actions, 4, 8);
            PanelUi.Button(actions.transform, UiTexts.RefreshAuction, Context.RefreshAuctionHouse, true);
            PanelUi.Button(actions.transform, UiTexts.SettleAuction, Context.SettleAuctionHouse, true);

            var lots = PanelUi.Card(content, UiTexts.AuctionLots);
            if (state.Lots.Count == 0) PanelUi.Text(lots, UiTexts.EmptyAuctionLots, 21);
            foreach (var lot in state.Lots)
            {
                var itemName = Batch2Ui.ItemName(Context.Database, lot.ItemId);
                var card = PanelUi.Card(lots, UiTexts.ItemCount(itemName, lot.Count));
                UIBuilder.StatRow(card, UiTexts.BasePrice, lot.BasePrice.ToString());
                UIBuilder.StatRow(card, UiTexts.CurrentBid, lot.CurrentBid.ToString());
                UIBuilder.StatRow(card, UiTexts.HighestBidder, lot.HighestBidder == "player" ? UiTexts.PlayerBidder : lot.HighestBidder == "none" ? UiTexts.NoBidder : lot.HighestBidder);
                var nextBid = AuctionSystem.GetNextBid(lot.CurrentBid == 0 ? lot.BasePrice : lot.CurrentBid);
                var extra = nextBid - (lot.HighestBidder == "player" ? lot.PlayerBid : 0);
                PanelUi.Button(card, UiTexts.CostText(UiTexts.Bid, nextBid), () => Context.BidAuctionLot(lot.Id), p.Gold >= extra);
            }

            var consignments = PanelUi.Card(content, UiTexts.AuctionConsignments);
            if (state.Consignments.Count == 0) PanelUi.Text(consignments, UiTexts.EmptyAuctionConsignments, 21);
            foreach (var item in state.Consignments)
            {
                UIBuilder.StatRow(consignments, UiTexts.ItemCount(Batch2Ui.ItemName(Context.Database, item.ItemId), item.Count), UiTexts.StatValue(UiTexts.AskPrice, item.AskPrice));
                UIBuilder.StatRow(consignments, UiTexts.TimeLeft, UiTexts.TimeLeftMonths(Math.Max(0, item.EndsAt - p.Age)));
            }
            foreach (var slot in p.Inventory)
            {
                var def = Context.Database.Items.TryGetValue(slot.ItemId, out var d) ? d : null;
                var ask = Math.Max(1, (def?.SellPrice ?? 1) * 3);
                var card = PanelUi.Card(consignments, UiTexts.ItemCount(def?.Name ?? slot.ItemId, slot.Count));
                UIBuilder.StatRow(card, UiTexts.AskPrice, ask.ToString());
                PanelUi.Button(card, UiTexts.Consign, () => Context.ConsignAuctionItem(slot.ItemId, 1, ask), slot.Count > 0);
            }

            var history = PanelUi.Card(content, UiTexts.AuctionHistory);
            if (state.History.Count == 0) PanelUi.Text(history, UiTexts.EmptyAuctionHistory, 21);
            foreach (var line in state.History.Take(12)) UIBuilder.StatRow(history, UiTexts.Auction, UiTexts.AuctionLog(line));
        }
    }

    public sealed class MiningPanel : PanelBase
    {
        public MiningPanel() : base(PanelId.Mining, UiTexts.Mining) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.MiningChanged || type == GameEventType.InventoryChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = MiningSystem.GetMiningState(p);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.MiningIntro, 21);
            UIBuilder.StatRow(content, UiTexts.MiningSummary, UiTexts.MiningTotal(state.MinedCount, state.TotalFengShui));
            UIBuilder.StatRow(content, UiTexts.LastMiningSite, string.IsNullOrEmpty(state.LastSiteId) ? UiTexts.None : state.LastSiteId);
            var sites = PanelUi.Card(content, UiTexts.AvailableMiningSites);
            var list = MiningSystem.GetAvailableMiningSites(Context.Database, p).ToList();
            if (list.Count == 0) PanelUi.Text(sites, UiTexts.EmptyMiningSites, 21);
            foreach (var site in list)
            {
                var card = PanelUi.Card(sites, (site.Icon ?? string.Empty) + site.Name);
                if (!string.IsNullOrEmpty(site.Description)) PanelUi.Text(card, site.Description, 18);
                var feng = ReadInt(site.FengShui, 0);
                UIBuilder.StatRow(card, UiTexts.FengShui, UiTexts.StatValue(feng.ToString(), UiTexts.FengShuiGrade(MiningSystem.GetFengShuiGrade(feng))));
                UIBuilder.StatRow(card, UiTexts.MinRealm, Batch2Ui.RealmName(Context.Database, site.MinRealm ?? 0));
                UIBuilder.StatRow(card, UiTexts.Cost, UiTexts.TravelCost(site.StaminaCost ?? 0, site.Months ?? 0));
                var lockReason = MiningSystem.GetMiningLockReason(Context.Database, p, site.Id);
                if (!string.IsNullOrEmpty(lockReason)) UIBuilder.StatRow(card, UiTexts.LockedReason, lockReason);
                PanelUi.Button(card, UiTexts.Mine, () => Context.MineSite(site.Id), lockReason == null);
            }
        }
        private static int ReadInt(JToken token, int fallback) => token == null ? fallback : token.Value<int?>() ?? fallback;
    }

    public sealed class MapPanel : PanelBase
    {
        public MapPanel() : base(PanelId.Map, UiTexts.Map) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.MapChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            MapSystem.RefreshUnlockedRegions(Context.Database, p);
            var state = MapSystem.GetMapState(Context.Database, p);
            var current = MapSystem.GetCurrentRegion(Context.Database, p);
            PanelUi.Page(parent, Title, out var content);
            UIBuilder.StatRow(content, UiTexts.CurrentRegion, current == null ? UiTexts.Unknown : (current.Emoji ?? string.Empty) + current.Name);
            var regions = PanelUi.Card(content, UiTexts.SectionCountLabel(UiTexts.WorldRegions, Context.Database.Regions.Count));
            foreach (var region in Context.Database.Regions.Values.OrderBy(r => r.ParentId).ThenBy(r => r.MinRealm ?? 0).ThenBy(r => r.Id))
            {
                var isCurrent = state.CurrentRegionId == region.Id;
                var lockReason = MapSystem.GetRegionAccessLockReason(Context.Database, p, region.Id);
                var card = PanelUi.Card(regions, (region.Emoji ?? string.Empty) + region.Name);
                if (!string.IsNullOrEmpty(region.Description)) PanelUi.Text(card, region.Description, 18);
                UIBuilder.StatRow(card, UiTexts.MinRealm, UiTexts.RegionMinRealm(Batch2Ui.RealmName(Context.Database, region.MinRealm ?? 0), Batch2Ui.BodyRealmName(Context.Database, region.MinRealm ?? 0)));
                UIBuilder.StatRow(card, UiTexts.Cost, UiTexts.TravelCost(MapSystem.CalcTravelCost(p, region), region.TravelTimeMonths ?? 0));
                UIBuilder.StatRow(card, UiTexts.Category, (region.IsContainer ?? false) ? UiTexts.WorldRegions : ((region.SafeZone ?? false) ? UiTexts.SafeZone : UiTexts.World));
                if (region.RegionTags != null && region.RegionTags.Count > 0) UIBuilder.StatRow(card, UiTexts.RegionTags, string.Join(" / ", region.RegionTags));
                if (isCurrent) UIBuilder.StatRow(card, UiTexts.Status, UiTexts.Here);
                else if (lockReason != null) UIBuilder.StatRow(card, UiTexts.LockedReason, UiTexts.WorldActionMessage(lockReason));
                PanelUi.Button(card, UiTexts.Travel, () => Context.TravelToRegion(region.Id), !isCurrent && !(region.IsContainer ?? false) && lockReason == null && p.Stamina >= MapSystem.CalcTravelCost(p, region));
            }
        }
    }

    public sealed class QuestPanel : PanelBase
    {
        public QuestPanel() : base(PanelId.Quest, UiTexts.Quest) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.QuestChanged || type == GameEventType.InventoryChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = QuestSystem.GetQuestState(p);
            PanelUi.Page(parent, Title, out var content);
            AddTracked(content, state);
            AddActive(content, state);
            AddDiscovered(content, state);
            AddCompleted(content, state);
        }
        private void AddTracked(Transform parent, QuestSystemState state)
        {
            if (string.IsNullOrEmpty(state.TrackedQuestId) || !state.ActiveQuests.TryGetValue(state.TrackedQuestId, out var progress)) return;
            if (!Context.Database.QuestChains.TryGetValue(progress.QuestId, out var def)) return;
            var card = PanelUi.Card(parent, UiTexts.TrackedQuest);
            AddQuestHeader(card, def);
            AddProgress(card, def, progress);
        }
        private void AddActive(Transform parent, QuestSystemState state)
        {
            var entries = state.ActiveQuests.Values.ToList();
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(UiTexts.ActiveQuests, entries.Count));
            if (entries.Count == 0) PanelUi.Text(card, UiTexts.NoActiveQuests, 21);
            foreach (var progress in entries)
            {
                if (!Context.Database.QuestChains.TryGetValue(progress.QuestId, out var def)) continue;
                var q = PanelUi.Card(card, def.Name);
                AddQuestHeader(q, def);
                AddProgress(q, def, progress);
                var row = UIBuilder.Rect("QuestActions", q);
                UIBuilder.Horizontal(row, 4, 8);
                PanelUi.Button(row.transform, state.TrackedQuestId == progress.QuestId ? UiTexts.UntrackQuest : UiTexts.TrackQuest, () => Context.TrackQuest(progress.QuestId), true);
                PanelUi.Button(row.transform, UiTexts.TurnInQuest, () => Context.TurnInQuest(progress.QuestId), progress.Status == "pending_turnin");
            }
        }
        private void AddDiscovered(Transform parent, QuestSystemState state)
        {
            var ids = state.DiscoveredQuests.ToList();
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(UiTexts.DiscoveredQuests, ids.Count));
            if (ids.Count == 0) PanelUi.Text(card, UiTexts.NoDiscoveredQuests, 21);
            foreach (var id in ids)
            {
                if (!Context.Database.QuestChains.TryGetValue(id, out var def)) continue;
                var q = PanelUi.Card(card, def.Name);
                AddQuestHeader(q, def);
                PanelUi.Button(q, UiTexts.AcceptQuest, () => Context.AcceptQuest(id), true);
            }
        }
        private void AddCompleted(Transform parent, QuestSystemState state)
        {
            var entries = state.CompletedQuests.Values.ToList();
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(UiTexts.CompletedQuests, entries.Count));
            if (entries.Count == 0) PanelUi.Text(card, UiTexts.NoCompletedQuests, 21);
            foreach (var entry in entries)
            {
                if (!Context.Database.QuestChains.TryGetValue(entry.QuestId, out var def)) continue;
                var q = PanelUi.Card(card, def.Name);
                UIBuilder.StatRow(q, UiTexts.CompletedAt, UiTexts.CompletedAtMonths(entry.CompletedAt));
                AddQuestHeader(q, def);
            }
        }
        private static void AddQuestHeader(Transform parent, QuestChainDef def)
        {
            UIBuilder.StatRow(parent, UiTexts.Category, def.Category ?? UiTexts.Unknown);
            if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(parent, def.Description, 18);
        }
        private void AddProgress(Transform parent, QuestChainDef def, QuestProgress progress)
        {
            if (progress.Status == "pending_turnin")
            {
                var npcName = !string.IsNullOrEmpty(def.TurnInNpcId) && Context.Database.Npcs.TryGetValue(def.TurnInNpcId, out var npc) ? npc.Name : UiTexts.Unknown;
                UIBuilder.StatRow(parent, UiTexts.Status, UiTexts.QuestPendingTurnIn(npcName));
                return;
            }
            var step = def.Steps != null && progress.CurrentStepIndex >= 0 && progress.CurrentStepIndex < def.Steps.Count ? def.Steps[progress.CurrentStepIndex] as JObject : null;
            if (step == null) return;
            UIBuilder.StatRow(parent, UiTexts.Step, step.Value<string>("name") ?? progress.CurrentStepIndex.ToString());
            var objectives = (step["objectives"] as JArray)?.OfType<JObject>().ToList() ?? new List<JObject>();
            for (var i = 0; i < objectives.Count; i++)
            {
                var obj = objectives[i];
                var op = progress.ObjectiveProgress.ElementAtOrDefault(i);
                var need = obj.Value<int?>("count") ?? 1;
                var label = obj.Value<string>("description") ?? obj.Value<string>("targetId") ?? UiTexts.Objectives;
                UIBuilder.StatRow(parent, label, op?.Completed == true ? UiTexts.Unlocked : UiTexts.HaveNeed(op?.CurrentCount ?? 0, need));
                if (obj.Value<string>("type") == "deliver_item") PanelUi.Button(parent, UiTexts.DeliverItem, () => Context.DeliverQuestItem(progress.QuestId, obj.Value<int?>("objectiveIndex") ?? i), op?.Completed != true);
            }
        }
    }

    public sealed class NpcPanel : PanelBase
    {
        public NpcPanel() : base(PanelId.Npc, UiTexts.Npc) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.NpcChanged || type == GameEventType.RegionChanged || type == GameEventType.QuestChanged || type == GameEventType.InventoryChanged || type == GameEventType.PlayerChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = NpcSystem.GetNpcState(p);
            var current = MapSystem.GetCurrentRegion(Context.Database, p);
            PanelUi.Page(parent, Title, out var content);
            UIBuilder.StatRow(content, UiTexts.CurrentRegion, current == null ? UiTexts.Unknown : (current.Emoji ?? string.Empty) + current.Name);
            AddNpcList(content, UiTexts.RegionNpcs, NpcSystem.GetNpcsInRegion(Context.Database, p));
            var contacts = state.DiscoveredNpcs.Select(id => Context.Database.Npcs.TryGetValue(id, out var npc) ? npc : null).Where(n => n != null).ToList();
            AddNpcList(content, UiTexts.Contacts, contacts);
        }
        private void AddNpcList(Transform parent, string title, List<NpcDef> npcs)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(title, npcs.Count));
            if (npcs.Count == 0) PanelUi.Text(card, title == UiTexts.RegionNpcs ? UiTexts.NoRegionNpcs : UiTexts.NoContacts, 21);
            foreach (var npc in npcs)
            {
                var rel = NpcSystem.GetRelation(Context.CurrentPlayer, npc.Id);
                var n = PanelUi.Card(card, (npc.Emoji ?? string.Empty) + npc.Name);
                UIBuilder.StatRow(n, UiTexts.Role, npc.Title ?? UiTexts.None);
                UIBuilder.StatRow(n, UiTexts.Relation, UiTexts.RelationName(rel.RelationLevel));
                UIBuilder.StatRow(n, UiTexts.Affinity, rel.Affinity.ToString());
                UIBuilder.StatRow(n, UiTexts.QiRealm, Batch2Ui.RealmName(Context.Database, npc.RealmIndex ?? 0));
                UIBuilder.StatRow(n, UiTexts.Personality, npc.Personality ?? UiTexts.Unknown);
                UIBuilder.StatRow(n, UiTexts.Disposition, UiTexts.AlignmentName(npc.Alignment));
                UIBuilder.StatRow(n, UiTexts.HomeRegion, Batch2Ui.RegionName(Context.Database, npc.HomeRegionId));
                if (!string.IsNullOrEmpty(npc.Description)) PanelUi.Text(n, npc.Description, 18);
                AddNpcQuests(n, npc.Id);
                var row = UIBuilder.Rect("NpcActions", n);
                UIBuilder.Horizontal(row, 4, 8);
                PanelUi.Button(row.transform, UiTexts.MeetNpc, () => Context.MeetNpc(npc.Id), !rel.Met);
                PanelUi.Button(row.transform, UiTexts.ChatNpc, () => Context.ChatNpc(npc.Id), rel.Met);
                var gift = Context.CurrentPlayer.Inventory.FirstOrDefault();
                PanelUi.Button(row.transform, UiTexts.GiftNpc, () => Context.GiveNpcGift(npc.Id, gift?.ItemId), rel.Met && gift != null);
            }
        }
        private void AddNpcQuests(Transform parent, string npcId)
        {
            var qstate = QuestSystem.GetQuestState(Context.CurrentPlayer);
            var available = qstate.DiscoveredQuests.Where(id => Context.Database.QuestChains.TryGetValue(id, out var def) && IsNpcQuest(def, npcId)).ToList();
            var turnIn = qstate.ActiveQuests.Values.Where(p => p.Status == "pending_turnin" && Context.Database.QuestChains.TryGetValue(p.QuestId, out var def) && def.TurnInNpcId == npcId).ToList();
            if (available.Count == 0 && turnIn.Count == 0) return;
            UIBuilder.SectionHeader(parent, UiTexts.QuestsAtNpc);
            foreach (var id in available)
            {
                var def = Context.Database.QuestChains[id];
                var row = UIBuilder.Rect("NpcQuest", parent);
                UIBuilder.Horizontal(row, 4, 8);
                UIBuilder.Layout(UIBuilder.Label(row.transform, UiTexts.StatValue(UiTexts.Available, def.Name), 20, TextAlignmentOptions.Left).gameObject, flexibleWidth: 1, preferredHeight: 42);
                PanelUi.Button(row.transform, UiTexts.AcceptQuest, () => Context.AcceptQuest(id), true, 110);
            }
            foreach (var progress in turnIn)
            {
                var def = Context.Database.QuestChains[progress.QuestId];
                var row = UIBuilder.Rect("NpcTurnIn", parent);
                UIBuilder.Horizontal(row, 4, 8);
                UIBuilder.Layout(UIBuilder.Label(row.transform, UiTexts.StatValue(UiTexts.CanTurnIn, def.Name), 20, TextAlignmentOptions.Left).gameObject, flexibleWidth: 1, preferredHeight: 42);
                PanelUi.Button(row.transform, UiTexts.TurnInQuest, () => Context.TurnInQuest(progress.QuestId), true, 130);
            }
        }
        private static bool IsNpcQuest(QuestChainDef def, string npcId)
        {
            if (def.TurnInNpcId == npcId) return true;
            return def.DiscoverSource is JObject ds && ds.Value<string>("type") == "npc" && ds.Value<string>("npcId") == npcId;
        }
    }

    internal static class Batch2Ui
    {
        public static void AddMentalHeader(Transform parent, Player p)
        {
            UIBuilder.StatRow(parent, UiTexts.MentalPower, UiTexts.CurrentMax(p.MentalPower, p.MaxMentalPower));
            UIBuilder.ProgressBar(parent, p.MentalPower, p.MaxMentalPower);
        }
        public static void AddAlchemyRecipes(Transform parent, GameContext context, string title)
        {
            var p = context.CurrentPlayer;
            var learned = LearningSystem.GetState(p).LearnedRecipes ?? Array.Empty<string>();
            var recipes = context.Database.Recipes.Values.Where(r => learned.Contains(r.Id) && p.RealmIndex >= (r.MinRealm ?? 0)).OrderBy(r => r.MinRealm ?? 0).ThenBy(r => r.Id).ToList();
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(title, recipes.Count));
            if (recipes.Count == 0) PanelUi.Text(card, UiTexts.NoAvailableRecipes, 21);
            foreach (var recipe in recipes)
            {
                var r = PanelUi.Card(card, recipe.Name);
                if (!string.IsNullOrEmpty(recipe.Description)) PanelUi.Text(r, recipe.Description, 18);
                UIBuilder.StatRow(r, UiTexts.SuccessRate, UiTexts.Percent(AlchemySystem.CalcSuccessRate(p, recipe) * 100));
                UIBuilder.StatRow(r, UiTexts.Output, UiTexts.RecipeOutput(ItemName(context.Database, recipe.OutputItemId), recipe.OutputCount ?? 1));
                UIBuilder.StatRow(r, UiTexts.MentalCost, (recipe.MentalCost ?? 0).ToString());
                AddInputs(r, context.Database, p, recipe.Inputs);
                PanelUi.Button(r, UiTexts.BrewAlchemy, () => context.BrewAlchemy(recipe.Id), AlchemySystem.CanCraft(context.Database, p, recipe.Id));
            }
        }
        public static void AddSmithingRecipes(Transform parent, GameContext context, string title)
        {
            var p = context.CurrentPlayer;
            var learned = LearningSystem.GetState(p).LearnedSmithingRecipes ?? Array.Empty<string>();
            var recipes = context.Database.SmithingRecipes.Values.Where(r => learned.Contains(r.Id) && p.RealmIndex >= (r.MinRealm ?? 0)).OrderBy(r => r.MinRealm ?? 0).ThenBy(r => r.Id).ToList();
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(title, recipes.Count));
            if (recipes.Count == 0) PanelUi.Text(card, UiTexts.NoAvailableSmithingRecipes, 21);
            foreach (var recipe in recipes)
            {
                var r = PanelUi.Card(card, recipe.Name);
                if (!string.IsNullOrEmpty(recipe.Description)) PanelUi.Text(r, recipe.Description, 18);
                UIBuilder.StatRow(r, UiTexts.SuccessRate, UiTexts.Percent(SmithingSystem.CalcSmithingSuccessRate(p, recipe) * 100));
                UIBuilder.StatRow(r, UiTexts.Output, ItemOrEquipName(context.Database, recipe.OutputItemId));
                UIBuilder.StatRow(r, UiTexts.MentalCost, (recipe.MentalCost ?? 0).ToString());
                UIBuilder.StatRow(r, UiTexts.GoldCost, (recipe.GoldCost ?? 0).ToString());
                AddInputs(r, context.Database, p, recipe.Inputs);
                PanelUi.Button(r, UiTexts.ForgeSmithing, () => context.ForgeSmithing(recipe.Id), SmithingSystem.CanSmith(context.Database, p, recipe.Id));
            }
        }
        private static void AddInputs(Transform parent, GameDatabase db, Player p, List<ItemCountDef> inputs)
        {
            UIBuilder.SectionHeader(parent, UiTexts.Materials);
            foreach (var input in inputs ?? new List<ItemCountDef>()) UIBuilder.StatRow(parent, ItemName(db, input.ItemId), UiTexts.HaveNeed(InventorySystem.CountItem(p, input.ItemId), input.Count));
        }
        public static string ItemName(GameDatabase db, string itemId) => db.Items.TryGetValue(itemId, out var def) ? def.Name : itemId ?? UiTexts.Unknown;
        public static string ItemOrEquipName(GameDatabase db, string itemId) => db.Items.TryGetValue(itemId, out var item) ? item.Name : db.Equips.TryGetValue(itemId, out var equip) ? equip.Name : itemId ?? UiTexts.Unknown;
        public static string RealmName(GameDatabase db, int index) => db.Realms.TryGetValue(index, out var def) ? def.Name : UiTexts.RealmUnknown;
        public static string BodyRealmName(GameDatabase db, int index) => db.BodyRealms.TryGetValue(index, out var def) ? def.Name : null;
        public static string RegionName(GameDatabase db, string id) => !string.IsNullOrEmpty(id) && db.Regions.TryGetValue(id, out var def) ? def.Name : UiTexts.Unknown;
    }
}
