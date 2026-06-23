// ============================================================
// ProceduralPortraitProvider.cs — deterministic placeholder portraits
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation
{
    public sealed class ProceduralPortraitProvider : IPortraitProvider
    {
        private const int Size = 192;

        public bool TryGetPortrait(in PortraitRequest request, out PortraitDescriptor portrait)
        {
            portrait = Create(request);
            return true;
        }

        public PortraitDescriptor Create(PortraitRequest request)
        {
            var element = RealmTheme.DominantElement(request.Player);
            var palette = RealmTheme.Blend(RealmTheme.ForRealm(request.RealmIndex), RealmTheme.ForElement(element), 0.45f);
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = $"procedural_portrait_{request.Gender}_{request.Appearance}_{request.RealmIndex}",
                hideFlags = HideFlags.DontSave,
            };

            var seed = StableHash($"{request.Gender}:{request.Appearance}:{request.RealmIndex}:{element}");
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var u = x / (Size - 1f);
                    var v = y / (Size - 1f);
                    var color = Color.Lerp(palette.Shadow, palette.Secondary, v);
                    color = Color.Lerp(color, palette.Primary, 0.18f + 0.12f * Mathf.Sin((u * 8f + seed % 17) * Mathf.PI));

                    var dx = u - 0.5f;
                    var headY = request.Gender == "female" ? 0.58f : 0.56f;
                    var head = (dx * dx) / 0.055f + ((v - headY) * (v - headY)) / 0.095f < 1f;
                    var body = Mathf.Abs(dx) < Mathf.Lerp(0.30f, 0.18f, v) && v < 0.44f && v > 0.08f;
                    var hair = (dx * dx) / 0.075f + ((v - (headY + 0.08f)) * (v - (headY + 0.08f))) / 0.07f < 1f && v > headY;
                    var aura = Mathf.Abs(dx) < 0.42f && v > 0.08f && v < 0.90f && ((x + y + seed) % (10 + request.Appearance % 5) == 0);

                    if (aura) color = Color.Lerp(color, palette.Accent, 0.35f);
                    if (body) color = Color.Lerp(palette.Primary, palette.Secondary, 0.38f + 0.08f * request.Appearance);
                    if (head) color = Color.Lerp(new Color(0.92f, 0.76f, 0.58f, 1f), palette.Accent, 0.12f * (request.Appearance + 1));
                    if (hair) color = request.Gender == "female" ? Color.Lerp(palette.Shadow, palette.Primary, 0.25f) : Color.Lerp(palette.Shadow, Color.black, 0.35f);

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = texture.name;

            return new PortraitDescriptor
            {
                Sprite = sprite,
                Palette = palette,
                AssetKey = PresentationAssetPaths.PortraitBase(request.Gender, request.Appearance),
                Procedural = true,
            };
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = 23;
                for (var i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash < 0 ? -hash : hash;
            }
        }
    }
}
