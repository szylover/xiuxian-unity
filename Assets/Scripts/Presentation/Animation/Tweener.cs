// ============================================================
// Tweener.cs — lightweight Update-driven tween helper for uGUI
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xiuxian.Presentation.Animation
{
    public sealed class TweenHandle
    {
        internal int Id;
        internal bool Active;
        public void Cancel() => Tweener.Cancel(this);
    }

    public static class Tweener
    {
        public delegate float Ease(float t);

        private sealed class Tween
        {
            public int Id;
            public float Duration;
            public float Elapsed;
            public bool Unscaled;
            public Action<float> Apply;
            public Action Complete;
            public Ease Easing;
            public TweenHandle Handle;
        }

        private static readonly List<Tween> Tweens = new();
        private static int nextId = 1;
        private static PresentationRunner runner;

        public static readonly Ease Linear = t => Mathf.Clamp01(t);
        public static readonly Ease EaseOutQuad = t => 1f - (1f - Mathf.Clamp01(t)) * (1f - Mathf.Clamp01(t));
        public static readonly Ease EaseInOutQuad = t =>
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
        };
        public static readonly Ease EaseOutBack = t =>
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        };

        public static TweenHandle To(float duration, Action<float> apply, Ease easing = null, Action complete = null, bool unscaled = true)
        {
            if (apply == null) return CompletedHandle();
            if (duration <= 0f)
            {
                apply(1f);
                complete?.Invoke();
                return CompletedHandle();
            }

            EnsureRunner();
            var handle = new TweenHandle { Id = nextId++, Active = true };
            Tweens.Add(new Tween
            {
                Id = handle.Id,
                Duration = duration,
                Apply = apply,
                Complete = complete,
                Easing = easing ?? EaseInOutQuad,
                Unscaled = unscaled,
                Handle = handle,
            });
            return handle;
        }

        public static TweenHandle Delay(float duration, Action complete, bool unscaled = true)
            => To(duration, _ => { }, Linear, complete, unscaled);

        public static TweenHandle CanvasGroupAlpha(CanvasGroup group, float from, float to, float duration, Ease easing = null, Action complete = null)
        {
            if (group == null) return CompletedHandle();
            group.alpha = from;
            return To(duration, t => { if (group != null) group.alpha = Mathf.Lerp(from, to, t); }, easing, complete);
        }

        public static TweenHandle RectAnchoredPosition(RectTransform rect, Vector2 from, Vector2 to, float duration, Ease easing = null, Action complete = null)
        {
            if (rect == null) return CompletedHandle();
            rect.anchoredPosition = from;
            return To(duration, t => { if (rect != null) rect.anchoredPosition = Vector2.LerpUnclamped(from, to, t); }, easing, complete);
        }

        public static TweenHandle RectScale(RectTransform rect, Vector3 from, Vector3 to, float duration, Ease easing = null, Action complete = null)
        {
            if (rect == null) return CompletedHandle();
            rect.localScale = from;
            return To(duration, t => { if (rect != null) rect.localScale = Vector3.LerpUnclamped(from, to, t); }, easing, complete);
        }

        public static TweenHandle ImageColor(Image image, Color from, Color to, float duration, Ease easing = null, Action complete = null)
        {
            if (image == null) return CompletedHandle();
            image.color = from;
            return To(duration, t => { if (image != null) image.color = Color.LerpUnclamped(from, to, t); }, easing, complete);
        }

        internal static void UpdateAll(float deltaTime, float unscaledDeltaTime)
        {
            for (var i = Tweens.Count - 1; i >= 0; i--)
            {
                var tween = Tweens[i];
                if (tween.Handle == null || !tween.Handle.Active)
                {
                    Tweens.RemoveAt(i);
                    continue;
                }

                tween.Elapsed += tween.Unscaled ? unscaledDeltaTime : deltaTime;
                var normalized = tween.Duration <= 0f ? 1f : Mathf.Clamp01(tween.Elapsed / tween.Duration);
                tween.Apply?.Invoke(tween.Easing(normalized));
                if (normalized < 1f) continue;
                tween.Handle.Active = false;
                Tweens.RemoveAt(i);
                tween.Complete?.Invoke();
            }
        }

        internal static void Cancel(TweenHandle handle)
        {
            if (handle == null || !handle.Active) return;
            handle.Active = false;
        }

        private static TweenHandle CompletedHandle() => new TweenHandle { Active = false };

        private static void EnsureRunner()
        {
            if (runner != null) return;
            var go = new GameObject("PresentationRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<PresentationRunner>();
        }
    }

    public sealed class PresentationRunner : MonoBehaviour
    {
        private void Update() => Tweener.UpdateAll(Time.deltaTime, Time.unscaledDeltaTime);
    }
}
