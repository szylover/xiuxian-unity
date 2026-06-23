// ============================================================
// PortraitView.cs — uGUI portrait slot bound to PresentationController
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.UI;

namespace Xiuxian.Presentation
{
    public sealed class PortraitView : IDisposable
    {
        private readonly PresentationController controller;
        private Image portraitImage;
        private Image overlayImage;
        private Image frameImage;
        private TMP_Text captionText;

        public GameObject Root { get; private set; }
        public Image PortraitImage => portraitImage;
        public Image FrameImage => frameImage;

        public PortraitView(Transform parent, PresentationController controller)
        {
            this.controller = controller;
            Build(parent);
            controller.PortraitChanged += OnPortraitChanged;
            if (controller.CurrentPortrait != null) Apply(controller.CurrentPortrait);
        }

        private void Build(Transform parent)
        {
            var root = UIBuilder.Panel(parent, "PortraitView", new Color(0.05f, 0.04f, 0.035f, 0.72f));
            Root = root;
            UIBuilder.Layout(root, preferredHeight: 238);
            UIBuilder.Vertical(root, 10, 6).childAlignment = TextAnchor.UpperCenter;

            var frame = UIBuilder.Panel(root.transform, "PortraitFrame", new Color(0.22f, 0.16f, 0.10f, 1f));
            UIBuilder.Layout(frame, preferredWidth: 182, preferredHeight: 182);
            frameImage = frame.GetComponent<Image>();

            var portrait = UIBuilder.Rect("PortraitImage", frame.transform);
            UIBuilder.Stretch(portrait.GetComponent<RectTransform>(), 8, 8, 8, 8);
            portraitImage = portrait.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;

            var overlay = UIBuilder.Rect("RealmOverlay", frame.transform);
            UIBuilder.Stretch(overlay.GetComponent<RectTransform>(), 8, 8, 8, 8);
            overlayImage = overlay.AddComponent<Image>();
            overlayImage.preserveAspect = true;
            overlayImage.raycastTarget = false;
            overlayImage.enabled = false;

            captionText = UIBuilder.Label(root.transform, UiTexts.PresentationPortrait, 20, TextAlignmentOptions.Center);
            UIBuilder.Layout(captionText.gameObject, preferredHeight: 30);
        }

        private void OnPortraitChanged(object sender, PortraitChangedEventArgs e) => Apply(e.Portrait);

        private void Apply(PortraitDescriptor descriptor)
        {
            if (descriptor == null || portraitImage == null) return;
            portraitImage.sprite = descriptor.Sprite;
            portraitImage.color = Color.white;
            frameImage.color = descriptor.Palette.Primary;
            if (overlayImage != null)
            {
                overlayImage.sprite = descriptor.RealmOverlay;
                overlayImage.enabled = descriptor.RealmOverlay != null;
            }
        }

        public void Dispose()
        {
            if (controller != null) controller.PortraitChanged -= OnPortraitChanged;
        }
    }
}
