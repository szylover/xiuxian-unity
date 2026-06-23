// ============================================================
// RealmTheme.cs — realm/element palettes for presentation layers
// ============================================================

using System;
using System.Linq;
using UnityEngine;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Presentation
{
    public static class RealmTheme
    {
        public static ThemePalette ForRealm(int realmIndex)
        {
            var tier = Math.Max(0, realmIndex);
            switch (tier)
            {
                case 0:
                case 1: return new ThemePalette(Rgb(158, 158, 158), Rgb(78, 70, 62), Rgb(210, 204, 184), Rgb(30, 28, 26, 190));
                case 2: return new ThemePalette(Rgb(76, 175, 80), Rgb(35, 96, 62), Rgb(178, 235, 180), Rgb(16, 48, 30, 200));
                case 3: return new ThemePalette(Rgb(33, 150, 243), Rgb(29, 78, 130), Rgb(174, 218, 255), Rgb(12, 35, 70, 205));
                case 4: return new ThemePalette(Rgb(156, 39, 176), Rgb(92, 42, 125), Rgb(235, 178, 255), Rgb(42, 16, 66, 205));
                default: return new ThemePalette(Rgb(255, 215, 0), Rgb(157, 105, 22), Rgb(255, 242, 168), Rgb(74, 45, 5, 215));
            }
        }

        public static ThemePalette ForElement(string element)
        {
            switch (element)
            {
                case ElementType.Fire: return new ThemePalette(Rgb(230, 72, 48), Rgb(124, 42, 28), Rgb(255, 176, 104), Rgb(70, 12, 8, 205));
                case ElementType.Water: return new ThemePalette(Rgb(64, 150, 224), Rgb(24, 72, 130), Rgb(166, 224, 255), Rgb(8, 28, 72, 205));
                case ElementType.Thunder: return new ThemePalette(Rgb(171, 105, 255), Rgb(78, 42, 148), Rgb(235, 224, 255), Rgb(36, 18, 82, 205));
                case ElementType.Wind: return new ThemePalette(Rgb(83, 196, 168), Rgb(35, 112, 94), Rgb(184, 255, 230), Rgb(10, 55, 48, 205));
                case ElementType.Earth: return new ThemePalette(Rgb(175, 122, 64), Rgb(96, 62, 34), Rgb(236, 192, 124), Rgb(52, 34, 18, 205));
                case ElementType.Wood: return new ThemePalette(Rgb(74, 174, 84), Rgb(34, 94, 48), Rgb(185, 236, 150), Rgb(16, 52, 28, 205));
                case ElementType.Metal: return new ThemePalette(Rgb(210, 200, 168), Rgb(120, 116, 106), Rgb(255, 248, 216), Rgb(54, 52, 48, 205));
                default: return ForRealm(0);
            }
        }

        public static string DominantElement(Player player)
        {
            var root = player?.SpiritRoots?.Roots?.OrderByDescending(r => r.Affinity).FirstOrDefault();
            return root?.Type ?? ElementType.Wood;
        }

        public static string GuessRegionElement(RegionDef region)
        {
            var tags = region?.RegionTags;
            if (tags == null || tags.Count == 0) return ElementType.Wood;
            foreach (var element in ElementTable.Counter.Keys)
                if (tags.Any(t => string.Equals(t, element, StringComparison.OrdinalIgnoreCase))) return element;
            if (tags.Any(t => t.Contains("mountain") || t.Contains("mine"))) return ElementType.Earth;
            if (tags.Any(t => t.Contains("forest") || t.Contains("wood"))) return ElementType.Wood;
            if (tags.Any(t => t.Contains("water") || t.Contains("lake") || t.Contains("river"))) return ElementType.Water;
            if (tags.Any(t => t.Contains("fire") || t.Contains("volcano"))) return ElementType.Fire;
            return ElementType.Wind;
        }

        public static ThemePalette Blend(ThemePalette a, ThemePalette b, float t)
        {
            t = Mathf.Clamp01(t);
            return new ThemePalette(
                Color.Lerp(a.Primary, b.Primary, t),
                Color.Lerp(a.Secondary, b.Secondary, t),
                Color.Lerp(a.Accent, b.Accent, t),
                Color.Lerp(a.Shadow, b.Shadow, t));
        }

        private static Color Rgb(int r, int g, int b, int a = 255)
            => new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
