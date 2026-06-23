// ============================================================
// FloatingTextSystem.cs — pooled TMP floating combat/gain numbers
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Xiuxian.Core;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class FloatingTextSystem
    {
        private readonly FeedbackOverlay overlay;
        private readonly Queue<TMP_Text> pool = new();
        private readonly List<TMP_Text> active = new();
        private readonly System.Random random = new();

        public FloatingTextSystem(FeedbackOverlay overlay)
        {
            this.overlay = overlay;
            if (overlay?.FloatingLayer == null) return;
            for (var i = 0; i < PresentationTunings.FloatingTextPoolPrewarmCount; i++) pool.Enqueue(CreateLabel());
        }

        public void Show(RectTransform anchor, string text, FeedbackTextStyle style, int magnitude = 0)
        {
            if (!FeedbackSettings.Enabled || overlay?.FloatingLayer == null || string.IsNullOrEmpty(text)) return;
            ShowAt(overlay.AnchorPosition(anchor), text, style, magnitude);
        }

        public void ShowAt(Vector2 position, string text, FeedbackTextStyle style, int magnitude = 0)
        {
            if (!FeedbackSettings.Enabled || overlay?.FloatingLayer == null || string.IsNullOrEmpty(text)) return;
            if (active.Count >= PresentationTunings.FloatingTextMaxActive) Release(active[0]);
            var label = pool.Count > 0 ? pool.Dequeue() : CreateLabel();
            active.Add(label);
            Configure(label, text, style);
            var rect = label.rectTransform;
            var jitter = (float)(random.NextDouble() * 2.0 - 1.0) * PresentationTunings.FloatingTextJitter;
            var from = position + new Vector2(jitter, 0f);
            var to = from + Vector2.up * PresentationTunings.FloatingTextRise;
            var group = label.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            rect.anchoredPosition = from;
            rect.localScale = style == FeedbackTextStyle.Crit ? Vector3.one * PresentationTunings.FloatingTextPopScale : Vector3.one;
            label.gameObject.SetActive(true);

            Tweener.To(PresentationTunings.FloatingTextDuration, t =>
            {
                if (label == null) return;
                rect.anchoredPosition = Vector2.LerpUnclamped(from, to, Tweener.EaseOutQuad(t));
                group.alpha = 1f - t;
                if (style == FeedbackTextStyle.Crit) rect.localScale = Vector3.LerpUnclamped(Vector3.one * PresentationTunings.FloatingTextPopScale, Vector3.one, t);
            }, Tweener.EaseOutQuad, () => Release(label));
        }

        public void Dispose()
        {
            for (var i = active.Count - 1; i >= 0; i--) Release(active[i]);
            pool.Clear();
        }

        private TMP_Text CreateLabel()
        {
            var go = new GameObject("FloatingText", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(overlay.FloatingLayer, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 72f);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            go.SetActive(false);
            return label;
        }

        private static void Configure(TMP_Text label, string text, FeedbackTextStyle style)
        {
            label.text = text;
            label.fontSize = style == FeedbackTextStyle.Crit ? PresentationTunings.FloatingTextCritFontSize : PresentationTunings.FloatingTextFontSize;
            label.color = style switch
            {
                FeedbackTextStyle.Crit => new Color(1f, 0.78f, 0.20f, 1f),
                FeedbackTextStyle.Heal => new Color(0.36f, 1f, 0.48f, 1f),
                FeedbackTextStyle.Gain => new Color(0.36f, 0.72f, 1f, 1f),
                FeedbackTextStyle.Cultivation => new Color(0.35f, 1f, 0.95f, 1f),
                _ => new Color(1f, 0.28f, 0.22f, 1f),
            };
        }

        private void Release(TMP_Text label)
        {
            if (label == null) return;
            active.Remove(label);
            label.gameObject.SetActive(false);
            if (overlay?.FloatingLayer != null) pool.Enqueue(label);
        }
    }
}
