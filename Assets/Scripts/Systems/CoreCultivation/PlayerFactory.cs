// ============================================================
// PlayerFactory.cs — create.ts + spirit-root.ts port
// UnityEngine-free
// ============================================================

using System;
using System.Linq;
using Xiuxian.Data;

namespace Xiuxian.Systems
{
    public sealed class PreviewRoll
    {
        public PlayerSpiritRoots SpiritRoots;
        public int Luck, Comprehension, Charisma, Mood, Health;
        public Aptitudes Aptitudes;
    }

    public sealed class CreatePlayerOptions
    {
        public string Name;
        public string Gender;
        public int Appearance;
        public PreviewRoll Preview;
        public string[] EnabledDLCs;
        public PlayerSpiritRoots SpiritRoots;
    }

    public static class PlayerFactory
    {
        private static readonly (string Combo, int Weight, int Count, double Multiplier)[] ComboWeights = new[]
        {
            ("single", 1, 1, 3.0), ("dual", 5, 2, 2.0), ("triple", 15, 3, 1.2),
            ("quad", 35, 4, 0.8), ("penta", 40, 5, 0.5), ("none", 4, 0, 0.1),
        };

        private static readonly string[] RootTypes = { "metal", "wood", "water", "fire", "earth" };

        public static PreviewRoll RollPreview(IRng rng)
        {
            var roots = RollSpiritRoots(rng);
            return new PreviewRoll
            {
                SpiritRoots = roots,
                Luck = RollInnateAttr(rng),
                Comprehension = RollInnateAttr(rng),
                Charisma = RollInnateAttr(rng),
                Aptitudes = RollAptitudesWithSpiritRoots(roots, rng),
                Mood = rng.NextIntInclusive(50, 90),
                Health = rng.NextIntInclusive(80, 100),
            };
        }

        public static PlayerSpiritRoots RollSpiritRoots(IRng rng)
        {
            int total = ComboWeights.Sum(x => x.Weight);
            double roll = rng.NextDouble() * total;
            var selected = ComboWeights[ComboWeights.Length - 1];
            foreach (var entry in ComboWeights)
            {
                roll -= entry.Weight;
                if (roll <= 0) { selected = entry; break; }
            }

            var result = new PlayerSpiritRoots { Combo = selected.Combo, CultivationMultiplier = selected.Multiplier };
            if (selected.Count == 0) return result;

            string[] types = SampleN(RootTypes, selected.Count, rng);
            foreach (string type in types)
            {
                result.Roots.Add(new SpiritRoot
                {
                    Type = type,
                    Affinity = selected.Count == 1 ? rng.NextIntInclusive(80, 100) : rng.NextIntInclusive(1, 100),
                });
            }
            return result;
        }

        public static int RollInnateAttr(IRng rng)
        {
            double roll = rng.NextDouble() * 100;
            if (roll < 50) return rng.NextIntInclusive(1, 30);
            if (roll < 80) return rng.NextIntInclusive(31, 60);
            if (roll < 95) return rng.NextIntInclusive(61, 85);
            return rng.NextIntInclusive(86, 100);
        }

        public static Aptitudes RollAptitudesWithSpiritRoots(PlayerSpiritRoots spiritRoots, IRng rng)
        {
            var a = new Aptitudes
            {
                Alchemy = RollAptitude(rng), Smithing = RollAptitude(rng), Fengshui = RollAptitude(rng), Mining = RollAptitude(rng),
                Blade = RollAptitude(rng), Spear = RollAptitude(rng), Sword = RollAptitude(rng), Fist = RollAptitude(rng), Palm = RollAptitude(rng), Finger = RollAptitude(rng),
                Fire = RollAptitude(rng), Water = RollAptitude(rng), Thunder = RollAptitude(rng), Wind = RollAptitude(rng), Earth = RollAptitude(rng), Wood = RollAptitude(rng), Metal = RollAptitude(rng),
            };

            foreach (var root in spiritRoots.Roots)
            {
                int bonus = rng.NextIntInclusive(20, 40);
                switch (root.Type)
                {
                    case "fire": a.Fire = Math.Min(100, a.Fire + bonus); break;
                    case "water": a.Water = Math.Min(100, a.Water + bonus); break;
                    case "earth": a.Earth = Math.Min(100, a.Earth + bonus); break;
                    case "wood": a.Wood = Math.Min(100, a.Wood + bonus); break;
                    case "metal":
                        int second = rng.NextIntInclusive(20, 40);
                        a.Thunder = Math.Min(100, a.Thunder + bonus);
                        a.Wind = Math.Min(100, a.Wind + second);
                        break;
                }
            }

            if (spiritRoots.Combo == "single" && spiritRoots.Roots.Count == 1)
            {
                string type = spiritRoots.Roots[0].Type;
                if (type == "fire" && a.Fire < 80) a.Fire = rng.NextIntInclusive(80, 100);
                if (type == "water" && a.Water < 80) a.Water = rng.NextIntInclusive(80, 100);
                if (type == "earth" && a.Earth < 80) a.Earth = rng.NextIntInclusive(80, 100);
                if (type == "wood" && a.Wood < 80) a.Wood = rng.NextIntInclusive(80, 100);
                if (type == "metal")
                {
                    if (a.Thunder < 80) a.Thunder = rng.NextIntInclusive(80, 100);
                    if (a.Wind < 80) a.Wind = rng.NextIntInclusive(80, 100);
                }
            }
            return a;
        }

