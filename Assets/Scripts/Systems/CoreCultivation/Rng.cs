// ============================================================
// Rng.cs — deterministic RNG boundary for pure systems
// UnityEngine-free
// ============================================================

using System;

namespace Xiuxian.Systems
{
    public interface IRng
    {
        double NextDouble();
        int NextIntInclusive(int min, int max);
    }

    public sealed class SystemRandomRng : IRng
    {
        private readonly Random random;
        public SystemRandomRng(int? seed = null) => random = seed.HasValue ? new Random(seed.Value) : new Random();
        public double NextDouble() => random.NextDouble();
        public int NextIntInclusive(int min, int max) => random.Next(min, max + 1);
    }

    public sealed class FixedRng : IRng
    {
        private readonly double doubleValue;
        public FixedRng(double doubleValue) => this.doubleValue = doubleValue;
        public double NextDouble() => doubleValue;
        public int NextIntInclusive(int min, int max) => min + (int)Math.Floor(doubleValue * (max - min + 1));
    }
}
