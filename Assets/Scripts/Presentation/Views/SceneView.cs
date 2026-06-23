// ============================================================
// SceneView.cs — uGUI scene backdrop/header bound to PresentationController
// ============================================================

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.UI;

namespace Xiuxian.Presentation
{
    public sealed class SceneView : IDisposable
    {
        private readonly PresentationController controller;
        private Image backgroundImage;
        private Image shadeImage;
        private TMP_Text titleText;
        private TMP_Text descriptionText;
        private TMP_Text exitsText;
        private TMP_Text npcsText;

        public SceneView(Transform parent, PresentationController controller)
        {
            this.controller = controller;
            Build(parent);
            controller.SceneChanged += OnSceneChanged;
            if (controller.CurrentScene != null) Apply(controller.CurrentScene);
        }

        private void Build(Transform parent)
        {
            var root = UIBuilder.Rect("SceneBackdrop", parent);
            UIBuilder.Stretch(root.GetComponent<RectTransform>());
            backgroundImage = root.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.06f, 0.04f, 0.9f);
            backgroundImage.preserveAspect = false;
            backgroundImage.raycastTarget = false;

            var shade = UIBuilder.Panel(root.transform, "SceneShade", new Color(0.02f, 0.018f, 0.015f, 0.58f));
            UIBuilder.Stretch(shade.GetComponent<RectTransform>());
            shadeImage = shade.GetComponent<Image>();
            shadeImage.raycastTarget = false;

            var header = UIBuilder.Panel(root.transform, "SceneHeader", new Color(0.04f, 0.032f, 0.026f, 0.78f));
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(18f, -132f);
            headerRect.offsetMax = new Vector2(-18f, -16f);
            UIBuilder.Vertical(header, 10, 4);

            titleText = UIBuilder.Label(header.transform, string.Empty, 26, TextAlignmentOptions.Left);
            UIBuilder.Layout(titleText.gameObject, preferredHeight: 34);
            descriptionText = UIBuilder.Label(header.transform, string.Empty, 18, TextAlignmentOptions.Left);
            UIBuilder.Layout(descriptionText.gameObject, preferredHeight: 28);
            exitsText = UIBuilder.Label(header.transform, string.Empty, 17, TextAlignmentOptions.Left);
            UIBuilder.Layout(exitsText.gameObject, preferredHeight: 24);
            npcsText = UIBuilder.Label(header.transform, string.Empty, 17, TextAlignmentOptions.Left);
            UIBuilder.Layout(npcsText.gameObject, preferredHeight: 24);
        }

        private void OnSceneChanged(object sender, SceneChangedEventArgs e) => Apply(e.Scene);

        private void Apply(SceneDescriptor descriptor)
        {
            if (descriptor == null || backgroundImage == null) return;
            backgroundImage.sprite = descriptor.Background;
            backgroundImage.color = Color.white;
            shadeImage.color = descriptor.Palette.Shadow;
            titleText.text = UiTexts.SceneTitle(descriptor.Title);
            descriptionText.text = descriptor.Description;
            exitsText.text = UiTexts.SceneExits(string.Join("、", descriptor.Exits.Take(5).Select(e => e.Region?.Name).Where(n => !string.IsNullOrEmpty(n))));
            npcsText.text = UiTexts.SceneNpcs(string.Join("、", descriptor.Npcs.Take(4).Select(n => n.Name).Where(n => !string.IsNullOrEmpty(n))));
        }

        public void Dispose()
        {
            if (controller != null) controller.SceneChanged -= OnSceneChanged;
        }
    }
}
