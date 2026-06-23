// ============================================================
// PlayerModels.cs — player domain model ported from game/player/types.ts
// UnityEngine-free
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Systems
{
    public sealed class Aptitudes
    {
        public int Alchemy, Smithing, Fengshui, Mining;
        public int Blade, Spear, Sword, Fist, Palm, Finger;
        public int Fire, Water, Thunder, Wind, Earth, Wood, Metal;
    }

    public sealed class InventorySlot { public string ItemId; public int Count; }
    public sealed class EquippedSlots
    {
        public string Weapon, Helmet, Armor, Boots, Accessory1, Accessory2;
    }

    public sealed class PlayerTracking
    {
        public int KillCount, BossKillCount, ConsecutiveRests, ConsecutiveCultivates, LowMoodStreak, ConsecutiveBreakthroughFails;
        public bool HasBeenBelow10Hp, DefeatedHigherRealm;
    }

    public sealed class TechniqueSlot
    {
        public string TechniqueId;
        public int Level;
        public int Exp;
        public string InstanceId;
    }

    public sealed class SpiritRoot
    {
        public string Type;
        public int Affinity;
    }

    public sealed class PlayerSpiritRoots
    {
        public readonly List<SpiritRoot> Roots = new();
        public string Combo;
        public double CultivationMultiplier;
    }

    public sealed class SpiritRootGrade
    {
        public string Grade;
        public double Multiplier;
        public string Color;
    }

    public sealed class BreakthroughState
    {
        public readonly Dictionary<int, int> FailedAttempts = new();
        public readonly List<string> TribulationsPassed = new();
        public bool IsLooseImmortal;
    }

    public sealed class BottleneckEntry
    {
        public string BottleneckId;
        public int ActivatedAt;
        public int PersistenceCultivationCount;
    }

    public sealed class BottleneckUnlockedEntry
    {
        public string BottleneckId;
        public int UnlockedAt;
        public string Method;
    }

    public sealed class BottleneckState
    {
        public readonly Dictionary<string, BottleneckEntry> Active = new();
        public readonly Dictionary<string, BottleneckUnlockedEntry> Unlocked = new();
    }

    public sealed class Player
    {
        public string Name, Avatar, Gender;
        public int RealmIndex, Exp, Age, Lifespan, Mood, Health, Stamina, MaxStamina;
        public int Hp, MaxHp, Mp, MaxMp, MentalPower, MaxMentalPower, Atk, Def, Speed;
        public double SkillResist, SpellResist, CritRate, CritDmgMultiplier, CritResist, MoveSpeed;
        public int Luck, Comprehension, Charisma, Karma, Gold, InventoryCapacity, Appearance;
        public Aptitudes Aptitudes = new();
        public readonly List<InventorySlot> Inventory = new();
        public EquippedSlots Equipped = new();
        public readonly List<TechniqueSlot> Techniques = new();
        public string ActiveTechniqueId, DestinyId;
        public readonly List<string> TalentIds = new();
        public readonly Dictionary<string, object> Items = new();
        public readonly Dictionary<string, object> Passives = new();
        public readonly Dictionary<string, object> Systems = new();
        public PlayerTracking Tracking = new();
        public int GameYear, GameMonth;
        public PlayerSpiritRoots SpiritRoots = new();
        public int Physique, MaxPhysique, BodyRealmIndex, BodyRealmExp, BodyTempering;
        public double PhysiqueDmgReduce;
        public readonly List<string> EnabledDLCs = new();
        public BreakthroughState Breakthrough = new();
        public BottleneckState Bottleneck = new();
    }
}
