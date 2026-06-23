// ============================================================
// PresentationAssetPaths.cs — presentation asset binding contract
// ============================================================

namespace Xiuxian.Presentation
{
    /// <summary>
    /// Contract for real art drop-in assets. Import sprites under Resources with these paths:
    /// - Portrait base: Assets/Resources/Portraits/{gender}_{appearance}.png, e.g. Portraits/male_0.
    /// - Optional realm overlay: Assets/Resources/Portraits/Overlays/realm_{realmIndex}.png.
    /// - Scene background: Assets/Resources/Scenes/{regionId}.png, e.g. Scenes/core_qingyun_mountain.
    /// Providers fall back to procedural placeholders when a path is missing, so content can bind by data keys first.
    /// Issue #19 can extend this contract with Addressables by registering providers that use the same logical keys.
    /// </summary>
    public static class PresentationAssetPaths
    {
        public const string PortraitRoot = "Portraits";
        public const string PortraitOverlayRoot = "Portraits/Overlays";
        public const string SceneRoot = "Scenes";

        public static string PortraitBase(string gender, int appearance)
            => $"{PortraitRoot}/{Normalize(gender)}_{appearance}";

        public static string PortraitRealmOverlay(int realmIndex)
            => $"{PortraitOverlayRoot}/realm_{realmIndex}";

        public static string SceneBackground(string regionId)
            => $"{SceneRoot}/{Normalize(regionId)}";

        public static string Normalize(string key)
            => string.IsNullOrWhiteSpace(key) ? "default" : key.Trim().ToLowerInvariant();
    }
}
