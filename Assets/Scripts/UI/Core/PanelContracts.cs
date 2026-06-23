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
        Status,
        Action,
        Talent,
        Technique,
        DivineArts,
        Equipment,
        Inventory,
        Enlightenment,
        Learning,
        Alchemy,
        Smithing,
        Crafting,
        Shop,
        Auction,
        Mining,
        Map,
        Quest,
        Npc,
        Sect,
        SecretRealm,
        Bounty,
        Companion,
        Achievement,
        Chronicle,
        Ranking,
        Pvp,
        HeartDemon,
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