        public static Player CreatePlayer(GameDatabase db, CreatePlayerOptions options, IRng rng)
        {
            var realm = db.Realms.ContainsKey(0) ? db.Realms[0] : db.Realms.Values.OrderBy(x => x.Index).First();
            var spiritRoots = options.Preview?.SpiritRoots ?? options.SpiritRoots ?? RollSpiritRoots(rng);
            var aptitudes = options.Preview?.Aptitudes ?? RollAptitudesWithSpiritRoots(spiritRoots, rng);

            var p = new Player
            {
                Name = string.IsNullOrEmpty(options.Name) ? SystemTexts.DefaultPlayerName : options.Name,
                Gender = options.Gender,
                Appearance = options.Appearance,
                Avatar = $"{options.Gender}-{options.Appearance}",
                RealmIndex = 0, Exp = 0, Age = 16 * 12, Lifespan = 100 * 12,
                Mood = options.Preview?.Mood ?? rng.NextIntInclusive(50, 90),
                Health = options.Preview?.Health ?? rng.NextIntInclusive(80, 100),
                Stamina = 100, MaxStamina = 100,
                Hp = realm.HpBase ?? 0, MaxHp = realm.HpBase ?? 0,
                Mp = realm.MpBase ?? 0, MaxMp = realm.MpBase ?? 0,
                MentalPower = realm.MentalBase ?? 0, MaxMentalPower = realm.MentalBase ?? 0,
                Atk = realm.AtkBase ?? 0, Def = realm.DefBase ?? 0, Speed = realm.SpeedBase ?? 0,
                SkillResist = 0, SpellResist = 0, CritRate = 5, CritDmgMultiplier = 1.5, CritResist = 0, MoveSpeed = 10,
                Luck = options.Preview?.Luck ?? RollInnateAttr(rng),
                Comprehension = options.Preview?.Comprehension ?? RollInnateAttr(rng),
                Charisma = options.Preview?.Charisma ?? RollInnateAttr(rng),
                Karma = 0, Aptitudes = aptitudes, SpiritRoots = spiritRoots,
                Gold = 0, InventoryCapacity = 20, Equipped = new EquippedSlots(), ActiveTechniqueId = null, DestinyId = null,
                Tracking = new PlayerTracking(), GameYear = 1, GameMonth = 1,
                Physique = 0, MaxPhysique = db.BodyRealms.TryGetValue(0, out var body0) ? (body0.MaxPhysique ?? 50) : 50,
                BodyRealmIndex = 0, BodyRealmExp = 0, PhysiqueDmgReduce = 0, BodyTempering = 0,
            };
            foreach (string id in options.EnabledDLCs ?? new[] { "core" }) p.EnabledDLCs.Add(id);
            p.Systems["learning"] = new { activeStudy = (object)null, learnedRecipes = new string[0], learnedSmithingRecipes = new string[0], migrationVersion = 1 };
            return p;
        }

        private static int RollAptitude(IRng rng)
        {
            double roll = rng.NextDouble() * 100;
            if (roll < 40) return rng.NextIntInclusive(1, 20);
            if (roll < 70) return rng.NextIntInclusive(21, 50);
            if (roll < 90) return rng.NextIntInclusive(51, 80);
            if (roll < 98) return rng.NextIntInclusive(81, 95);
            return rng.NextIntInclusive(96, 100);
        }

        private static string[] SampleN(string[] arr, int n, IRng rng)
        {
            var copy = arr.ToArray();
            for (int i = 0; i < n; i++)
            {
                int j = i + (int)Math.Floor(rng.NextDouble() * (copy.Length - i));
                (copy[i], copy[j]) = (copy[j], copy[i]);
            }
            return copy.Take(n).ToArray();
        }
    }
}
