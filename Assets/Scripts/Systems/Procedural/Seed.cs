using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Systems;

namespace Xiuxian.Systems.Procedural
{
    public sealed class Mulberry32Rng : IRng
    {
        private int state;
        public Mulberry32Rng(int seed) { state = seed; }
        public double NextDouble()
        {
            unchecked
            {
                state += (int)0x6D2B79F5;
                int t = (state ^ (int)((uint)state >> 15)) * (1 | state);
                t = (t + ((t ^ (int)((uint)t >> 7)) * (61 | t))) ^ t;
                return ((uint)(t ^ (int)((uint)t >> 14))) / 4294967296.0;
            }
        }
        public int NextIntInclusive(int min, int max) => min + (int)Math.Floor(NextDouble() * (max - min + 1));
    }

    public static class ProceduralSeed
    {
        public static Mulberry32Rng CreateRng(int seed) => new Mulberry32Rng(seed);
        public static int HashSeed(int master, int counter)
        {
            unchecked
            {
                int h = master ^ counter;
                h = (h ^ (int)((uint)h >> 16)) * 0x45d9f3b;
                h = (h ^ (int)((uint)h >> 13)) * 0x45d9f3b;
                h = h ^ (int)((uint)h >> 16);
                return (int)(uint)h;
            }
        }
        public static T WeightedPick<T>(IEnumerable<T> items, Func<T, double> weightOf, IRng rng)
        {
            var list = items.ToList();
            double total = list.Sum(x => Math.Max(0, weightOf(x)));
            if (list.Count == 0) return default;
            if (total <= 0) return list[0];
            double roll = rng.NextDouble() * total;
            foreach (var item in list)
            {
                roll -= Math.Max(0, weightOf(item));
                if (roll <= 0) return item;
            }
            return list[list.Count - 1];
        }
    }
}
