// ============================================================
// PanelContracts.cs — panel registry contract for issue #10
// ============================================================

using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;

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
        void OnGameEvent(in GameEvent gameEvent);
    }
}
