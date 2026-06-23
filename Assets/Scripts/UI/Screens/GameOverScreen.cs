// ============================================================
// GameOverScreen.cs — simple end-state shell screen
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public sealed class GameOverScreen : UiScreen
    {
        protected override void Build()
        {
            var root = UIBuilder.Panel(transform, "GameOverRoot", new Color(0.035f, 0.028f, 0.022f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            var layout = UIBuilder.Vertical(root, 48, 18);
            layout.childAlignment = TextAnchor.MiddleCenter;
            UIBuilder.Layout(UIBuilder.Label(root.transform, UiTexts.GameOverTitle, 60).gameObject, preferredHeight: 100);
            UIBuilder.Layout(UIBuilder.Button(root.transform, UiTexts.Restart, () => { Context.ExitToStart(); Navigator.Show<StartScreen>(); }).gameObject, preferredWidth: 260, preferredHeight: 64);
        }
    }
}
