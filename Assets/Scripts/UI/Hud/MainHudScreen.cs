// ============================================================
// MainHudScreen.cs — in-game HUD shell and panel navigation
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Presentation;
using Xiuxian.Presentation.Animation;
using Xiuxian.Presentation.Audio;
using Xiuxian.Presentation.Feedback;
using Xiuxian.Presentation.Vfx;

namespace Xiuxian.UI
{
    public sealed class MainHudScreen : UiScreen
    {
        private readonly Dictionary<PanelId, IPanel> panels = new();
        private IReadOnlyList<PanelCategory> categories;
        private Transform panelHost;
        private TMP_Text logText;
        private TMP_Text nameRealmText;
        private TMP_Text hpText;
        private TMP_Text mpText;
        private TMP_Text staminaText;
        private TMP_Text goldText;
        private TMP_Text ageText;
        private PanelId activePanel = PanelId.Status;
        private PresentationController presentationController;
        private PortraitView portraitView;
        private Xiuxian.Presentation.SceneView sceneView;
        private AnimationDirector animationDirector;
        private AudioDirector audioDirector;
        private SoundControls soundControls;
        private VfxDirector vfxDirector;
        private VfxOverlay vfxOverlay;
        private FeedbackDirector feedbackDirector;
        private FeedbackOverlay feedbackOverlay;
        private RectTransform statusBarRect;
        private RectTransform logRect;

        protected override void Build()
        {
            RegisterPanels();
            presentationController = new AnimatedPresentationController(Context);
            var root = UIBuilder.Panel(transform, "HudRoot", new Color(0.035f, 0.028f, 0.022f, 1f));
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            UIBuilder.Vertical(root, 8, 8);

            var status = UIBuilder.Panel(root.transform, "StatusBar", new Color(0.11f, 0.075f, 0.045f, 0.98f));
            statusBarRect = status.GetComponent<RectTransform>();
            UIBuilder.Layout(status, preferredHeight: 78);
            BuildStatus(status.transform);

            var body = UIBuilder.Rect("Body", root.transform);
            UIBuilder.Layout(body, flexibleHeight: 1);
            UIBuilder.Horizontal(body, 0, 8);

            var nav = UIBuilder.Panel(body.transform, "Navigation", new Color(0.08f, 0.06f, 0.04f, 0.94f));
            UIBuilder.Layout(nav, preferredWidth: 270, flexibleHeight: 1);
            BuildNavigation(nav.transform);

            var center = UIBuilder.Panel(body.transform, "PanelHost", new Color(0.07f, 0.055f, 0.04f, 0.72f));
            UIBuilder.Layout(center, flexibleWidth: 1, flexibleHeight: 1);
            sceneView = new Xiuxian.Presentation.SceneView(center.transform, presentationController);
            var panelLayer = UIBuilder.Rect("PanelContent", center.transform);
            UIBuilder.Stretch(panelLayer.GetComponent<RectTransform>());
            panelHost = panelLayer.transform;

            var log = UIBuilder.Panel(body.transform, "GameLog", new Color(0.05f, 0.04f, 0.035f, 0.94f));
            logRect = log.GetComponent<RectTransform>();
            UIBuilder.Layout(log, preferredWidth: 390, flexibleHeight: 1);
            BuildLog(log.transform);
            vfxOverlay = VfxOverlay.Attach(transform);
            if (vfxOverlay != null) vfxOverlay.SetEnabled(VfxSettings.Enabled);
            feedbackOverlay = FeedbackOverlay.Attach(transform);

            var animationTargets = new AnimationTargets
            {
                Root = root.transform,
                StatusBar = statusBarRect,
                CombatFeedbackArea = logRect,
                PortraitView = portraitView,
                SceneView = sceneView,
            };
            animationDirector = new AnimationDirector(Context, presentationController, animationTargets);
            audioDirector = new AudioDirector(Context, AudioManager.Instance, animationDirector);
            vfxDirector = new VfxDirector(Context, animationDirector, vfxOverlay, animationTargets);
            feedbackDirector = new FeedbackDirector(Context, animationDirector, feedbackOverlay, animationTargets);
            ShowPanel(activePanel);
            presentationController.RefreshAll();
            Context.Bus.Subscribe(OnGameEvent);
        }

        private void BuildStatus(Transform parent)
        {
            UIBuilder.Horizontal(parent.gameObject, 12, 18).childAlignment = TextAnchor.MiddleLeft;
            nameRealmText = AddStatus(parent, string.Empty);
            hpText = AddStatus(parent, string.Empty);
            mpText = AddStatus(parent, string.Empty);
            staminaText = AddStatus(parent, string.Empty);
            goldText = AddStatus(parent, string.Empty);
            ageText = AddStatus(parent, string.Empty);
            UIBuilder.Layout(UIBuilder.Toggle(parent, UiTexts.VfxToggle, VfxSettings.Enabled, OnVfxToggleChanged).gameObject, preferredWidth: 130, preferredHeight: 52);
            UIBuilder.Layout(UIBuilder.Toggle(parent, UiTexts.FeedbackToggle, FeedbackSettings.Enabled, OnFeedbackToggleChanged).gameObject, preferredWidth: 150, preferredHeight: 52);
            soundControls = new SoundControls(parent, AudioManager.Instance);
            UIBuilder.Layout(UIBuilder.Button(parent, UiTexts.MainMenu, () => { Context.SaveCurrent(); Context.ExitToStart(); Navigator.Show<StartScreen>(); }).gameObject, preferredWidth: 140, preferredHeight: 52);
            RefreshStatus();
        }

