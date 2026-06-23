// ============================================================
// Batch3Panels.cs — issue #10 batch 3 concrete uGUI panels
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class SectPanel : PanelBase
    {
        public SectPanel() : base(PanelId.Sect, UiTexts.Sect) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.SectChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = SectSystem.GetSectState(p);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.SectIntro, 21);
            if (string.IsNullOrEmpty(state.SectId) || !Context.Database.Sects.TryGetValue(state.SectId, out var sect))
            {
                AddJoinView(content, p);
                return;
            }
            AddMembership(content, p, state, sect);
            AddMissions(content, p, state, sect);
            AddStore(content, state, sect);
            PanelUi.Text(content, UiTexts.SectManagementUnavailable, 18);
        }
        private void AddJoinView(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.JoinSect);
            if (Context.Database.Sects.Count == 0) PanelUi.Text(card, UiTexts.NoSects, 21);
            foreach (var sect in Context.Database.Sects.Values.OrderBy(s => s.MinRealm ?? 0).ThenBy(s => s.Id))
            {
                var s = PanelUi.Card(card, sect.Name);
                if (!string.IsNullOrEmpty(sect.Description)) PanelUi.Text(s, sect.Description, 18);
                UIBuilder.StatRow(s, UiTexts.Disposition, UiTexts.AlignmentName(sect.Alignment));
                UIBuilder.StatRow(s, UiTexts.MinRealm, Batch2Ui.RealmName(Context.Database, sect.MinRealm ?? 0));
                UIBuilder.StatRow(s, UiTexts.EntryGold, (sect.EntryGold ?? 0).ToString());
                var lockReason = SectSystem.GetJoinLockReason(Context.Database, p, sect.Id);
                if (lockReason != null) UIBuilder.StatRow(s, UiTexts.LockedReason, UiTexts.WorldSystemLog(lockReason));
                PanelUi.Button(s, UiTexts.JoinSect, () => Context.JoinSect(sect.Id), lockReason == null);
            }
        }
        private void AddMembership(Transform parent, Player p, SectSystemState state, SectDef sect)
        {
            var card = PanelUi.Card(parent, UiTexts.Membership);
            var rank = SectRank(sect, state.RankId);
            UIBuilder.StatRow(card, UiTexts.Sect, sect.Name);
            UIBuilder.StatRow(card, UiTexts.SectRank, rank?.Value<string>("name") ?? state.RankId ?? UiTexts.Unknown);
            UIBuilder.StatRow(card, UiTexts.Contribution, UiTexts.ContributionText(state.Contribution, state.TotalContribution));
            UIBuilder.StatRow(card, UiTexts.Disposition, UiTexts.AlignmentName(sect.Alignment));
            PanelUi.Button(card, UiTexts.ClaimStipend, Context.ClaimSectStipend, state.ClaimedStipendYear != p.GameYear);
            var benefits = PanelUi.Card(parent, UiTexts.SectBenefits);
            if (rank != null)
            {
                UIBuilder.StatRow(benefits, UiTexts.Gold, (rank.Value<int?>("stipendGold") ?? 0).ToString());
                UIBuilder.StatRow(benefits, UiTexts.Contribution, (rank.Value<int?>("stipendContribution") ?? 0).ToString());
            }
        }
        private void AddMissions(Transform parent, Player p, SectSystemState state, SectDef sect)
        {
            var card = PanelUi.Card(parent, UiTexts.SectMissions);
            var missions = sect.Missions?.OfType<JObject>().ToList() ?? new List<JObject>();
            if (missions.Count == 0) PanelUi.Text(card, UiTexts.NoMissions, 21);
            foreach (var mission in missions)
            {
                var id = mission.Value<string>("id");
                var m = PanelUi.Card(card, mission.Value<string>("title") ?? id);
                PanelUi.Text(m, mission.Value<string>("description"), 18);
                UIBuilder.StatRow(m, UiTexts.SectRank, RankName(sect, mission.Value<string>("minRankId")));
                UIBuilder.StatRow(m, UiTexts.Cost, UiTexts.TravelCost(mission.Value<int?>("staminaCost") ?? 0, 0));
                UIBuilder.StatRow(m, UiTexts.Cooldown, UiTexts.CooldownMonths(CooldownLeft(state, id, p.Age)));
                AddRewardRows(m, mission["reward"]);
                PanelUi.Button(m, UiTexts.DoMission, () => Context.CompleteSectMission(id), CooldownLeft(state, id, p.Age) <= 0);
            }
        }
        private void AddStore(Transform parent, SectSystemState state, SectDef sect)
        {
            var card = PanelUi.Card(parent, UiTexts.SectStore);
            var store = sect.Store?.OfType<JObject>().ToList() ?? new List<JObject>();
            if (store.Count == 0) PanelUi.Text(card, UiTexts.NoSectStore, 21);
            foreach (var item in store)
            {
                var itemId = item.Value<string>("itemId");
                var s = PanelUi.Card(card, Batch2Ui.ItemName(Context.Database, itemId));
                UIBuilder.StatRow(s, UiTexts.Contribution, (item.Value<int?>("contributionCost") ?? 0).ToString());
                UIBuilder.StatRow(s, UiTexts.SectRank, RankName(sect, item.Value<string>("minRankId")));
                UIBuilder.StatRow(s, UiTexts.Status, state.StorePurchases.ContainsKey(itemId) ? UiTexts.Unlocked : UiTexts.Locked);
            }
        }
        private static JObject SectRank(SectDef sect, string rankId) => sect.Ranks?.OfType<JObject>().FirstOrDefault(r => r.Value<string>("id") == rankId);
        private static string RankName(SectDef sect, string rankId) => SectRank(sect, rankId)?.Value<string>("name") ?? rankId ?? UiTexts.None;
        private static int CooldownLeft(SectSystemState state, string id, int age) => Math.Max(0, (state.MissionCooldowns.TryGetValue(id, out var until) ? until : 0) - age);
        private void AddRewardRows(Transform parent, JToken reward)
        {
            if (reward == null) return;
            if ((reward.Value<int?>("exp") ?? 0) != 0) UIBuilder.StatRow(parent, UiTexts.Rewards, UiTexts.CultivateGain(reward.Value<int>("exp")));
            if ((reward.Value<int?>("gold") ?? 0) != 0) UIBuilder.StatRow(parent, UiTexts.Gold, reward.Value<int>("gold").ToString());
            if ((reward.Value<int?>("contribution") ?? 0) != 0) UIBuilder.StatRow(parent, UiTexts.Contribution, reward.Value<int>("contribution").ToString());
        }
    }

    public sealed class SecretRealmPanel : PanelBase
    {
        public SecretRealmPanel() : base(PanelId.SecretRealm, UiTexts.SecretRealm) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.SecretRealmChanged || type == GameEventType.InventoryChanged || type == GameEventType.CurrencyChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = SecretRealmSystem.GetSecretRealmState(p);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.SecretRealmIntro, 21);
            if (state.ActiveRun != null) AddRun(content, state.ActiveRun);
            AddRealms(content, p, state);
            AddHistory(content, state);
        }
        private void AddRun(Transform parent, SecretRealmRun run)
        {
            var card = PanelUi.Card(parent, UiTexts.ActiveSecretRealm);
            Context.Database.SecretRealms.TryGetValue(run.RealmId, out var def);
            UIBuilder.StatRow(card, UiTexts.SecretRealm, def?.Name ?? run.RealmId);
            UIBuilder.StatRow(card, UiTexts.Stage, UiTexts.StageText(run.StageIndex, def?.Stages?.Count ?? run.StageIndex));
            foreach (var log in run.Logs.Skip(Math.Max(0, run.Logs.Count - 4))) UIBuilder.StatRow(card, UiTexts.GameLog, UiTexts.SecretRealmLog(log));
            PanelUi.Button(card, run.Completed || run.Failed ? UiTexts.FinishSecretRealm : UiTexts.AdvanceSecretRealm, run.Completed || run.Failed ? Context.FinishSecretRealm : Context.AdvanceSecretRealm, true);
        }
        private void AddRealms(Transform parent, Player p, SecretRealmSystemState state)
        {
            var card = PanelUi.Card(parent, UiTexts.AvailableSecretRealms);
            if (Context.Database.SecretRealms.Count == 0) PanelUi.Text(card, UiTexts.NoSecretRealms, 21);
            foreach (var realm in Context.Database.SecretRealms.Values.OrderBy(r => r.MinRealm ?? 0).ThenBy(r => r.Id))
            {
                var r = PanelUi.Card(card, (realm.Icon ?? string.Empty) + realm.Name);
                if (!string.IsNullOrEmpty(realm.Description)) PanelUi.Text(r, realm.Description, 18);
                UIBuilder.StatRow(r, UiTexts.MinRealm, Batch2Ui.RealmName(Context.Database, realm.MinRealm ?? 0));
                UIBuilder.StatRow(r, UiTexts.Cost, UiTexts.TravelCost(realm.EntryCost?.Value<int?>("stamina") ?? 0, 0));
                var lockReason = SecretRealmSystem.GetSecretRealmLockReason(Context.Database, p, realm.Id);
                if (lockReason != null) UIBuilder.StatRow(r, UiTexts.LockedReason, UiTexts.WorldSystemLog(lockReason));
                if (state.Cooldowns.TryGetValue(realm.Id, out var until) && until > p.Age) UIBuilder.StatRow(r, UiTexts.Cooldown, UiTexts.CooldownMonths(until - p.Age));
                PanelUi.Button(r, UiTexts.EnterSecretRealm, () => Context.StartSecretRealm(realm.Id), lockReason == null && state.ActiveRun == null);
            }
        }
        private void AddHistory(Transform parent, SecretRealmSystemState state)
        {
            var card = PanelUi.Card(parent, UiTexts.SecretRealmHistory);
            foreach (var realm in Context.Database.SecretRealms.Values)
                UIBuilder.StatRow(card, realm.Name, UiTexts.CompletedRuns(state.CompletedRuns.TryGetValue(realm.Id, out var count) ? count : 0));
        }
    }

    public sealed class BountyPanel : PanelBase
    {
        public BountyPanel() : base(PanelId.Bounty, UiTexts.Bounty) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.BountyChanged || type == GameEventType.InventoryChanged || type == GameEventType.RegionChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            Context.EnsureBountyBoard();
            var state = BountySystem.GetBountyState(Context.CurrentPlayer);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.BountyIntro, 21);
            UIBuilder.StatRow(content, UiTexts.BountyReputation, state.Reputation.ToString());
            PanelUi.Button(content, UiTexts.RefreshBounty, Context.RefreshBounties, true);
            AddActive(content, state);
            AddAvailable(content, state);
        }
        private void AddActive(Transform parent, BountySystemState state)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(UiTexts.ActiveBounties, state.Active.Count));
            if (state.Active.Count == 0) PanelUi.Text(card, UiTexts.NoActiveBounties, 21);
            foreach (var bounty in state.Active.Values)
            {
                var b = AddBountyCard(card, bounty);
                var need = bounty.Objective?.Value<int?>("count") ?? 1;
                UIBuilder.StatRow(b, UiTexts.Progress, UiTexts.BountyProgress(bounty.Progress, need));
                PanelUi.Button(b, UiTexts.ClaimBounty, () => Context.ClaimBounty(bounty.Id), bounty.Completed);
            }
        }
        private void AddAvailable(Transform parent, BountySystemState state)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionCountLabel(UiTexts.AvailableBounties, state.Available.Count));
            if (state.Available.Count == 0) PanelUi.Text(card, UiTexts.NoAvailableBounties, 21);
            foreach (var bounty in state.Available)
            {
                var b = AddBountyCard(card, bounty);
                UIBuilder.StatRow(b, UiTexts.ExpiresAt, UiTexts.ExpiresAtMonths(bounty.ExpiresAt));
                PanelUi.Button(b, UiTexts.AcceptBounty, () => Context.AcceptBounty(bounty.Id), true);
            }
        }
        private Transform AddBountyCard(Transform parent, GeneratedBounty bounty)
        {
            var card = PanelUi.Card(parent, (bounty.Icon ?? string.Empty) + bounty.Title);
            UIBuilder.StatRow(card, UiTexts.Issuer, bounty.Issuer ?? UiTexts.Unknown);
            if (!string.IsNullOrEmpty(bounty.Description)) PanelUi.Text(card, bounty.Description, 18);
            UIBuilder.StatRow(card, UiTexts.Objectives, DescribeObjective(bounty.Objective));
            return card;
        }
        private static string DescribeObjective(JToken obj)
        {
            if (obj == null) return UiTexts.Unknown;
            var type = obj.Value<string>("type");
            var target = obj.Value<string>("targetId");
            var count = obj.Value<int?>("count") ?? 1;
            return UiTexts.HaveNeed(0, count) + " " + UiTexts.WorldSystemLog(type) + " " + (target ?? UiTexts.Unknown);
        }
    }

    public sealed class CompanionPanel : PanelBase
    {
        public CompanionPanel() : base(PanelId.Companion, UiTexts.Companion) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.NpcChanged || type == GameEventType.PlayerChanged || type == GameEventType.RegionChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var npcState = NpcSystem.GetNpcState(p);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.CompanionIntro, 21);
            var current = PanelUi.Card(content, UiTexts.CurrentCompanion);
            PanelUi.Text(current, UiTexts.NoCompanion, 21);
            var candidates = npcState.DiscoveredNpcs.Select(id => Context.Database.Npcs.TryGetValue(id, out var npc) ? npc : null)
                .Where(n => n != null && NpcSystem.GetRelation(p, n.Id).Affinity >= 60).ToList();
            var card = PanelUi.Card(content, UiTexts.SectionCountLabel(UiTexts.CompanionCandidates, candidates.Count));
            PanelUi.Text(card, UiTexts.CompanionRequirement, 18);
            if (candidates.Count == 0) PanelUi.Text(card, UiTexts.NoCompanionCandidates, 21);
            foreach (var npc in candidates)
            {
                var rel = NpcSystem.GetRelation(p, npc.Id);
                var n = PanelUi.Card(card, (npc.Emoji ?? string.Empty) + npc.Name);
                UIBuilder.StatRow(n, UiTexts.QiRealm, Batch2Ui.RealmName(Context.Database, npc.RealmIndex ?? 0));
                UIBuilder.StatRow(n, UiTexts.HomeRegion, Batch2Ui.RegionName(Context.Database, npc.HomeRegionId));
                UIBuilder.StatRow(n, UiTexts.Affinity, rel.Affinity.ToString());
                UIBuilder.StatRow(n, UiTexts.Relation, UiTexts.RelationName(rel.RelationLevel));
            }
        }
    }

    public sealed class AchievementPanel : PanelBase
    {
        public AchievementPanel() : base(PanelId.Achievement, UiTexts.Achievement) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.AchievementChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.CombatChanged || type == GameEventType.CurrencyChanged;
        protected override void BuildContent(Transform parent)
        {
            var state = AchievementSystem.GetState(Context.CurrentPlayer);
            var all = AchievementSystem.GetAllAchievements();
            PanelUi.Page(parent, Title, out var content);
            UIBuilder.StatRow(content, UiTexts.AchievementProgress, UiTexts.LearnedCount(state.UnlockedIds.Count, all.Count));
            PanelUi.Button(content, UiTexts.CheckAchievements, Context.CheckAchievements, true);
            var list = PanelUi.Card(content, UiTexts.AchievementList);
            foreach (var ach in all.OrderBy(a => state.UnlockedIds.Contains(a.Id) ? 0 : 1).ThenBy(a => a.Id))
            {
                var unlocked = state.UnlockedIds.Contains(ach.Id);
                var card = PanelUi.Card(list, UiTexts.AchievementName(ach.Id));
                UIBuilder.StatRow(card, UiTexts.Status, unlocked ? UiTexts.Unlocked : UiTexts.Locked);
                PanelUi.Text(card, UiTexts.AchievementDescription(ach.Id), 18);
                UIBuilder.StatRow(card, UiTexts.Stats, UiTexts.FormatStats(ach.BonusStats));
            }
            var bonus = PanelUi.Card(content, UiTexts.AchievementBonusSummary);
            var effect = AchievementSystem.GetRecalcBonus(Context.CurrentPlayer);
            var lines = UiTexts.DescribeEffect(effect).ToList();
            if (lines.Count == 0) PanelUi.Text(bonus, UiTexts.NoAchievementBonus, 21);
            foreach (var line in lines) UIBuilder.StatRow(bonus, UiTexts.Stats, line);
        }
    }

    public sealed class ChroniclePanel : PanelBase
    {
        public ChroniclePanel() : base(PanelId.Chronicle, UiTexts.Chronicle) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.ChronicleChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.RealmChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var chronicle = Context.EnsureChronicle();
            PanelUi.Page(parent, Title, out var content);
            var record = chronicle?.Current;
            if (record == null)
            {
                PanelUi.Text(content, UiTexts.ChronicleStart, 21);
                return;
            }
            var summary = PanelUi.Card(content, UiTexts.ChronicleSummary);
            UIBuilder.StatRow(summary, UiTexts.DaoName, record.CharacterName);
            UIBuilder.StatRow(summary, UiTexts.QiRealm, record.FinalRealmName);
            UIBuilder.StatRow(summary, UiTexts.Age, UiTexts.AgeYears(record.FinalAge));
            UIBuilder.StatRow(summary, UiTexts.Stats, UiTexts.ChronicleStats(record.TotalKills, record.TotalDeaths, record.TotalRevives));
            UIBuilder.StatRow(summary, UiTexts.Outcome, record.Outcome);
            var timeline = PanelUi.Card(content, UiTexts.ChronicleTimeline);
            if (record.Events.Count == 0) PanelUi.Text(timeline, UiTexts.NoChronicleEvents, 21);
            foreach (var e in record.Events)
                UIBuilder.StatRow(timeline, UiTexts.RefreshedAtText(e.Year, e.Month), UiTexts.StatValue(UiTexts.ChronicleEventTypeName(e.Type), e.Description));
        }
    }

    public sealed class RankingPanel : PanelBase
    {
        public RankingPanel() : base(PanelId.Ranking, UiTexts.Ranking) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.RankingChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.PvpChanged || type == GameEventType.AchievementChanged;
        protected override void BuildContent(Transform parent)
        {
            RankingSystem.Refresh(Context.Database, Context.CurrentPlayer);
            var state = RankingSystem.GetState(Context.CurrentPlayer);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.RankingIntro, 21);
            PanelUi.Button(content, UiTexts.RefreshRanking, Context.RefreshRanking, true);
            foreach (var dim in RankingSystem.Dimensions.OrderBy(d => d.Board).ThenBy(d => d.Order))
            {
                if (!state.Snapshots.TryGetValue(dim.Id, out var snap)) continue;
                var card = PanelUi.Card(content, UiTexts.StatValue(UiTexts.RankingBoardName(dim.Board), UiTexts.RankingDimensionName(dim.Id)));
                UIBuilder.StatRow(card, UiTexts.PlayerRank, UiTexts.PlayerRankText(snap.PlayerRank));
                UIBuilder.StatRow(card, UiTexts.Score, snap.PlayerScore.ToString());
                UIBuilder.StatRow(card, UiTexts.RefreshedAt, UiTexts.RefreshedAtText(snap.RefreshedAtYear, snap.RefreshedAtMonth));
                foreach (var entry in snap.Entries.Take(10))
                {
                    var row = PanelUi.Card(card, UiTexts.RankPrefix(entry.Rank));
                    UIBuilder.StatRow(row, UiTexts.DaoName, (entry.Emoji ?? string.Empty) + entry.Name);
                    UIBuilder.StatRow(row, UiTexts.QiRealm, Batch2Ui.RealmName(Context.Database, entry.RealmIndex));
                    UIBuilder.StatRow(row, UiTexts.Source, UiTexts.RankingSourceName(entry.Source));
                    UIBuilder.StatRow(row, UiTexts.Score, entry.Score.ToString());
                }
            }
        }
    }

    public sealed class PvpPanel : PanelBase
    {
        public PvpPanel() : base(PanelId.Pvp, UiTexts.Pvp) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.PvpChanged || type == GameEventType.RankingChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = PvpSystem.GetState(p);
            var cooldown = Math.Max(0, state.CooldownUntilAge - p.Age);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.PvpIntro, 21);
            var summary = PanelUi.Card(content, UiTexts.PvpSummary);
            UIBuilder.StatRow(summary, UiTexts.Rating, state.Rating.ToString());
            UIBuilder.StatRow(summary, UiTexts.Record, UiTexts.PvpRecord(state.Wins, state.Losses));
            UIBuilder.StatRow(summary, UiTexts.Cooldown, UiTexts.CooldownMonths(cooldown));
            var opponents = PanelUi.Card(content, UiTexts.Opponents);
            var list = PvpSystem.GetCandidates(Context.Database, p);
            if (list.Count == 0) PanelUi.Text(opponents, UiTexts.NoOpponents, 21);
            foreach (var entry in list)
            {
                var row = PanelUi.Card(opponents, (entry.Emoji ?? string.Empty) + entry.Name);
                UIBuilder.StatRow(row, UiTexts.Rank, UiTexts.RankPrefix(entry.Rank));
                UIBuilder.StatRow(row, UiTexts.QiRealm, Batch2Ui.RealmName(Context.Database, entry.RealmIndex));
                UIBuilder.StatRow(row, UiTexts.Score, entry.Score.ToString());
                PanelUi.Button(row, UiTexts.Challenge, () => Context.ChallengePvp(entry.Id), cooldown <= 0);
            }
            var history = PanelUi.Card(content, UiTexts.PvpHistory);
            if (state.Records.Count == 0) PanelUi.Text(history, UiTexts.NoPvpHistory, 21);
            foreach (var record in state.Records.Take(10))
            {
                var row = PanelUi.Card(history, record.OpponentName);
                UIBuilder.StatRow(row, UiTexts.Status, record.PlayerWon ? UiTexts.Conquered : UiTexts.Failed);
                UIBuilder.StatRow(row, UiTexts.Rewards, UiTexts.PvpReward(record.RewardExp, record.RewardGold));
            }
        }
    }

    public sealed class HeartDemonPanel : PanelBase
    {
        public HeartDemonPanel() : base(PanelId.HeartDemon, UiTexts.HeartDemon) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.HeartDemonChanged || type == GameEventType.EnlightenmentChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;
        protected override void BuildContent(Transform parent)
        {
            var state = HeartDemonSystem.GetState(Context.CurrentPlayer);
            PanelUi.Page(parent, Title, out var content);
            PanelUi.Text(content, UiTexts.HeartDemonIntro, 21);
            var value = PanelUi.Card(content, UiTexts.HeartDemonValue);
            UIBuilder.StatRow(value, UiTexts.Status, UiTexts.HeartDemonState(state.Value));
            UIBuilder.StatRow(value, UiTexts.HeartDemonValue, UiTexts.HeartDemonValueLine(state.Value, state.MaxValue));
            UIBuilder.ProgressBar(value, state.Value, state.MaxValue);
            UIBuilder.StatRow(value, UiTexts.MentalCost, UiTexts.HeartDemonSuppressCost());
            var actions = UIBuilder.Rect("HeartDemonActions", value);
            UIBuilder.Horizontal(actions, 4, 8);
            PanelUi.Button(actions.transform, UiTexts.SuppressHeartDemon, Context.SuppressHeartDemon, true);
            PanelUi.Button(actions.transform, UiTexts.ConfrontHeartDemon, Context.ConfrontHeartDemon, true);
            var effects = PanelUi.Card(content, UiTexts.HeartDemonEffects);
            UIBuilder.StatRow(effects, UiTexts.Conquered, state.ConqueredCount.ToString());
            UIBuilder.StatRow(effects, UiTexts.Failed, state.FailedCount.ToString());
            if (state.ActiveDebuffs.Count == 0) PanelUi.Text(effects, UiTexts.NoHeartDemonDebuff, 21);
            foreach (var buff in state.ActiveDebuffs) UIBuilder.StatRow(effects, UiTexts.Status, UiTexts.HeartDemonDebuffLine(buff.Name, buff.RemainingMonths));
            var history = PanelUi.Card(content, UiTexts.HeartDemonHistory);
            if (state.History.Count == 0) PanelUi.Text(history, UiTexts.NoHeartDemonHistory, 21);
            foreach (var h in state.History.Take(10)) UIBuilder.StatRow(history, UiTexts.CompletedAtMonths(h.Age), UiTexts.HeartDemonHistoryLine(h.Age, h.Source, h.Outcome, h.Value));
        }
    }
}
