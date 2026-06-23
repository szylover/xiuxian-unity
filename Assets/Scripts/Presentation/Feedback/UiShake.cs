// ============================================================
// UiShake.cs — reduce-motion-aware uGUI / camera shake
// ============================================================

using UnityEngine;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class UiShake
    {
        private readonly RectTransform target;
        private readonly Camera cameraTarget;
        private readonly Vector2 originalPosition;
        private readonly Vector3 originalCameraPosition;
        private TweenHandle handle;

        public UiShake(RectTransform target, Camera cameraTarget = null)
        {
            this.target = target;
            this.cameraTarget = cameraTarget;
            if (target != null) originalPosition = target.anchoredPosition;
            if (cameraTarget != null) originalCameraPosition = cameraTarget.transform.localPosition;
        }

        public void Shake(float intensity, float duration)
        {
            if (!FeedbackSettings.Enabled) return;
            if (target == null && cameraTarget == null) return;
            handle?.Cancel();
            handle = Tweener.To(duration, t =>
            {
                var decay = 1f - t;
                var offset = Random.insideUnitCircle * intensity * decay;
                if (target != null) target.anchoredPosition = originalPosition + offset;
                if (cameraTarget != null) cameraTarget.transform.localPosition = originalCameraPosition + new Vector3(offset.x, offset.y, 0f) * 0.01f;
            }, Tweener.EaseOutQuad, Reset);
        }

        public void Reset()
        {
            if (target != null) target.anchoredPosition = originalPosition;
            if (cameraTarget != null) cameraTarget.transform.localPosition = originalCameraPosition;
        }
    }
}