        private void OnVfxToggleChanged(bool enabled)
        {
            if (vfxDirector != null) vfxDirector.Enabled = enabled;
            else VfxSettings.Enabled = enabled;
        }

        private void OnFeedbackToggleChanged(bool enabled)
        {
            if (feedbackDirector != null) feedbackDirector.Enabled = enabled;
            else FeedbackSettings.Enabled = enabled;
        }

        private TMP_Text AddStatus(Transform parent, string text)
        {
            var label = UIBuilder.Label(parent, text, 22, TextAlignmentOptions.Left);
            UIBuilder.Layout(label.gameObject, preferredWidth: 190, preferredHeight: 52);
            return label;
        }

        private void RefreshStatus()
        {
            var p = Context.CurrentPlayer;
            if (p == null || nameRealmText == null) return;
            var realm = Context.Database.Realms.TryGetValue(p.RealmIndex, out var r) ? r.Name : UiTexts.RealmUnknown;
            nameRealmText.text = UiTexts.HudNameRealm(p.Name, realm);
            hpText.text = UiTexts.StatCurrentMax(UiTexts.Hp, p.Hp, p.MaxHp);
            mpText.text = UiTexts.StatCurrentMax(UiTexts.Mp, p.Mp, p.MaxMp);
            staminaText.text = UiTexts.StatCurrentMax(UiTexts.Stamina, p.Stamina, p.MaxStamina);
            goldText.text = UiTexts.StatValue(UiTexts.Gold, p.Gold);
            ageText.text = UiTexts.StatValue(UiTexts.Age, UiTexts.AgeYears(p.Age));
        }

        private void BuildNavigation(Transform parent)
        {
            UIBuilder.Vertical(parent.gameObject, 8, 8);
            portraitView = new PortraitView(parent, presentationController);
            var content = UIBuilder.ScrollList(parent, "NavScroll");
            UIBuilder.Layout(content.transform.parent.gameObject, flexibleHeight: 1);
            foreach (var category in categories)
            {
                UIBuilder.SectionHeader(content.transform, category.Title);
                foreach (var panel in category.Panels)
                {
                    var id = panel.Id;
                    var button = UIBuilder.Button(content.transform, panel.Title, () => ShowPanel(id));
                    UIBuilder.Layout(button.gameObject, preferredHeight: 44);
                }
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
            if (logText == null) return;
            logText.text = Context.LogEntries.Count == 0 ? UiTexts.NoLog : string.Join("\n", Context.LogEntries);
        }

        private void RegisterPanels()
        {
            panels.Clear();
            categories = PanelRegistry.GetCategories();
            foreach (var category in categories)
                foreach (var panel in category.Panels)
                    panels[panel.Id] = panel;
        }

        private void ShowPanel(PanelId id)
        {
            if (!panels.ContainsKey(id)) id = PanelId.Status;
            activePanel = id;
            for (var i = panelHost.childCount - 1; i >= 0; i--) Destroy(panelHost.GetChild(i).gameObject);
            panels[id].Build(panelHost, Context);
            for (var i = 0; i < panelHost.childCount; i++) UiTransitionLibrary.PlayPanelSwitch(panelHost.GetChild(i));
            RefreshLog();
        }

        private void OnGameEvent(in GameEvent gameEvent)
        {
            if (IsStatusEvent(gameEvent.Type)) RefreshStatus();
            if (gameEvent.Type == GameEventType.LogAppended || gameEvent.Type == GameEventType.ExitToStart) RefreshLog();
            if (panels.TryGetValue(activePanel, out var panel)) panel.OnGameEvent(in gameEvent);
        }

        private static bool IsStatusEvent(GameEventType type)
        {
            switch (type)
            {
                case GameEventType.PlayerCreated:
                case GameEventType.PlayerLoaded:
                case GameEventType.PlayerSaved:
                case GameEventType.PlayerChanged:
                case GameEventType.PlayerStatsChanged:
                case GameEventType.RealmChanged:
                case GameEventType.CultivationChanged:
                case GameEventType.BodyCultivationChanged:
                case GameEventType.CurrencyChanged:
                case GameEventType.TimeAdvanced:
                case GameEventType.CombatChanged:
                case GameEventType.InventoryChanged:
                case GameEventType.EquipmentChanged:
                    return true;
                default:
                    return false;
            }
        }

        private void OnDestroy()
        {
            if (Context != null) Context.Bus.Unsubscribe(OnGameEvent);
            soundControls?.Dispose();
            feedbackDirector?.Dispose();
            audioDirector?.Dispose();
            vfxDirector?.Dispose();
            animationDirector?.Dispose();
            portraitView?.Dispose();
            sceneView?.Dispose();
            presentationController?.Dispose();
        }
    }
}
