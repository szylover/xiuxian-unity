// ============================================================
// CharacterCreationScreen.cs — name, gender, appearance, reroll
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class CharacterCreationScreen : UiScreen
    {
        private int slotIndex;
        private string gender = "male";
        private int appearance;
        private TMP_InputField nameInput;
        private PreviewRoll preview;
        private TMPro.TMP_Text previewText;

        public void Configure(int slot)
        {
            slotIndex = slot;
        }

        protected override void Build()
        {
            preview = Context.RollPreview();
            var root = UIBuilder.Panel(transform, "CharacterRoot", new Color(0.04f, 0.032f, 0.024f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            var layout = UIBuilder.Vertical(root, 42, 16);
            layout.childAlignment = TextAnchor.MiddleCenter;

            var title = UIBuilder.Label(root.transform, UiTexts.CharacterCreation, 52);
            UIBuilder.Layout(title.gameObject, preferredHeight: 76);
            var slot = UIBuilder.Label(root.transform, UiTexts.SlotTitle(slotIndex + 1), 26);
            UIBuilder.Layout(slot.gameObject, preferredHeight: 42);

            var form = UIBuilder.Panel(root.transform, "Form", new Color(0.10f, 0.075f, 0.045f, 0.94f));
            UIBuilder.Layout(form, preferredWidth: 780, preferredHeight: 570);
            UIBuilder.Vertical(form, 24, 14);

            AddNameRow(form.transform);
            AddGenderRow(form.transform);
            AddAppearanceRow(form.transform);
            previewText = UIBuilder.Label(form.transform, string.Empty, 22, TextAlignmentOptions.Left);
            UIBuilder.Layout(previewText.gameObject, preferredHeight: 170);
            RefreshPreview();

            var reroll = UIBuilder.Button(form.transform, UiTexts.Reroll, () => { preview = Context.RollPreview(); RefreshPreview(); });
            UIBuilder.Layout(reroll.gameObject, preferredHeight: 52);

            var actions = UIBuilder.Rect("Actions", root.transform);
            UIBuilder.Layout(actions, preferredWidth: 780, preferredHeight: 72);
            UIBuilder.Horizontal(actions, 0, 18).childAlignment = TextAnchor.MiddleCenter;
            var back = UIBuilder.Button(actions.transform, UiTexts.Back, () => Navigator.Show<StartScreen>());
            UIBuilder.Layout(back.gameObject, preferredWidth: 180, preferredHeight: 58);
            var next = UIBuilder.Button(actions.transform, UiTexts.Confirm, () => Navigator.Show<DlcSelectScreen>(s => s.Configure(slotIndex, PlayerName(), gender, appearance, preview)));
            UIBuilder.Layout(next.gameObject, preferredWidth: 220, preferredHeight: 58);
        }

        private void AddNameRow(Transform parent)
        {
            var row = UIBuilder.Rect("NameRow", parent);
            UIBuilder.Layout(row, preferredHeight: 64);
            UIBuilder.Horizontal(row, 0, 12).childAlignment = TextAnchor.MiddleLeft;
            var label = UIBuilder.Label(row.transform, UiTexts.DaoName, 24, TextAlignmentOptions.Left);
            UIBuilder.Layout(label.gameObject, preferredWidth: 120, preferredHeight: 54);
            var inputRoot = UIBuilder.Panel(row.transform, "NameInput", new Color(0.06f, 0.05f, 0.04f, 1f));
            UIBuilder.Layout(inputRoot, preferredHeight: 54, flexibleWidth: 1);
            nameInput = inputRoot.AddComponent<TMP_InputField>();
            var text = UIBuilder.Label(inputRoot.transform, string.Empty, 24, TextAlignmentOptions.Left);
            UIBuilder.Stretch(text.rectTransform, 12, 8, 12, 8);
            nameInput.textComponent = text;
            var placeholder = UIBuilder.Label(inputRoot.transform, UiTexts.NamePlaceholder, 22, TextAlignmentOptions.Left);
            placeholder.color = new Color(0.55f, 0.50f, 0.42f, 1f);
            UIBuilder.Stretch(placeholder.rectTransform, 12, 8, 12, 8);
            nameInput.placeholder = placeholder;
            nameInput.characterLimit = 10;
        }

        private void AddGenderRow(Transform parent)
        {
            var row = UIBuilder.Rect("GenderRow", parent);
            UIBuilder.Layout(row, preferredHeight: 60);
            UIBuilder.Horizontal(row, 0, 12).childAlignment = TextAnchor.MiddleLeft;
            UIBuilder.Layout(UIBuilder.Label(row.transform, UiTexts.GenderLabel(gender), 24, TextAlignmentOptions.Left).gameObject, preferredWidth: 120, preferredHeight: 54);
            UIBuilder.Layout(UIBuilder.Button(row.transform, UiTexts.Male, () => gender = "male").gameObject, preferredWidth: 150, preferredHeight: 52);
            UIBuilder.Layout(UIBuilder.Button(row.transform, UiTexts.Female, () => gender = "female").gameObject, preferredWidth: 150, preferredHeight: 52);
        }

        private void AddAppearanceRow(Transform parent)
        {
            var row = UIBuilder.Rect("AppearanceRow", parent);
            UIBuilder.Layout(row, preferredHeight: 60);
            UIBuilder.Horizontal(row, 0, 12).childAlignment = TextAnchor.MiddleLeft;
            UIBuilder.Layout(UIBuilder.Label(row.transform, UiTexts.Appearance, 24, TextAlignmentOptions.Left).gameObject, preferredWidth: 120, preferredHeight: 54);
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                UIBuilder.Layout(UIBuilder.Button(row.transform, (i + 1).ToString(), () => appearance = index).gameObject, preferredWidth: 80, preferredHeight: 52);
            }
        }

        private void RefreshPreview()
        {
            previewText.text = $"{UiTexts.SpiritRoot}：{UiTexts.RootSummary(preview.SpiritRoots)}\n{UiTexts.Luck}：{preview.Luck}  {UiTexts.Comprehension}：{preview.Comprehension}  {UiTexts.Charisma}：{preview.Charisma}\n{UiTexts.Mood}：{preview.Mood}  {UiTexts.Health}：{preview.Health}\n{UiTexts.Aptitude}：火{preview.Aptitudes.Fire} 水{preview.Aptitudes.Water} 木{preview.Aptitudes.Wood} 金{preview.Aptitudes.Metal} 土{preview.Aptitudes.Earth}";
        }

        private string PlayerName()
        {
            var value = nameInput == null ? string.Empty : nameInput.text.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
