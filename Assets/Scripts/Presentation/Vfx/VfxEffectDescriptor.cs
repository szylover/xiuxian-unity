// ============================================================
// VfxEffectDescriptor.cs — code-only ParticleSystem recipes
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation.Vfx
{
    public enum VfxShape
    {
        Circle,
        Cone,
    }

    public sealed class VfxEffectDescriptor
    {
        public string Name;
        public float Duration;
        public float LifetimeMin;
        public float LifetimeMax;
        public int BurstCount;
        public float EmissionRate;
        public float StartSpeedMin;
        public float StartSpeedMax;
        public float StartSizeMin;
        public float StartSizeMax;
        public float Radius;
        public float ConeAngle;
        public bool Looping;
        public VfxShape Shape;
        public Color StartColor;
        public Color EndColor;
        public Vector2 LocalOffset;
    }
}
