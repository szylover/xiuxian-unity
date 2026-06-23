// ============================================================
// ProgressionState.cs — advanced progression runtime state containers
// UnityEngine-free
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Systems
{
    public sealed class StatEffect
    {
        public readonly Dictionary<string, double> StatBonuses = new();
        public readonly Dictionary<string, double> StatMultipliers = new();
        public double CultivationSpeedBonus;
        public double BreakthroughRateBonus;
    }

    public sealed class DestinyDefRuntime { public string Id, Name, Rarity, Description; public int Weight, InitialKarma; public StatEffect Effect = new(); }
    public sealed class TalentDefRuntime { public string Id, Name, Rarity, Description; public StatEffect Effect = new(); }
    public sealed class TalentTreeNodeDefRuntime { public string Id, TalentId; public int Tier, Cost; public readonly List<string> PrereqNodeIds = new(); }
    public sealed class DestinyTalentState { public int TalentPoints; public readonly List<string> UnlockedNodeIds = new(); public readonly List<string> AcquiredTalentIds = new(); }

    public sealed class EnlightenmentInsightDefRuntime { public string Id, Name, Description, Route; public int RequiredInsight; public StatEffect Effect = new(); }
    public sealed class EnlightenmentBuff { public string Id, Name; public int RemainingMonths; public StatEffect Effect = new(); }
    public sealed class EnlightenmentState { public int ComprehensionExp, InsightPoints, LastEventAge = -1, TotalTriggers; public readonly List<string> UnlockedInsightIds = new(); public readonly List<EnlightenmentBuff> ActiveBuffs = new(); }

    public sealed class KarmaEvent { public string Id, Reason; public int Delta, Age; }
    public sealed class KarmaSystemState { public int TotalGained, TotalLost, LastChangeAge = -1; public readonly List<string> MajorEvents = new(); }

    public sealed class HeartDemonDebuff { public string Id, Name; public int RemainingMonths; public double CultivationSpeedMultiplier = 0.7, BreakthroughRatePenalty = 0.12; }
    public sealed class HeartDemonHistory { public string Source, Outcome; public int Age, Value; }
    public sealed class HeartDemonSystemState { public int Value, MaxValue = 100, LastTriggerAge = -1, ConqueredCount, FailedCount; public int? SuppressedUntilAge; public readonly List<HeartDemonDebuff> ActiveDebuffs = new(); public readonly List<HeartDemonHistory> History = new(); }

    public sealed class RankingDimensionDefRuntime { public string Id, Board, ScoreKey; public int Order, Limit; }
    public sealed class RankingEntry { public string Id, Source, Name, Title, Emoji; public int RealmIndex, Score, Rank; public bool IsPlayer; }
    public sealed class RankingSnapshot { public string DimensionId; public readonly List<RankingEntry> Entries = new(); public int? PlayerRank; public int PlayerScore, RefreshedAtAge, RefreshedAtYear, RefreshedAtMonth; }
    public sealed class RankingSystemState { public readonly Dictionary<string, RankingSnapshot> Snapshots = new(); public int LastRefreshAge = -1; }

    public sealed class PvpDuelRecord { public string Id, OpponentId, OpponentName; public int OpponentRank, RankBefore, RankAfter, RewardGold, RewardExp, Age, Year, Month; public bool PlayerWon; public readonly List<string> Logs = new(); }
    public sealed class PvpSystemState { public int Rating = 1000, Wins, Losses, CooldownUntilAge; public string LastOpponentId; public readonly List<PvpDuelRecord> Records = new(); }

    public sealed class ReincarnationLegacy { public double CultivationSpeedBonus, BodyExpBonus; public int AtkBonus, DefBonus, HpBonus, MpBonus, SpeedBonus, LuckBonus, ComprehensionBonus, CharismaBonus, AptitudeBonus, SpiritRootFloor, InventoryCapacityBonus, LifespanBonus; }
    public sealed class LegacySnapshot { public int IncarnationNo, PeakRealmIndex, PeakBodyRealmIndex, Age; public string Outcome; }
    public sealed class ReincarnationState { public int Count; public readonly List<LegacySnapshot> Snapshots = new(); public ReincarnationLegacy Legacy = new(); }

    public sealed class AscensionHistoryEntry { public string FromTier, ToTier; public int AtAge, RealmIndexBefore; }
    public sealed class AscensionState { public bool HasAscended; public string CurrentTier = "mortal"; public readonly List<AscensionHistoryEntry> AscensionHistory = new(); public int AscensionFailCount; }

    public sealed class PrimordialEndgameState { public readonly List<string> AttemptedIds = new(); public string CompletedId, EndingRoute, EndingTitle, EndingText; public int? CompletedAtAge; public double LegacyMultiplierBonus; }

    public sealed class ActiveStudy { public string ScrollItemId, ScrollType, TargetId, TargetName; public int ProgressMonths, TotalMonths; }

    public sealed class AchievementDefRuntime { public string Id, Name, Description, Category, Icon; public bool Hidden; public readonly Dictionary<string, double> BonusStats = new(); }
    public sealed class AchievementSystemState { public readonly List<string> UnlockedIds = new(); public readonly List<string> PendingToast = new(); }

    public sealed class ChronicleEvent { public string Type, Description; public int Year, Month; public readonly Dictionary<string, object> Meta = new(); }
    public sealed class IncarnationRecord { public int IncarnationNo, StartedAt, FinalRealmIndex, FinalBodyRealmIndex, PeakExp, FinalAge, FinalLifespan, TotalKills, TotalDeaths, TotalRevives; public string CharacterName, CharacterGender, FinalRealmName, Outcome = "active"; public readonly List<ChronicleEvent> Events = new(); }
    public sealed class CultivationChronicle { public int SchemaVersion = 1, NextIncarnationNo = 1; public readonly List<IncarnationRecord> Incarnations = new(); public IncarnationRecord Current; }
}
