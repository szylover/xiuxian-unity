// ============================================================
// VfxOverlay.cs — top uGUI layer that hosts code-built particles
// ============================================================

using System.Collections;
using UnityEngine;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Vfx
{
    public sealed class VfxOverlay : MonoBehaviour
    {
        private RectTransform rectTransform;
        private VfxPool pool;

        public static VfxOverlay Attach(Transform parent)
        {
            if (parent == null) return null;
            var go = new GameObject("VfxOverlay", typeof(RectTransform), typeof(Canvas), typeof(VfxOverlay));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var canvas = go.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = PresentationTunings.VfxOverlaySortingOrder;
            return go.GetComponent<VfxOverlay>();
        }

        public bool Enabled { get; private set; } = true;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            pool = new VfxPool(rectTransform, PresentationTunings.VfxPoolPrewarmCount);
        }

        public VfxInstance Play(RectTransform anchor, VfxEffectDescriptor descriptor)
            => Play(AnchorPosition(anchor), descriptor);

        public VfxInstance Play(Vector2 anchoredPosition, VfxEffectDescriptor descriptor)
        {
            if (!Enabled || descriptor == null || pool == null || rectTransform == null) return null;
            var instance = pool.Acquire();
            instance.Play(anchoredPosition, descriptor);
            if (!descriptor.Looping) StartCoroutine(ReleaseAfter(instance, descriptor.Duration + descriptor.LifetimeMax + 0.18f));
            return instance;
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            gameObject.SetActive(enabled);
            if (!enabled) pool?.StopAll();
        }

        public Vector2 AnchorPosition(RectTransform anchor)
        {
            if (rectTransform == null || anchor == null) return Vector2.zero;
            var worldCenter = anchor.TransformPoint(anchor.rect.center);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out var local)
                ? local
                : Vector2.zero;
        }

        public void StopAll() => pool?.StopAll();

        private IEnumerator ReleaseAfter(VfxInstance instance, float seconds)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, seconds));
            instance?.Release();
        }
    }
}
