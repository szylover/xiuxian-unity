// ============================================================
// FeedbackOverlay.cs — top uGUI layer for toasts and floating text
// ============================================================

using UnityEngine;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class FeedbackOverlay : MonoBehaviour
    {
        public RectTransform Root { get; private set; }
        public RectTransform FloatingLayer { get; private set; }
        public RectTransform ToastLayer { get; private set; }

        public static FeedbackOverlay Attach(Transform parent)
        {
            if (parent == null) return null;
            var go = new GameObject("FeedbackOverlay", typeof(RectTransform), typeof(Canvas), typeof(FeedbackOverlay));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var canvas = go.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = PresentationTunings.FeedbackOverlaySortingOrder;
            return go.GetComponent<FeedbackOverlay>();
        }

        private void Awake()
        {
            Root = GetComponent<RectTransform>();
            FloatingLayer = CreateLayer("FloatingTextLayer");
            ToastLayer = CreateLayer("ToastLayer");
        }

        public Vector2 AnchorPosition(RectTransform anchor)
        {
            if (Root == null || anchor == null) return Vector2.zero;
            var worldCenter = anchor.TransformPoint(anchor.rect.center);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(Root, screenPoint, null, out var local) ? local : Vector2.zero;
        }

        private RectTransform CreateLayer(string name)
        {
            var layer = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(transform, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;
            return layer;
        }
    }
}
