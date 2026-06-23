// ============================================================
// PortraitSystem.cs — portrait resolution entry point
// ============================================================

using Xiuxian.Systems;

namespace Xiuxian.Presentation
{
    public interface IPortraitProvider
    {
        bool TryGetPortrait(in PortraitRequest request, out PortraitDescriptor portrait);
    }

    public sealed class PortraitSystem
    {
        private readonly IPortraitProvider provider;

        public PortraitSystem(IPortraitProvider provider = null)
        {
            this.provider = provider ?? new ResourcePortraitProvider(new ProceduralPortraitProvider());
        }

        public PortraitDescriptor Resolve(Player player)
        {
            var request = new PortraitRequest(player);
            return provider.TryGetPortrait(in request, out var portrait)
                ? portrait
                : new ProceduralPortraitProvider().Create(request);
        }
    }
}
