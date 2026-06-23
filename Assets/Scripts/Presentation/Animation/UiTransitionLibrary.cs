// ============================================================
// UiTransitionLibrary.cs — reusable uGUI transitions and beat motion
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;

namespace Xiuxian.Presentation.Animation
{
    public static class UiTransitionLibrary
    {
        public static void PlayScreenEnter(GameObject screenRoot)
        {
            if (screenRoot == null) return;
            var group = EnsureCanvasGroup(screenRoot);
            group.blocksRaycasts = true;
            Tweener.CanvasGroupAlpha(group, 0f, 1f, PresentationTunings.ScreenFadeDuration, Tweener.EaseOutQuad);
            PlayOverlayFade(screenRoot.transform.parent, Color.black, 0.38f, 0f, PresentationTunings.ScreenFadeDuration, true);
        }

        public static void PlayPanelSwitch(Transform panelRoot)
        {
            if (panelRoot == null) return;
            var go = panelRoot.gameObject;
            var group = EnsureCanvasGroup(go);
            var rect = go.GetComponent<RectTransform>();
            group.alpha = 0f;
            Tweener.CanvasGroupAlpha(group, 0f, 1f, PresentationTunings.PanelFadeDuration, Tweener.EaseOutQuad);
            if (rect == null) return;
            var end = rect.anchoredPosition;
            var start = end + new Vector2(0f, -PresentationTunings.PanelSlideOffset);
            Tweener.RectAnchoredPosition(rect, start, end, PresentationTunings.PanelFadeDuration, Tweener.EaseOutQuad);
        }

        public static void PlaySceneTransition(SceneView view, ThemePalette palette)
        {
            if (view?.Root == null) return;
            var group = EnsureCanvasGroup(view.Root);
            Tweener.CanvasGroupAlpha(group, 0.55f, 1f, PresentationTunings.SceneFadeDuration, Tweener.EaseOutQuad);
            var rect = view.Root.GetComponent<RectTransform>();
            if (rect != null)
            {
                var end = rect.anchoredPosition;
                Tweener.RectAnchoredPosition(rect, end + new Vector2(0f, PresentationTunings.SceneSlideOffset), end, PresentationTunings.SceneFadeDuration, Tweener.EaseOutQuad);
            }

            var color = palette.Shadow;
            color.a = PresentationTunings.TransitionOverlayAlpha;
            PlayOverlayFade(view.Root.transform, color, color.a, 0f, PresentationTunings.SceneFadeDuration, true);
        }

        public static void PlayPortraitSwap(PortraitView view, ThemePalette palette)
        {
            if (view?.PortraitImage == null) return;
            var image = view.PortraitImage;
            var target = Color.white;
            var from = target;
            from.a = 0f;
            Tweener.ImageColor(image, from, target, PresentationTunings.PortraitCrossfadeDuration, Tweener.EaseOutQuad);
            if (view.FrameImage != null) FlashImage(view.FrameImage, palette.Accent, view.FrameImage.color, PresentationTunings.PortraitCrossfadeDuration);
        }

        public static void PlayBuildUp(RectTransform target, Color color)
        {
            if (target == null) return;
            Pulse(target, PresentationTunings.DefaultPulseScale, PresentationTunings.BeatBuildUpDuration);
            PlayOverlayFade(target, color, 0.16f, 0f, PresentationTunings.BeatBuildUpDuration, false);
        }

        public static void PlaySuccessBurst(RectTransform target, Color color)
        {
            if (target == null) return;
            Pulse(target, PresentationTunings.RealmPulseScale, PresentationTunings.BeatBurstDuration);
            PlayOverlayFade(target, color, 0.36f, 0f, PresentationTunings.BeatBurstDuration, false);
        }

        public static void PlayFailureShudder(RectTransform target)
        {
            if (target == null) return;
            var origin = target.anchoredPosition;
            var offset = PresentationTunings.FailureShudderOffset;
            Tweener.RectAnchoredPosition(target, origin + new Vector2(-offset, 0f), origin + new Vector2(offset, 0f), PresentationTunings.BeatFailureDuration * 0.33f, Tweener.EaseInOutQuad,
                () => Tweener.RectAnchoredPosition(target, origin + new Vector2(offset, 0f), origin, PresentationTunings.BeatFailureDuration * 0.67f, Tweener.EaseOutQuad));
        }

        public static void PlayCombatHit(RectTransform target, Color color)
        {
            if (target == null) return;
            PlayOverlayFade(target, color, PresentationTunings.CombatHitFlashAlpha, 0f, PresentationTunings.CombatFeedbackDuration, false);
            Pulse(target, 1.025f, PresentationTunings.CombatFeedbackDuration);
        }

        public static void PlayCultivationPulse(RectTransform target, Color color)
        {
            if (target == null) return;
            PlayOverlayFade(target, color, 0.10f, 0f, PresentationTunings.CultivationPulseDuration, false);
        }

        public static void PlayGameOverFade(Transform parent)
        {
            if (parent == null) return;
            PlayOverlayFade(parent, Color.black, 0f, 0.86f, PresentationTunings.GameOverFadeDuration, false);
        }

        public static void PlayRealmFlourish(Transform parent, Color color)
        {
            if (parent == null) return;
            var go = new GameObject("RealmFlourish", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = UiTexts.AnimationRealmAdvance;
            label.fontSize = 42;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            Tweener.CanvasGroupAlpha(group, 0f, 1f, PresentationTunings.BeatBuildUpDuration, Tweener.EaseOutQuad,
                () => Tweener.CanvasGroupAlpha(group, 1f, 0f, PresentationTunings.BeatBurstDuration, Tweener.EaseOutQuad, () => Object.Destroy(go)));
            Tweener.RectScale(rect, Vector3.one * 0.88f, Vector3.one * 1.08f, PresentationTunings.BeatBuildUpDuration + PresentationTunings.BeatBurstDuration, Tweener.EaseOutBack);
        }

        private static void Pulse(RectTransform rect, float scale, float duration)
        {
            var original = rect.localScale;
            Tweener.RectScale(rect, original, original * scale, duration * 0.45f, Tweener.EaseOutBack,
                () => Tweener.RectScale(rect, rect.localScale, original, duration * 0.55f, Tweener.EaseOutQuad));
        }

        private static void FlashImage(Image image, Color flash, Color target, float duration)
        {
            if (image == null) return;
            Tweener.ImageColor(image, flash, target, duration, Tweener.EaseOutQuad);
        }

        private static void PlayOverlayFade(Transform parent, Color color, float fromAlpha, float toAlpha, float duration, bool stretch)
        {
            if (parent == null) return;
            var go = new GameObject("AnimationOverlay", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            go.transform.SetAsLastSibling();
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            var from = color;
            from.a = fromAlpha;
            var to = color;
            to.a = toAlpha;
            image.color = from;
            Tweener.ImageColor(image, from, to, duration, Tweener.EaseOutQuad, () => Object.Destroy(go));
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            return group;
        }
    }
}
