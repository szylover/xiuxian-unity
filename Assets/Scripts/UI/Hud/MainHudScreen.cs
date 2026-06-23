// ============================================================
// MainHudScreen.cs — in-game HUD shell and panel navigation
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public sealed class MainHudScreen : UiScreen
    {
        private readonly Dictionary<PanelId, IPanel> panels = new();
        private Transform panelHost;
        private TMP_Text logText;
        private PanelId activePanel = PanelId.Cultivation;

        protected override void Build()
        {
            RegisterPlaceholders();
            var root = UIBuilder.Panel(transform, "HudRoot", new Color(0.035f, 0.028f, 0.022f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            UIBuilder.Vertical(root, 8, 8);

            var status = UIBuilder.Panel(root.transform, "StatusBar", new Color(0.11f, 0.075f, 0.045f, 0.98f));
            UIBuilder.Layout(status, preferredHeight: 78);
            BuildStatus(status.transform);

            var body = UIBuilder.Rect("Body", root.transform);
            UIBuilder.Layout(body, flexibleHeight: 1);
            UIBuilder.Horizontal(body, 0, 8);

            var nav = UIBuilder.Panel(body.transform, "Navigation", new Color(0.08f, 0.06f, 0.04f, 0.94f));
            UIBuilder.Layout(nav, preferredWidth: 270, flexibleHeight: 1);
            BuildNavigation(nav.transform);

            var center = UIBuilder.Panel(body.transform, "PanelHost", new Color(0.07f, 0.055f, 0.04f, 0.92f));
            UIBuilder.Layout(center, flexibleWidth: 1, flexibleHeight: 1);
            panelHost = center.transform;

            var log = UIBuilder.Panel(body.transform, "GameLog", new Color(0.05f, 0.04f, 0.035f, 0.94f));
            UIBuilder.Layout(log, preferredWidth: 390, flexibleHeight: 1);
            BuildLog(log.transform);

            ShowPanel(activePanel);
        }

        private void BuildStatus(Transform parent)
        {
            UIBuilder.Horizontal(parent.gameObject, 12, 18).childAlignment = TextAnchor.MiddleLeft;
            var p = Context.CurrentPlayer;
            var realm = Context.Database.Realms.TryGetValue(p.RealmIndex, out var r) ? r.Name : UiTexts.RealmUnknown;
            AddStatus(parent, $"{p.Name}【{realm}】");
            AddStatus(parent, $"{UiTexts.Hp} {p.Hp}/{p.MaxHp}");
            AddStatus(parent, $"{UiTexts.Mp} {p.Mp}/{p.MaxMp}");
            AddStatus(parent, $"{UiTexts.Stamina} {p.Stamina}/{p.MaxStamina}");
            AddStatus(parent, $"{UiTexts.Gold} {p.Gold}");
            AddStatus(parent, $"{UiTexts.Age} {UiTexts.AgeYears(p.Age)}");
            UIBuilder.Layout(UIBuilder.Button(parent, UiTexts.MainMenu, () => { Context.SaveCurrent(); Context.ExitToStart(); Navigator.Show<StartScreen>(); }).gameObject, preferredWidth: 140, preferredHeight: 52);
        }

        private void AddStatus(Transform parent, string text)
        {
            var label = UIBuilder.Label(parent, text, 22, TextAlignmentOptions.Left);
            UIBuilder.Layout(label.gameObject, preferredWidth: 190, preferredHeight: 52);
        }

        private void BuildNavigation(Transform parent)
        {
            UIBuilder.Vertical(parent.gameObject, 14, 8);
            foreach (var entry in panels)
            {
                var id = entry.Key;
                var button = UIBuilder.Button(parent, entry.Value.Title, () => ShowPanel(id));
                UIBuilder.Layout(button.gameObject, preferredHeight: 48);
            }
        }

        private void BuildLog(Transform parent)
        {
            UIBuilder.Vertical(parent.gameObject, 12, 10);
            UIBuilder.Layout(UIBuilder.Label(parent, UiTexts.GameLog, 28).gameObject, preferredHeight: 46);
            var viewport = UIBuilder.ScrollList(parent, "LogScroll");
            UIBuilder.Layout(viewport.transform.parent.gameObject, flexibleHeight: 1);
            logText = UIBuilder.Label(viewport.transform, string.Empty, 20, TextAlignmentOptions.TopLeft);
            UIBuilder.Layout(logText.gameObject, flexibleHeight: 1);
            RefreshLog();
        }

        private void RefreshLog()
        {
            logText.text = Context.LogEntries.Count == 0 ? UiTexts.NoLog : string.Join("\n", Context.LogEntries);
        }

        private void RegisterPlaceholders()
        {
            panels.Clear();
            AddPanel(PanelId.Cultivation, UiTexts.Cultivation);
            AddPanel(PanelId.Inventory, UiTexts.Inventory);
            AddPanel(PanelId.Map, UiTexts.Map);
            AddPanel(PanelId.Combat, UiTexts.Combat);
            AddPanel(PanelId.Sect, UiTexts.Sect);
            AddPanel(PanelId.Quests, UiTexts.Quests);
            AddPanel(PanelId.Shop, UiTexts.Shop);
            AddPanel(PanelId.Equipment, UiTexts.Equipment);
            AddPanel(PanelId.Technique, UiTexts.Technique);
            AddPanel(PanelId.Alchemy, UiTexts.Alchemy);
            AddPanel(PanelId.World, UiTexts.World);
            panels[PanelId.Save] = new SavePanel();
        }

        private void AddPanel(PanelId id, string title) => panels[id] = new PlaceholderPanel(id, title);

        private void ShowPanel(PanelId id)
        {
            activePanel = id;
            for (var i = panelHost.childCount - 1; i >= 0; i--) Destroy(panelHost.GetChild(i).gameObject);
            panels[id].Build(panelHost, Context);
            RefreshLog();
        }

        private sealed class PlaceholderPanel : IPanel
        {
            public PanelId Id { get; }
            public string Title { get; }
            public PlaceholderPanel(PanelId id, string title) { Id = id; Title = title; }
            public void Build(Transform parent, GameContext context)
            {
                UIBuilder.Vertical(parent.gameObject, 24, 16);
                UIBuilder.Layout(UIBuilder.Label(parent, Title, 42).gameObject, preferredHeight: 76);
                UIBuilder.Layout(UIBuilder.Label(parent, UiTexts.PanelPlaceholder(Title), 26, TextAlignmentOptions.Center).gameObject, preferredHeight: 140);
            }
        }

        private sealed class SavePanel : IPanel
        {
            public PanelId Id => PanelId.Save;
            public string Title => UiTexts.Save;
            public void Build(Transform parent, GameContext context)
            {
                UIBuilder.Vertical(parent.gameObject, 24, 16).childAlignment = TextAnchor.UpperCenter;
                UIBuilder.Layout(UIBuilder.Label(parent, UiTexts.Save, 42).gameObject, preferredHeight: 76);
                UIBuilder.Layout(UIBuilder.Button(parent, UiTexts.SaveNow, context.SaveCurrent).gameObject, preferredWidth: 280, preferredHeight: 64);
            }
        }
    }
}
