// ============================================================
// DlcSelectScreen.cs — optional DLC toggle screen
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class DlcSelectScreen : UiScreen
    {
        private int slotIndex;
        private string playerName;
        private string gender;
        private int appearance;
        private PreviewRoll preview;
        private readonly HashSet<string> selected = new() { "core" };
        private TMPro.TMP_Text countText;

        public void Configure(int slotIndex, string playerName, string gender, int appearance, PreviewRoll preview)
        {
            this.slotIndex = slotIndex;
            this.playerName = playerName;
            this.gender = gender;
            this.appearance = appearance;
            this.preview = preview;
            selected.Clear();
            var defaults = Context.SelectedPackIds.Count == 0 ? new[] { "core" } : Context.SelectedPackIds.ToArray();
            foreach (var id in defaults) selected.Add(id);
            selected.Add("core");
        }

        protected override void Build()
        {
            var root = UIBuilder.Panel(transform, "DlcRoot", new Color(0.035f, 0.028f, 0.022f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            var layout = UIBuilder.Vertical(root, 42, 16);
            layout.childAlignment = TextAnchor.MiddleCenter;
            UIBuilder.Layout(UIBuilder.Label(root.transform, UiTexts.DlcSelection, 52).gameObject, preferredHeight: 78);
            countText = UIBuilder.Label(root.transform, string.Empty, 26);
            UIBuilder.Layout(countText.gameObject, preferredHeight: 42);

            var list = UIBuilder.ScrollList(root.transform, "DlcList");
            UIBuilder.Layout(list.transform.parent.gameObject, preferredWidth: 900, preferredHeight: 560);
            foreach (var id in Context.AvailablePackIds)
            {
                var isCore = id == "core";
                var toggle = UIBuilder.Toggle(list.transform, UiTexts.PackLabel(id), selected.Contains(id), value =>
                {
                    if (value) selected.Add(id); else selected.Remove(id);
                    selected.Add("core");
                    RefreshCount();
                });
                UIBuilder.Layout(toggle.gameObject, preferredHeight: 48);
                toggle.interactable = !isCore;
            }
            RefreshCount();

            var actions = UIBuilder.Rect("Actions", root.transform);
            UIBuilder.Layout(actions, preferredWidth: 700, preferredHeight: 72);
            UIBuilder.Horizontal(actions, 0, 18).childAlignment = TextAnchor.MiddleCenter;
            UIBuilder.Layout(UIBuilder.Button(actions.transform, UiTexts.Back, () => Navigator.Show<CharacterCreationScreen>(s => s.Configure(slotIndex))).gameObject, preferredWidth: 180, preferredHeight: 58);
            UIBuilder.Layout(UIBuilder.Button(actions.transform, UiTexts.StartCultivation, StartGame).gameObject, preferredWidth: 240, preferredHeight: 58);
        }

        private void RefreshCount()
        {
            if (countText != null) countText.text = UiTexts.DlcCount(selected.Count, Context.AvailablePackIds.Count);
        }

        private void StartGame()
        {
            Context.CreateNewPlayer(slotIndex, playerName, gender, appearance, preview, selected.OrderBy(id => id == "core" ? string.Empty : id));
            Navigator.Show<MainHudScreen>();
        }
    }
}

