// ============================================================
// AudioManager.cs — runtime SFX/BGM playback and persisted settings
// ============================================================

using System;
using System.Collections;
using UnityEngine;

namespace Xiuxian.Presentation.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        [Serializable]
        private sealed class SoundSettings
        {
            public bool muted;
            public float volume = AudioTunings.DefaultVolume;
        }

        private AudioSource sfxSource;
        private AudioSource bgmSource;
        private AudioSource nextBgmSource;
        private Coroutine crossfade;
        private SoundSettings settings;
        private string currentBgmKey;

        public static AudioManager Instance { get; private set; }
        public event Action<bool, float> SettingsChanged;

        public bool Muted => settings?.muted ?? false;
        public float Volume => settings?.volume ?? AudioTunings.DefaultVolume;

        public static void PlayButtonClickGlobal()
        {
            if (Instance != null) Instance.PlayCue(SoundCue.ButtonClick);
        }

        public void Initialize()
        {
            EnsureSources();
            settings = LoadSettings();
            ApplyVolumes();
            if (Instance == null) Instance = this;
        }

        private void Awake() => Initialize();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayCue(SoundCue cue)
        {
            if (Muted || Volume <= 0f) return;
            EnsureSources();
            if (sfxSource == null) return;
            var clip = ProceduralAudio.ClipForCue(cue);
            if (clip != null) sfxSource.PlayOneShot(clip, Volume * AudioTunings.SfxGain);
        }

        public void SetMuted(bool muted)
        {
            EnsureSettings();
            if (settings.muted == muted) return;
            settings.muted = muted;
            SaveSettings();
            ApplyVolumes();
            SettingsChanged?.Invoke(Muted, Volume);
        }

        public void SetVolume(float volume)
        {
            EnsureSettings();
            volume = Mathf.Clamp01(volume);
            if (Mathf.Approximately(settings.volume, volume)) return;
            settings.volume = volume;
            if (volume <= 0f) settings.muted = true;
            SaveSettings();
            ApplyVolumes();
            SettingsChanged?.Invoke(Muted, Volume);
        }

        public void PlayBgmFor(string regionId, int realmTier, string element)
        {
            EnsureSources();
            if (bgmSource == null) return;
            regionId = string.IsNullOrEmpty(regionId) ? "default" : regionId;
            var key = regionId + "|" + realmTier + "|" + element;
            if (key == currentBgmKey && bgmSource.isPlaying) return;
            currentBgmKey = key;
            var clip = LoadResourceBgm(regionId, realmTier) ?? ProceduralAudio.BgmLoop(regionId, realmTier, element);
            CrossfadeTo(clip);
        }

        private void CrossfadeTo(AudioClip clip)
        {
            if (clip == null) return;
            if (crossfade != null) StopCoroutine(crossfade);
            crossfade = StartCoroutine(CrossfadeRoutine(clip));
        }

        private IEnumerator CrossfadeRoutine(AudioClip clip)
        {
            nextBgmSource.clip = clip;
            nextBgmSource.loop = true;
            nextBgmSource.volume = 0f;
            nextBgmSource.Play();

            var from = bgmSource.volume;
            var target = Muted ? 0f : Volume;
            var elapsed = 0f;
            while (elapsed < AudioTunings.BgmCrossfadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / AudioTunings.BgmCrossfadeSeconds);
                bgmSource.volume = Mathf.Lerp(from, 0f, t);
                nextBgmSource.volume = Mathf.Lerp(0f, target, t);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.clip = nextBgmSource.clip;
            bgmSource.time = nextBgmSource.time;
            bgmSource.loop = true;
            bgmSource.volume = target;
            bgmSource.Play();
            nextBgmSource.Stop();
            crossfade = null;
        }

        private static AudioClip LoadResourceBgm(string regionId, int realmTier)
        {
            var normalizedRegion = PresentationAssetPaths.Normalize(regionId);
            var byRegion = Resources.Load<AudioClip>(AudioTunings.RegionBgmResourcePrefix + normalizedRegion);
            return byRegion != null ? byRegion : Resources.Load<AudioClip>(AudioTunings.RealmBgmResourcePrefix + Math.Max(0, realmTier));
        }

        private void EnsureSources()
        {
            if (sfxSource == null) sfxSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            if (bgmSource == null)
            {
                var bgm = new GameObject("BgmSource");
                bgm.transform.SetParent(transform, false);
                bgmSource = bgm.AddComponent<AudioSource>();
                bgmSource.loop = true;
            }
            if (nextBgmSource == null)
            {
                var next = new GameObject("BgmSourceNext");
                next.transform.SetParent(transform, false);
                nextBgmSource = next.AddComponent<AudioSource>();
                nextBgmSource.loop = true;
            }
        }

        private void ApplyVolumes()
        {
            EnsureSources();
            var volume = Muted ? 0f : Volume;
            if (sfxSource != null) sfxSource.volume = volume;
            if (bgmSource != null) bgmSource.volume = volume;
            if (nextBgmSource != null) nextBgmSource.volume = Mathf.Min(nextBgmSource.volume, volume);
        }

        private void EnsureSettings()
        {
            if (settings == null) settings = LoadSettings();
        }

        private static SoundSettings LoadSettings()
        {
            try
            {
                var raw = PlayerPrefs.GetString(AudioTunings.SoundSettingsKey, string.Empty);
                if (string.IsNullOrEmpty(raw)) return new SoundSettings();
                var loaded = JsonUtility.FromJson<SoundSettings>(raw) ?? new SoundSettings();
                loaded.volume = Mathf.Clamp01(loaded.volume);
                return loaded;
            }
            catch
            {
                return new SoundSettings();
            }
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetString(AudioTunings.SoundSettingsKey, JsonUtility.ToJson(settings));
            PlayerPrefs.Save();
        }
    }
}
