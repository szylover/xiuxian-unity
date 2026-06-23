// ============================================================
// FeedbackDirector.cs — routes game/animation events to toasts, numbers, shake
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Presentation.Animation;
using Xiuxian.Systems;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class FeedbackDirector : IDisposable
    {
        private readonly GameContext context;
        private readonly AnimationDirector animationDirector;
        private readonly AnimationTargets targets;
        private readonly FloatingTextSystem floatingText;
        private readonly ToastSystem toasts;
        private readonly UiShake shake;
        private bool hasSnapshot;
        private int lastGold;
        private int lastExp;
        private readonly Dictionary<string, int> lastInventory = new();

        public FeedbackDirector(GameContext context, AnimationDirector animationDirector, FeedbackOverlay overlay, AnimationTargets targets)
        {
            this.context = context;
            this.animationDirector = animationDirector;
            this.targets = targets ?? new AnimationTargets();
            floatingText = new FloatingTextSystem(overlay);
            toasts = new ToastSystem(context, overlay);
            shake = new UiShake(this.targets.Root as RectTransform, Camera.main);
            CaptureSnapshot();
            if (animationDirector != null)
            {
                animationDirector.CombatFeedbackRequested += OnCombatFeedbackRequested;
                animationDirector.RealmAdvanced += OnRealmAdvanced;
                animationDirector.BreakthroughFailed += OnBreakthroughFailed;
            }
            if (context?.Bus != null) context.Bus.Subscribe(OnGameEvent);
        }

        public bool Enabled
        {
            get => FeedbackSettings.Enabled;
            set => FeedbackSettings.Enabled = value;
        }

        public void Dispose()
        {
            if (animationDirector != null)
            {
                animationDirector.CombatFeedbackRequested -= OnCombatFeedbackRequested;
                animationDirector.RealmAdvanced -= OnRealmAdvanced;
                animationDirector.BreakthroughFailed -= OnBreakthroughFailed;
            }
            if (context?.Bus != null) context.Bus.Unsubscribe(OnGameEvent);
            floatingText?.Dispose();
            toasts?.Dispose();
            shake?.Reset();
        }

        private void OnCombatFeedbackRequested(CombatFeedbackRequest request)
        {
            var payload = request?.Payload;
            if (payload != null)
            {
                var style = payload.IsCrit ? FeedbackTextStyle.Crit : FeedbackTextStyle.Damage;
                var text = payload.IsDodge ? UiTexts.FeedbackDodge : payload.IsCrit ? UiTexts.FeedbackCritDamage(payload.Damage) : UiTexts.FeedbackDamage(payload.Damage);
                floatingText.Show(targets.CombatFeedbackArea ?? targets.StatusBar, text, style, payload.Damage);
                foreach (var extra in payload.ExtraTexts)
                    floatingText.Show(targets.StatusBar, extra.Text, extra.Style, extra.Magnitude);
                var intensity = PresentationTunings.UiShakeBaseIntensity + Mathf.Min(18f, Mathf.Sqrt(Mathf.Max(0, payload.Damage)) * 0.8f);
                if (payload.IsCrit) intensity *= PresentationTunings.UiShakeCritMultiplier;
                shake.Shake(intensity, PresentationTunings.UiShakeDefaultDuration);
                return;
            }

            floatingText.Show(targets.CombatFeedbackArea ?? targets.StatusBar, UiTexts.FeedbackDamage(0), FeedbackTextStyle.Damage);
            shake.Shake(PresentationTunings.UiShakeBaseIntensity, PresentationTunings.UiShakeDefaultDuration);
        }

        private void OnRealmAdvanced(ThemePalette palette)
        {
            toasts.Show(UiTexts.ToastRealmAdvanced, ToastSeverity.Success);
            shake.Shake(PresentationTunings.UiShakeRealmIntensity, PresentationTunings.UiShakeDefaultDuration * 2f);
        }

        private void OnBreakthroughFailed(ThemePalette palette)
            => toasts.Show(UiTexts.ToastBreakthroughFailed, ToastSeverity.Warning);

        private void OnGameEvent(in GameEvent gameEvent)
        {
            if (context?.CurrentPlayer == null) return;
            if (!hasSnapshot)
            {
                CaptureSnapshot();
                return;
            }

            switch (gameEvent.Type)
            {
                case GameEventType.CurrencyChanged:
                    HandleCurrencyGain();
                    break;
                case GameEventType.InventoryChanged:
                    HandleInventoryGain();
                    break;
                case GameEventType.CultivationChanged:
                    HandleCultivationGain();
                    break;
                case GameEventType.LogAppended:
                    if (gameEvent.Payload is string logText) toasts.Show(logText, ToastSeverity.Info);
                    break;
                case GameEventType.AchievementChanged:
                    toasts.Show(UiTexts.ToastAchievementUnlocked, ToastSeverity.Success);
                    break;
                case GameEventType.QuestChanged:
                    toasts.Show(UiTexts.ToastQuestUpdated, ToastSeverity.Info);
                    break;
                case GameEventType.GameOver:
                    toasts.Show(UiTexts.ToastDanger, ToastSeverity.Danger);
                    break;
            }

            CaptureSnapshot();
        }

        private void HandleCurrencyGain()
        {
            var delta = context.CurrentPlayer.Gold - lastGold;
            if (delta <= 0) return;
            var text = UiTexts.FeedbackGoldGain(delta);
            floatingText.Show(targets.StatusBar, text, FeedbackTextStyle.Gain, delta);
            toasts.Show(text, ToastSeverity.Success);
        }

        private void HandleCultivationGain()
        {
            var delta = context.CurrentPlayer.Exp - lastExp;
            if (delta <= 0) return;
            floatingText.Show(targets.StatusBar, UiTexts.FeedbackCultivationGain(delta), FeedbackTextStyle.Cultivation, delta);
        }

        private void HandleInventoryGain()
        {
            foreach (var slot in context.CurrentPlayer.Inventory)
            {
                if (slot == null || string.IsNullOrEmpty(slot.ItemId)) continue;
                lastInventory.TryGetValue(slot.ItemId, out var before);
                var delta = slot.Count - before;
                if (delta <= 0) continue;
                var name = context.Database.Items.TryGetValue(slot.ItemId, out var item) ? item.Name : slot.ItemId;
                var text = UiTexts.FeedbackItemGain(name, delta);
                floatingText.Show(targets.StatusBar, text, FeedbackTextStyle.Gain, delta);
                toasts.Show(text, ToastSeverity.Success);
            }
        }

        private void CaptureSnapshot()
        {
            var p = context?.CurrentPlayer;
            if (p == null) return;
            lastGold = p.Gold;
            lastExp = p.Exp;
            lastInventory.Clear();
            foreach (var slot in p.Inventory)
                if (slot != null && !string.IsNullOrEmpty(slot.ItemId)) lastInventory[slot.ItemId] = slot.Count;
            hasSnapshot = true;
        }
    }
}
