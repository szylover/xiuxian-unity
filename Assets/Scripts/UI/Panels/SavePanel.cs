// ============================================================
// SavePanel.cs — manual save panel
// ============================================================

using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.UI
{
    public sealed class SavePanel : PanelBase
    {
        public SavePanel() : base(PanelId.Save, UiTexts.Save) { }

        protected override void BuildContent(Transform parent)
        {
            UIBuilder.Vertical(parent.gameObject, 24, 16).childAlignment = TextAnchor.UpperCenter;
            UIBuilder.Layout(UIBuilder.Label(parent, UiTexts.Save, 42).gameObject, preferredHeight: 76);
            UIBuilder.Layout(UIBuilder.Button(parent, UiTexts.SaveNow, Context.SaveCurrent).gameObject, preferredWidth: 280, preferredHeight: 64);
        }

        protected override bool ShouldRefreshOn(GameEventType type) => false;
    }
}
