// ============================================================
// SceneSystem.cs — scene descriptor resolution entry point
// ============================================================

using Xiuxian.App;
using Xiuxian.Systems;

namespace Xiuxian.Presentation
{
    public interface ISceneProvider
    {
        bool TryGetScene(in SceneRequest request, out SceneDescriptor scene);
    }

    public sealed class SceneSystem
    {
        private readonly ISceneProvider provider;

        public SceneSystem(ISceneProvider provider = null)
        {
            this.provider = provider ?? new ResourceSceneProvider(new ProceduralSceneProvider());
        }

        public SceneDescriptor Resolve(GameContext context)
        {
            var player = context?.CurrentPlayer;
            var db = context?.Database;
            var region = player == null || db == null ? null : MapSystem.GetCurrentRegion(db, player);
            var exits = player == null || db == null ? null : MapSystem.GetSceneExits(db, player);
            var npcs = player == null || db == null ? null : NpcSystem.GetNpcsInRegion(db, player);
            var request = new SceneRequest(db, player, region, exits, npcs);
            return provider.TryGetScene(in request, out var scene)
                ? scene
                : new ProceduralSceneProvider().Create(request);
        }
    }
}
