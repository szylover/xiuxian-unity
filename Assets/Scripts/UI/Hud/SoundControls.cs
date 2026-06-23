// ============================================================
// SoundControls.cs — HUD audio settings widget
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Presentation.Audio;

namespace Xiuxian.UI
{
    public sealed class SoundControls : IDisposable
    {
        private readonly AudioManager audioManager;
        private readonly Button muteButton;
        private readonly TMP_Text muteLabel;
        private readonly TMP_Text volumeValue;
        private readonly Slider slider;
        private bool suppressSlider;

        public SoundControls(Transform parent, AudioManager audioManager)
        {
            this.audioManager = audioManager;
            var root = UIBuilder.Rect("SoundControls", parent);
            UIBuilder.Horizontal(root, 8, 8).childAlignment = TextAnchor.MiddleLeft;

            UIBuilder.Layout(UIBuilder.Label(root.transform, UiTexts.AudioPanelTitle, 22, TextAlignmentOptions.Left).gameObject, preferredWidth: 54, preferredHeight: 52);
            muteButton = UIBuilder.Button(root.transform, string.Empty, ToggleMuted, false);
            UIBuilder.Layout(muteButton.gameObject, preferredWidth: 112, preferredHeight: 52);
            muteLabel = muteButton.GetComponentInChildren<TMP_Text>();

            UIBuilder.Layout(UIBuilder.Label(root.transform, UiTexts.AudioVolumeLabel, 20, TextAlignmentOptions.Left).gameObject, preferredWidth: 50, preferredHeight: 52);
            slider = UIBuilder.Slider(root.transform, audioManager != null ? audioManager.Volume * 100f : AudioTunings.DefaultVolume * 100f, 0f, 100f);
            UIBuilder.Layout(slider.gameObject, preferredWidth: 140, preferredHeight: 24);
            slider.onValueChanged.AddListener(OnSliderChanged);
            volumeValue = UIBuilder.Label(root.transform, string.Empty, 20, TextAlignmentOptions.Left);
            UIBuilder.Layout(volumeValue.gameObject, preferredWidth: 56, preferredHeight: 52);
            UIBuilder.Layout(root, preferredWidth: 430, preferredHeight: 52);

            if (audioManager != null) audioManager.SettingsChanged += OnSettingsChanged;
            Refresh();
        }

        public void Dispose()
        {
            if (audioManager != null) audioManager.SettingsChanged -= OnSettingsChanged;
            if (slider != null) slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void ToggleMuted()
        {
            if (audioManager == null) return;
            audioManager.SetMuted(!audioManager.Muted);
            if (!audioManager.Muted) audioManager.PlayCue(SoundCue.ButtonClick);
        }

        private void OnSliderChanged(float value)
        {
            if (suppressSlider || audioManager == null) return;
            audioManager.SetVolume(value / 100f);
            if (!audioManager.Muted) audioManager.PlayCue(SoundCue.ButtonClick);
        }

        private void OnSettingsChanged(bool muted, float volume) => Refresh();

        private void Refresh()
        {
            var muted = audioManager?.Muted ?? false;
            var volume = audioManager?.Volume ?? AudioTunings.DefaultVolume;
            if (muteLabel != null) muteLabel.text = muted ? UiTexts.AudioMuteOn : UiTexts.AudioMuteOff;
            if (volumeValue != null) volumeValue.text = UiTexts.AudioVolumeValue(Mathf.RoundToInt(volume * 100f));
            if (slider != null)
            {
                suppressSlider = true;
                slider.value = volume * 100f;
                suppressSlider = false;
            }
        }
    }
}
