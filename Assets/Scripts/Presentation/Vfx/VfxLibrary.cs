// ============================================================
// VfxLibrary.cs — named, theme-colored particle recipes
// ============================================================

using UnityEngine;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Vfx
{
    public static class VfxLibrary
    {
        public static VfxEffectDescriptor BreakthroughSuccess(ThemePalette palette)
            => Burst("BreakthroughSuccess", palette.Accent, palette.Primary, PresentationTunings.VfxBreakthroughSuccessDuration, PresentationTunings.VfxBreakthroughSuccessCount, PresentationTunings.VfxPortraitRadius, PresentationTunings.VfxBreakthroughSuccessSpeedMin, PresentationTunings.VfxBreakthroughSuccessSpeedMax, PresentationTunings.VfxBreakthroughSuccessSizeMin, PresentationTunings.VfxBreakthroughSuccessSizeMax);

        public static VfxEffectDescriptor RealmAdvance(ThemePalette palette)
            => new()
            {
                Name = "RealmAdvance",
                Duration = PresentationTunings.VfxRealmAdvanceDuration,
                LifetimeMin = 0.55f,
                LifetimeMax = 1.15f,
                BurstCount = PresentationTunings.VfxRealmAdvanceCount,
                StartSpeedMin = PresentationTunings.VfxRealmAdvanceSpeedMin,
                StartSpeedMax = PresentationTunings.VfxRealmAdvanceSpeedMax,
                StartSizeMin = PresentationTunings.VfxRealmAdvanceSizeMin,
                StartSizeMax = PresentationTunings.VfxRealmAdvanceSizeMax,
                Radius = PresentationTunings.VfxColumnRadius,
                ConeAngle = PresentationTunings.VfxRealmColumnConeAngle,
                Shape = VfxShape.Cone,
                StartColor = WithAlpha(palette.Accent, 0.95f),
                EndColor = WithAlpha(palette.Primary, 0.2f),
                LocalOffset = new Vector2(0f, PresentationTunings.VfxRealmColumnOffsetY),
            };

        public static VfxEffectDescriptor BreakthroughFailure(ThemePalette palette)
            => Burst("BreakthroughFailure", palette.Secondary, palette.Shadow, PresentationTunings.VfxBreakthroughFailureDuration, PresentationTunings.VfxBreakthroughFailureCount, PresentationTunings.VfxFailureRadius, PresentationTunings.VfxFailureSpeedMin, PresentationTunings.VfxFailureSpeedMax, PresentationTunings.VfxFailureSizeMin, PresentationTunings.VfxFailureSizeMax);

        public static VfxEffectDescriptor CombatHit(ThemePalette palette)
            => Burst("CombatHit", new Color(1f, 0.24f, 0.12f, 0.95f), palette.Accent, PresentationTunings.VfxCombatHitDuration, PresentationTunings.VfxCombatHitCount, PresentationTunings.VfxCombatHitRadius, PresentationTunings.VfxCombatHitSpeedMin, PresentationTunings.VfxCombatHitSpeedMax, PresentationTunings.VfxCombatHitSizeMin, PresentationTunings.VfxCombatHitSizeMax);

        public static VfxEffectDescriptor Alchemy(ThemePalette palette)
            => Burst("Alchemy", RealmTheme.ForElement("wood").Accent, palette.Accent, PresentationTunings.VfxCraftDuration, PresentationTunings.VfxCraftBurstCount, PresentationTunings.VfxCraftRadius, PresentationTunings.VfxCraftSpeedMin, PresentationTunings.VfxCraftSpeedMax, PresentationTunings.VfxCraftSizeMin, PresentationTunings.VfxCraftSizeMax);

        public static VfxEffectDescriptor Smithing(ThemePalette palette)
            => Burst("Smithing", RealmTheme.ForElement("fire").Accent, RealmTheme.ForElement("metal").Primary, PresentationTunings.VfxCraftDuration, PresentationTunings.VfxCraftBurstCount, PresentationTunings.VfxCraftRadius, PresentationTunings.VfxCraftSpeedMin, PresentationTunings.VfxCraftSpeedMax, PresentationTunings.VfxCraftSizeMin, PresentationTunings.VfxCraftSizeMax);

        public static VfxEffectDescriptor Ascension()
        {
            var gold = new Color(1f, 0.84f, 0.18f, 1f);
            return Burst("Ascension", gold, new Color(1f, 1f, 0.72f, 0.35f), PresentationTunings.VfxAscensionDuration, PresentationTunings.VfxAscensionCount, PresentationTunings.VfxSceneRadius, PresentationTunings.VfxAscensionSpeedMin, PresentationTunings.VfxAscensionSpeedMax, PresentationTunings.VfxAscensionSizeMin, PresentationTunings.VfxAscensionSizeMax);
        }

        public static VfxEffectDescriptor Ambient(ThemePalette palette)
            => new()
            {
                Name = "Ambient",
                Duration = PresentationTunings.VfxAmbientLifetime,
                LifetimeMin = 3.2f,
                LifetimeMax = PresentationTunings.VfxAmbientLifetime,
                EmissionRate = PresentationTunings.VfxAmbientEmissionRate,
                StartSpeedMin = PresentationTunings.VfxAmbientSpeedMin,
                StartSpeedMax = PresentationTunings.VfxAmbientSpeedMax,
                StartSizeMin = PresentationTunings.VfxAmbientSizeMin,
                StartSizeMax = PresentationTunings.VfxAmbientSizeMax,
                Radius = PresentationTunings.VfxSceneRadius,
                Shape = VfxShape.Circle,
                Looping = true,
                StartColor = WithAlpha(palette.Accent, 0.32f),
                EndColor = WithAlpha(palette.Primary, 0.05f),
            };

        private static VfxEffectDescriptor Burst(string name, Color start, Color end, float duration, int count, float radius, float speedMin, float speedMax, float sizeMin, float sizeMax)
            => new()
            {
                Name = name,
                Duration = duration,
                LifetimeMin = duration * 0.55f,
                LifetimeMax = duration * 1.1f,
                BurstCount = count,
                StartSpeedMin = speedMin,
                StartSpeedMax = speedMax,
                StartSizeMin = sizeMin,
                StartSizeMax = sizeMax,
                Radius = radius,
                Shape = VfxShape.Circle,
                StartColor = WithAlpha(start, PresentationTunings.VfxPrimaryAlpha),
                EndColor = WithAlpha(end, PresentationTunings.VfxEndAlpha),
            };

        private static Color WithAlpha(Color color, float alpha)
            => new(color.r, color.g, color.b, alpha);
    }
}
