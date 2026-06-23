// ============================================================
// ToastContainer.cs — stacked transient uGUI toast queue
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.Core;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class ToastContainer
    {
        private sealed class ToastView
        {
            public GameObject Root;
            public RectTransform Rect;
            public CanvasGroup Group;
            public TMP_Text Label;
            public TweenHandle Lifetime;
        }

        private readonly RectTransform host;
        private readonly Queue<ToastRequest> pending = new();
        private readonly List<ToastView> visible = new();

        public ToastContainer(FeedbackOverlay overlay)
        {
            if (overlay?.ToastLayer == null) return;
            host = new GameObject("ToastContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(overlay.ToastLayer, false);
            host.anchorMin = new Vector2(1f, 1f);
            host.anchorMax = new Vector2(1f, 1f);
            host.pivot = new Vector2(1f, 1f);
            host.anchoredPosition = new Vector2(-24f, -24f);
            host.sizeDelta = new Vector2(PresentationTunings.ToastWidth, 420f);
            var layout = host.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = PresentationTunings.ToastSpacing;
            layout.childAlignment = TextAnchor.UpperRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        public void Enqueue(string text, ToastSeverity severity, float duration = 0f)
        {
            if (host == null || string.IsNullOrEmpty(text)) return;
            pending.Enqueue(new ToastRequest { Text = text, Severity = severity, DurationSeconds = duration });
            Pump();
        }

        public void Dispose()
        {
            pending.Clear();
            foreach (var view in visible) view.Lifetime?.Cancel();
            visible.Clear();
        }

        private void Pump()
        {
            while (visible.Count < PresentationTunings.ToastMaxVisible && pending.Count > 0)
                Show(pending.Dequeue());
        }

        private void Show(ToastRequest request)
        {
            var view = CreateView(request);
            visible.Add(view);
            var from = new Vector2(PresentationTunings.ToastSlideOffset, 0f);
            view.Rect.anchoredPosition = from;
            view.Group.alpha = 0f;
            Tweener.To(PresentationTunings.ToastFadeDuration, t =>
            {
                if (view.Root == null) return;
                view.Rect.anchoredPosition = Vector2.LerpUnclamped(from, Vector2.zero, Tweener.EaseOutQuad(t));
                view.Group.alpha = t;
            }, Tweener.EaseOutQuad);
            var duration = request.DurationSeconds > 0f ? request.DurationSeconds : PresentationTunings.ToastDefaultDuration;
            view.Lifetime = Tweener.Delay(duration, () => Dismiss(view));
        }

        private ToastView CreateView(ToastRequest request)
        {
            var root = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            root.transform.SetParent(host, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(PresentationTunings.ToastWidth, PresentationTunings.ToastHeight);
            var image = root.GetComponent<Image>();
            image.color = BackgroundFor(request.Severity);
            var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(root.transform, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(18f, 6f);
            label.rectTransform.offsetMax = new Vector2(-18f, -6f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 22f;
            label.color = Color.white;
            label.text = request.Text;
            label.raycastTarget = false;
            return new ToastView { Root = root, Rect = rect, Group = root.GetComponent<CanvasGroup>(), Label = label };
        }

        private void Dismiss(ToastView view)
        {
            if (view?.Root == null) return;
            Tweener.To(PresentationTunings.ToastFadeDuration, t =>
            {
                if (view.Root == null) return;
                view.Group.alpha = 1f - t;
                view.Rect.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, new Vector2(PresentationTunings.ToastSlideOffset, 0f), t);
            }, Tweener.EaseInOutQuad, () =>
            {
                visible.Remove(view);
                if (view.Root != null) Object.Destroy(view.Root);
                Pump();
            });
        }

        private static Color BackgroundFor(ToastSeverity severity) => severity switch
        {
            ToastSeverity.Success => new Color(0.12f, 0.42f, 0.22f, 0.94f),
            ToastSeverity.Warning => new Color(0.54f, 0.34f, 0.08f, 0.94f),
            ToastSeverity.Danger => new Color(0.52f, 0.12f, 0.10f, 0.94f),
            _ => new Color(0.10f, 0.18f, 0.32f, 0.94f),
        };
    }
}
