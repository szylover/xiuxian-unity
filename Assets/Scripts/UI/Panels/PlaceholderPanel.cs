// ============================================================
// PlaceholderPanel.cs — temporary panel until later batches land
// ============================================================

using TMPro;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.UI
{
    public sealed class PlaceholderPanel : PanelBase
    {
        public PlaceholderPanel(PanelId id, string title) : base(id, title) { }

        protected override void BuildContent(Transform parent)
        {
            UIBuilder.Vertical(parent.gameObject, 24, 16);
            UIBuilder.Layout(UIBuilder.Label(parent, Title, 42).gameObject, preferredHeight: 76);
            UIBuilder.Layout(UIBuilder.Label(parent, UiTexts.PanelPlaceholder(Title), 26, TextAlignmentOptions.Center).gameObject, preferredHeight: 140);
        }

        protected override bool ShouldRefreshOn(GameEventType type) => false;
    }
}
