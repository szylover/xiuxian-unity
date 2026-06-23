// ============================================================
// ProceduralSceneProvider.cs — deterministic placeholder backdrops
// ============================================================

using UnityEngine;
using Xiuxian.App;

namespace Xiuxian.Presentation
{
    public sealed class ProceduralSceneProvider : ISceneProvider
    {
        private const int Width = 512;
        private const int Height = 288;

        public bool TryGetScene(in SceneRequest request, out SceneDescriptor scene)
        {
            scene = Create(request);
            return true;
        }

        public SceneDescriptor Create(SceneRequest request)
        {
            var element = RealmTheme.GuessRegionElement(request.Region);
            var palette = RealmTheme.Blend(RealmTheme.ForElement(element), RealmTheme.ForRealm(request.RealmIndex), 0.2f);
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = $"procedural_scene_{request.RegionId}",
                hideFlags = HideFlags.DontSave,
            };
            var seed = StableHash(request.RegionId);

            for (var y = 0; y < Height; y++)
            {
                var v = y / (Height - 1f);
                for (var x = 0; x < Width; x++)
                {
                    var u = x / (Width - 1f);
                    var color = Color.Lerp(palette.Shadow, palette.Secondary, v);
                    color = Color.Lerp(color, palette.Primary, 0.25f * Mathf.Sin((u * 3f + seed % 11) * Mathf.PI) + 0.25f);

                    var ridge = 0.28f + 0.12f * Mathf.Sin(u * 9f + seed * 0.013f) + 0.05f * Mathf.Sin(u * 23f);
                    if (v < ridge) color = Color.Lerp(palette.Shadow, palette.Secondary, 0.55f);
                    var mist = Mathf.Abs(v - (0.42f + 0.04f * Mathf.Sin(u * 14f))) < 0.018f;
                    if (mist) color = Color.Lerp(color, palette.Accent, 0.38f);
                    if (((x + seed) % 79) < 2 && v > 0.58f) color = Color.Lerp(color, palette.Accent, 0.45f);

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = texture.name;
            return CreateDescriptor(request, sprite, palette, PresentationAssetPaths.SceneBackground(request.RegionId), true);
        }

        public static SceneDescriptor CreateDescriptor(SceneRequest request, Sprite background, ThemePalette palette, string key, bool procedural)
        {
            return new SceneDescriptor
            {
                Background = background,
                Palette = palette,
                AssetKey = key,
                Procedural = procedural,
                Title = string.IsNullOrEmpty(request.Region?.Name) ? UiTexts.SceneUnknownRegion : request.Region.Name,
                Description = string.IsNullOrEmpty(request.Region?.Description) ? UiTexts.SceneNoDescription : request.Region.Description,
                Exits = request.Exits,
                Npcs = request.Npcs,
            };
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = 23;
                value ??= "default";
                for (var i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash < 0 ? -hash : hash;
            }
        }
    }
}
