// ============================================================
// PanelContracts.cs — panel registry contract for issue #10
// ============================================================

using UnityEngine;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public enum PanelId
    {
        Cultivation,
        Inventory,
        Map,
        Combat,
        Sect,
        Quests,
        Shop,
        Equipment,
        Technique,
        Alchemy,
        World,
        Save,
    }

    public interface IPanel
    {
        PanelId Id { get; }
        string Title { get; }
        void Build(Transform parent, GameContext context);
    }
}
