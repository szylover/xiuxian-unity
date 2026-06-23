// ============================================================
// StartScreen.cs — title, new game, save slots
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class StartScreen : UiScreen
    {
        protected override void Build()
        {
            var root = UIBuilder.Panel(transform, "StartRoot", new Color(0.035f, 0.028f, 0.022f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            var layout = UIBuilder.Vertical(root, 48, 18);
            layout.childAlignment = TextAnchor.MiddleCenter;

            var title = UIBuilder.Label(root.transform, UiTexts.GameTitle, 64);
            UIBuilder.Layout(title.gameObject, preferredHeight: 90);
            var subtitle = UIBuilder.Label(root.transform, UiTexts.Subtitle, 30);
            UIBuilder.Layout(subtitle.gameObject, preferredHeight: 54);

            var slotsPanel = UIBuilder.Panel(root.transform, "SaveSlots", new Color(0.10f, 0.075f, 0.045f, 0.92f));
            UIBuilder.Layout(slotsPanel, preferredWidth: 860, preferredHeight: 560);
            UIBuilder.Vertical(slotsPanel, 22, 12);

            foreach (var preview in Context.SaveSystem.ListSlots())
                AddSlotRow(slotsPanel.transform, preview);

            var quit = UIBuilder.Button(root.transform, UiTexts.Quit, Application.Quit);
            UIBuilder.Layout(quit.gameObject, preferredWidth: 280, preferredHeight: 58);
        }

        private void AddSlotRow(Transform parent, SaveSlotPreview preview)
        {
            var row = UIBuilder.Panel(parent, "SlotRow", new Color(0.16f, 0.12f, 0.075f, 0.95f));
            UIBuilder.Layout(row, preferredHeight: 86);
            UIBuilder.Horizontal(row, 12, 12).childAlignment = TextAnchor.MiddleLeft;

            var text = UIBuilder.Label(row.transform, $"{UiTexts.SlotTitle(preview.SlotIndex + 1)}\n{UiTexts.SlotPreview(preview)}", 22, TMPro.TextAlignmentOptions.Left);
            UIBuilder.Layout(text.gameObject, preferredWidth: 510, preferredHeight: 70, flexibleWidth: 1);
            var newGame = UIBuilder.Button(row.transform, UiTexts.NewGame, () => Navigator.Show<CharacterCreationScreen>(s => s.Configure(preview.SlotIndex)));
            UIBuilder.Layout(newGame.gameObject, preferredWidth: 145, preferredHeight: 58);
            var load = UIBuilder.Button(row.transform, UiTexts.ContinueGame, () =>
            {
                if (Context.LoadSlot(preview.SlotIndex)) Navigator.Show<MainHudScreen>();
            });
            UIBuilder.Layout(load.gameObject, preferredWidth: 145, preferredHeight: 58);
            load.interactable = !preview.IsEmpty;
        }
    }
}
