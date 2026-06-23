// ============================================================
// UIBuilder.cs — runtime uGUI hierarchy helpers
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Xiuxian.Presentation.Audio;

namespace Xiuxian.UI
{
    public static class UIBuilder
    {
        public static Canvas CreateCanvas(string name = "XiuxianCanvas")
        {
            EnsureEventSystem();
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            Stretch(go.GetComponent<RectTransform>());
            return canvas;
        }

        public static GameObject Panel(Transform parent, string name, Color color)
        {
            var go = Rect(name, parent);
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        public static TMP_Text Label(Transform parent, string text, int size = 28, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = Rect("Label", parent);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = new Color(0.94f, 0.88f, 0.72f, 1f);
            label.raycastTarget = false;
            return label;
        }

        public static Button Button(Transform parent, string text, Action onClick, bool playClickSound = true)
        {
            var go = Panel(parent, "Button", new Color(0.28f, 0.20f, 0.11f, 0.95f));
            var button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(() =>
            {
                if (playClickSound) AudioManager.PlayButtonClickGlobal();
                onClick?.Invoke();
            });
            var label = Label(go.transform, text, 24);
            Stretch(label.rectTransform, 8, 4, 8, 4);
            return button;
        }

        public static TMP_Text SectionHeader(Transform parent, string text)
        {
            var label = Label(parent, text, 30, TextAlignmentOptions.Left);
            label.color = new Color(1f, 0.78f, 0.34f, 1f);
            Layout(label.gameObject, preferredHeight: 42);
            return label;
        }

        public static TMP_Text StatRow(Transform parent, string label, string value)
        {
            var row = Rect("StatRow", parent);
            Horizontal(row, 4, 8).childAlignment = TextAnchor.MiddleLeft;
            Layout(row, preferredHeight: 34);
            var left = Label(row.transform, label, 22, TextAlignmentOptions.Left);
            Layout(left.gameObject, preferredWidth: 210, preferredHeight: 32);
            var right = Label(row.transform, value, 22, TextAlignmentOptions.Left);
            right.color = new Color(0.98f, 0.92f, 0.76f, 1f);
            Layout(right.gameObject, flexibleWidth: 1, preferredHeight: 32);
            return right;
        }

        public static GameObject Card(Transform parent, string name = "Card")
        {
            var card = Panel(parent, name, new Color(0.10f, 0.075f, 0.052f, 0.92f));
            Vertical(card, 12, 8);
            Layout(card, flexibleWidth: 1);
            return card;
        }

        public static Slider ProgressBar(Transform parent, float value, float max)
        {
            var slider = Slider(parent, Math.Max(0, value), 0, Math.Max(1, max));
            slider.interactable = false;
            Layout(slider.gameObject, preferredHeight: 18);
            return slider;
        }

        public static Toggle Toggle(Transform parent, string text, bool value, Action<bool> onChanged)
        {
            var row = Rect("Toggle", parent);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10;
            var box = Panel(row.transform, "CheckmarkBox", new Color(0.12f, 0.10f, 0.08f, 1f));
            SetSize(box.GetComponent<RectTransform>(), 28, 28);
            var check = Panel(box.transform, "Checkmark", new Color(0.90f, 0.68f, 0.24f, 1f));
            Stretch(check.GetComponent<RectTransform>(), 5, 5, 5, 5);
            var label = Label(row.transform, text, 22, TextAlignmentOptions.Left);
            label.color = new Color(0.92f, 0.86f, 0.70f, 1f);
            var toggle = row.AddComponent<Toggle>();
            toggle.targetGraphic = box.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            return toggle;
        }

        public static Slider Slider(Transform parent, float value, float min, float max)
        {
            var root = Rect("Slider", parent);
            var bg = Panel(root.transform, "Background", new Color(0.12f, 0.09f, 0.06f, 1f));
            Stretch(bg.GetComponent<RectTransform>());
            var fillArea = Rect("Fill Area", root.transform);
            Stretch(fillArea.GetComponent<RectTransform>(), 4, 4, 4, 4);
            var fill = Panel(fillArea.transform, "Fill", new Color(0.75f, 0.48f, 0.16f, 1f));
            Stretch(fill.GetComponent<RectTransform>());
            var slider = root.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = bg.GetComponent<Image>();
            return slider;
        }

        public static GameObject ScrollList(Transform parent, string name = "ScrollList")
        {
            var root = Panel(parent, name, new Color(0.05f, 0.04f, 0.035f, 0.78f));
            var scroll = root.AddComponent<ScrollRect>();
            var viewport = Panel(root.transform, "Viewport", new Color(0, 0, 0, 0));
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            Stretch(viewport.GetComponent<RectTransform>(), 8, 8, 8, 8);
            var content = Rect("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            return content;
        }

        public static GameObject Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void Stretch(RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static void SetSize(RectTransform rt, float width, float height)
        {
            rt.sizeDelta = new Vector2(width, height);
        }

        public static LayoutElement Layout(GameObject go, float preferredWidth = -1, float preferredHeight = -1, float flexibleWidth = -1, float flexibleHeight = -1)
        {
            var e = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) e.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) e.preferredHeight = preferredHeight;
            if (flexibleWidth >= 0) e.flexibleWidth = flexibleWidth;
            if (flexibleHeight >= 0) e.flexibleHeight = flexibleHeight;
            return e;
        }

        public static VerticalLayoutGroup Vertical(GameObject go, int padding = 18, int spacing = 12)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup Horizontal(GameObject go, int padding = 18, int spacing = 12)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
