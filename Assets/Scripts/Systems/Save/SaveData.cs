// ============================================================
// SaveData.cs — explicit save schema for player + subsystem state
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xiuxian.Systems.Procedural;

namespace Xiuxian.Systems
{
    public sealed class SaveData
    {
        public int SaveVersion;
        public long SavedAt;
        public Player Player;
        public SaveSystemStates Systems = new();
    }

    public sealed class SaveSystemStates
    {
        public MapSystemState Map;
        public EventRuntimeState Events;
        public QuestSystemState Quest;
        public DialogueSystemState Dialogue;
        public NpcSystemState Npc;
        public SectSystemState Sect;
        public SecretRealmSystemState SecretRealm;
        public BountySystemState Bounty;

        public DivineArtsSystemState DivineArts;
        public AuctionSystemState Auction;
        public MiningSystemState Mining;
        public LearningState Learning;
        public ProceduralItemState ProceduralItemState;
        public DeathSystemState Death;

        public DestinyTalentState Destiny;
        public EnlightenmentState Enlightenment;
        public KarmaSystemState Karma;
        public HeartDemonSystemState HeartDemon;
        public RankingSystemState Ranking;
        public PvpSystemState Pvp;
        public ReincarnationState Reincarnation;
        public AscensionState Ascension;
        public PrimordialEndgameState PrimordialEndgame;
        public AchievementSystemState Achievement;
        public CultivationChronicle Chronicle;

        public ProceduralEventState Procedural;
        public ProceduralMonsterState ProceduralMonsterState;
        public ProceduralTechniqueState ProceduralTechniqueState;

        public readonly Dictionary<string, JToken> Other = new();

        public static SaveSystemStates Capture(Player player)
        {
            var states = new SaveSystemStates();
            Capture(player, "map", x => states.Map = Cast<MapSystemState>(x));
            Capture(player, "events", x => states.Events = Cast<EventRuntimeState>(x));
            Capture(player, "quest", x => states.Quest = Cast<QuestSystemState>(x));
            Capture(player, "dialogue", x => states.Dialogue = Cast<DialogueSystemState>(x));
            Capture(player, "npc", x => states.Npc = Cast<NpcSystemState>(x));
            Capture(player, "sect", x => states.Sect = Cast<SectSystemState>(x));
            Capture(player, "secretRealm", x => states.SecretRealm = Cast<SecretRealmSystemState>(x));
            Capture(player, "bounty", x => states.Bounty = Cast<BountySystemState>(x));
            Capture(player, "divineArts", x => states.DivineArts = Cast<DivineArtsSystemState>(x));
            Capture(player, "auction", x => states.Auction = Cast<AuctionSystemState>(x));
            Capture(player, "mining", x => states.Mining = Cast<MiningSystemState>(x));
            Capture(player, "learning", x => states.Learning = Cast<LearningState>(x));
            Capture(player, "proceduralItemState", x => states.ProceduralItemState = Cast<ProceduralItemState>(x));
            Capture(player, "death", x => states.Death = Cast<DeathSystemState>(x));
            Capture(player, "destiny", x => states.Destiny = Cast<DestinyTalentState>(x));
            Capture(player, "enlightenment", x => states.Enlightenment = Cast<EnlightenmentState>(x));
            Capture(player, "karma", x => states.Karma = Cast<KarmaSystemState>(x));
            Capture(player, "heartDemon", x => states.HeartDemon = Cast<HeartDemonSystemState>(x));
            Capture(player, "ranking", x => states.Ranking = Cast<RankingSystemState>(x));
            Capture(player, "pvp", x => states.Pvp = Cast<PvpSystemState>(x));
            Capture(player, "reincarnation", x => states.Reincarnation = Cast<ReincarnationState>(x));
            Capture(player, "ascension", x => states.Ascension = Cast<AscensionState>(x));
            Capture(player, "primordialEndgame", x => states.PrimordialEndgame = Cast<PrimordialEndgameState>(x));
            Capture(player, "achievement", x => states.Achievement = Cast<AchievementSystemState>(x));
            Capture(player, "chronicle", x => states.Chronicle = Cast<CultivationChronicle>(x));
            Capture(player, "procedural", x => states.Procedural = Cast<ProceduralEventState>(x));
            Capture(player, "proceduralMonsterState", x => states.ProceduralMonsterState = Cast<ProceduralMonsterState>(x));
            Capture(player, "proceduralTechniqueState", x => states.ProceduralTechniqueState = Cast<ProceduralTechniqueState>(x));

            foreach (var kv in player.Systems)
                if (!KnownKeys.Contains(kv.Key))
                    states.Other[kv.Key] = kv.Value == null ? JValue.CreateNull() : JToken.FromObject(kv.Value, SaveSystem.JsonSerializer);
            return states;
        }

        public void ApplyTo(Player player)
        {
            player.Systems.Clear();
            Put(player, "map", Map);
            Put(player, "events", Events);
            Put(player, "quest", Quest);
            Put(player, "dialogue", Dialogue);
            Put(player, "npc", Npc);
            Put(player, "sect", Sect);
            Put(player, "secretRealm", SecretRealm);
            Put(player, "bounty", Bounty);
            Put(player, "divineArts", DivineArts);
            Put(player, "auction", Auction);
            Put(player, "mining", Mining);
            Put(player, "learning", Learning);
            Put(player, "proceduralItemState", ProceduralItemState);
            Put(player, "death", Death);
            Put(player, "destiny", Destiny);
            Put(player, "enlightenment", Enlightenment);
            Put(player, "karma", Karma);
            Put(player, "heartDemon", HeartDemon);
            Put(player, "ranking", Ranking);
            Put(player, "pvp", Pvp);
            Put(player, "reincarnation", Reincarnation);
            Put(player, "ascension", Ascension);
            Put(player, "primordialEndgame", PrimordialEndgame);
            Put(player, "achievement", Achievement);
            Put(player, "chronicle", Chronicle);
            Put(player, "procedural", Procedural);
            Put(player, "proceduralMonsterState", ProceduralMonsterState);
            Put(player, "proceduralTechniqueState", ProceduralTechniqueState);
            foreach (var kv in Other) player.Systems[kv.Key] = kv.Value;
        }

        private static readonly HashSet<string> KnownKeys = new()
        {
            "map", "events", "quest", "dialogue", "npc", "sect", "secretRealm", "bounty",
            "divineArts", "auction", "mining", "learning", "proceduralItemState", "death",
            "destiny", "enlightenment", "karma", "heartDemon", "ranking", "pvp", "reincarnation",
            "ascension", "primordialEndgame", "achievement", "chronicle", "procedural",
            "proceduralMonsterState", "proceduralTechniqueState"
        };

        private static void Capture(Player player, string key, System.Action<object> set)
        {
            if (player.Systems.TryGetValue(key, out var value) && value != null) set(value);
        }

        private static T Cast<T>(object value) where T : class =>
            value as T ?? (value is JToken token ? token.ToObject<T>(SaveSystem.JsonSerializer) : JToken.FromObject(value, SaveSystem.JsonSerializer).ToObject<T>(SaveSystem.JsonSerializer));

        private static void Put(Player player, string key, object value)
        {
            if (value != null) player.Systems[key] = value;
        }
    }

    public sealed class SaveSlotPreview
    {
        public int SlotIndex;
        public bool IsEmpty;
        public string Name;
        public int? RealmIndex;
        public int? Age;
        public int? GameYear;
        public int? GameMonth;
        public long? SavedAt;
    }
}
