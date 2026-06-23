// ============================================================
// ResourceSceneProvider.cs — Resources-backed scene provider
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation
{
    public sealed class ResourceSceneProvider : ISceneProvider
    {
        private readonly ProceduralSceneProvider fallback;

        public ResourceSceneProvider(ProceduralSceneProvider fallback = null)
        {
            this.fallback = fallback ?? new ProceduralSceneProvider();
        }

        public bool TryGetScene(in SceneRequest request, out SceneDescriptor scene)
        {
            var key = PresentationAssetPaths.SceneBackground(request.RegionId);
            var sprite = Resources.Load<Sprite>(key);
            if (sprite == null)
            {
                scene = fallback.Create(request);
                return true;
            }

            var palette = RealmTheme.Blend(
                RealmTheme.ForElement(RealmTheme.GuessRegionElement(request.Region)),
                RealmTheme.ForRealm(request.RealmIndex),
                0.25f);
            scene = ProceduralSceneProvider.CreateDescriptor(request, sprite, palette, key, false);
            return true;
        }
    }
}
