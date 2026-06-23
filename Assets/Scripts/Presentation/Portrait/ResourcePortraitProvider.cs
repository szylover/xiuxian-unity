// ============================================================
// ResourcePortraitProvider.cs — Resources-backed portrait provider
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation
{
    public sealed class ResourcePortraitProvider : IPortraitProvider
    {
        private readonly ProceduralPortraitProvider fallback;

        public ResourcePortraitProvider(ProceduralPortraitProvider fallback = null)
        {
            this.fallback = fallback ?? new ProceduralPortraitProvider();
        }

        public bool TryGetPortrait(in PortraitRequest request, out PortraitDescriptor portrait)
        {
            var key = PresentationAssetPaths.PortraitBase(request.Gender, request.Appearance);
            var sprite = Resources.Load<Sprite>(key);
            if (sprite == null)
            {
                portrait = fallback.Create(request);
                return true;
            }

            portrait = new PortraitDescriptor
            {
                Sprite = sprite,
                RealmOverlay = Resources.Load<Sprite>(PresentationAssetPaths.PortraitRealmOverlay(request.RealmIndex)),
                Palette = RealmTheme.Blend(RealmTheme.ForRealm(request.RealmIndex), RealmTheme.ForElement(RealmTheme.DominantElement(request.Player)), 0.35f),
                AssetKey = key,
                Procedural = false,
            };
            return true;
        }
    }
}
