// ============================================================
// ProceduralAudio.cs — PCM synthesis for SFX cues and BGM loops
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Xiuxian.Data;

namespace Xiuxian.Presentation.Audio
{
    public static class ProceduralAudio
    {
        private static readonly Dictionary<SoundCue, AudioClip> CueCache = new();
        private static readonly Dictionary<string, AudioClip> BgmCache = new();

        public static AudioClip ClipForCue(SoundCue cue)
        {
            if (CueCache.TryGetValue(cue, out var cached) && cached != null) return cached;
            var clip = CreateToneClip("sfx_" + cue, AudioTunings.StepsFor(cue));
            CueCache[cue] = clip;
            return clip;
        }

        public static AudioClip CreateToneClip(string name, IReadOnlyList<ToneStep> steps)
        {
            var totalSeconds = 0f;
            for (var i = 0; i < steps.Count; i++) totalSeconds += Math.Max(0f, steps[i].Duration);
            var sampleCount = Math.Max(1, Mathf.CeilToInt(totalSeconds * AudioTunings.SampleRate));
            var data = new float[sampleCount];
            var cursor = 0;

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var stepSamples = Mathf.Max(1, Mathf.RoundToInt(step.Duration * AudioTunings.SampleRate));
                for (var j = 0; j < stepSamples && cursor + j < data.Length; j++)
                {
                    var t = j / (float)AudioTunings.SampleRate;
                    data[cursor + j] = Wave(step.Type, step.Frequency, t) * Envelope(j, stepSamples);
                }
                cursor += stepSamples;
            }

            var clip = AudioClip.Create(name, data.Length, 1, AudioTunings.SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip BgmLoop(string regionId, int realmTier, string element)
        {
            var key = string.Concat(PresentationAssetPaths.Normalize(regionId), "_", Math.Max(0, realmTier), "_", PresentationAssetPaths.Normalize(element));
            if (BgmCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var clip = CreateBgmLoop("bgm_" + key, realmTier, element, StableHash(key));
            BgmCache[key] = clip;
            return clip;
        }

        private static AudioClip CreateBgmLoop(string name, int realmTier, string element, int seed)
        {
            var sampleCount = Mathf.CeilToInt(AudioTunings.BgmLoopSeconds * AudioTunings.SampleRate);
            var data = new float[sampleCount];
            var root = RootFrequencyFor(element, realmTier);
            var scale = ScaleFor(element);
            var rand = new System.Random(seed);
            var beatSeconds = AudioTunings.BgmLoopSeconds / 16f;

            for (var i = 0; i < sampleCount; i++)
            {
                var time = i / (float)AudioTunings.SampleRate;
                var beat = Math.Min(15, (int)(time / beatSeconds));
                var degree = scale[(beat + realmTier) % scale.Length];
                var arpeggioFreq = root * Mathf.Pow(2f, degree / 12f);
                var padFreq = root * 0.5f * Mathf.Pow(2f, scale[(beat / 4) % scale.Length] / 12f);
                var shimmer = root * 2f * Mathf.Pow(2f, scale[(beat * 3 + rand.Next(0, 1)) % scale.Length] / 12f);
                var loopEnvelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(time / AudioTunings.BgmLoopSeconds));
                data[i] =
                    0.11f * Wave(ToneWaveform.Sine, padFreq, time) +
                    0.055f * Wave(ToneWaveform.Triangle, arpeggioFreq, time) +
                    0.018f * Wave(ToneWaveform.Sine, shimmer, time);
                data[i] *= Mathf.Lerp(0.75f, 1f, loopEnvelope) * AudioTunings.BgmGain;
            }

            ApplyLoopSeamFade(data);
            var clip = AudioClip.Create(name, data.Length, 1, AudioTunings.SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Wave(ToneWaveform waveform, float frequency, float time)
        {
            var phase = Mathf.Repeat(time * frequency, 1f);
            switch (waveform)
            {
                case ToneWaveform.Square: return phase < 0.5f ? 1f : -1f;
                case ToneWaveform.Sawtooth: return 2f * phase - 1f;
                case ToneWaveform.Triangle: return 1f - 4f * Mathf.Abs(phase - 0.5f);
                default: return Mathf.Sin(2f * Mathf.PI * frequency * time);
            }
        }

        private static float Envelope(int sample, int totalSamples)
        {
            var attack = Mathf.Max(1, Mathf.RoundToInt(AudioTunings.EnvelopeAttackSeconds * AudioTunings.SampleRate));
            var release = Mathf.Max(1, Mathf.RoundToInt(AudioTunings.EnvelopeReleaseSeconds * AudioTunings.SampleRate));
            var a = sample < attack ? sample / (float)attack : 1f;
            var r = sample > totalSamples - release ? Math.Max(0f, (totalSamples - sample) / (float)release) : 1f;
            return Mathf.Min(a, r);
        }

        private static int[] ScaleFor(string element)
        {
            switch (element)
            {
                case ElementType.Fire: return new[] { 0, 3, 5, 7, 10, 12 };
                case ElementType.Water: return new[] { 0, 2, 3, 7, 9, 12 };
                case ElementType.Thunder: return new[] { 0, 2, 6, 7, 11, 12 };
                case ElementType.Earth: return new[] { 0, 2, 5, 7, 10, 12 };
                case ElementType.Metal: return new[] { 0, 4, 7, 9, 11, 12 };
                case ElementType.Wind: return new[] { 0, 2, 5, 9, 11, 12 };
                default: return new[] { 0, 2, 4, 7, 9, 12 };
            }
        }

        private static float RootFrequencyFor(string element, int realmTier)
        {
            var baseFreq = element == ElementType.Water ? 196f : element == ElementType.Fire ? 220f : element == ElementType.Thunder ? 185f : 174.61f;
            return baseFreq * Mathf.Pow(2f, Mathf.Clamp(realmTier, 0, 8) / 24f);
        }

        private static void ApplyLoopSeamFade(float[] data)
        {
            var fadeSamples = Mathf.Min(data.Length / 4, Mathf.RoundToInt(0.12f * AudioTunings.SampleRate));
            for (var i = 0; i < fadeSamples; i++)
            {
                var t = i / (float)fadeSamples;
                data[i] *= t;
                data[data.Length - 1 - i] *= t;
            }
        }

        private static int StableHash(string text)
        {
            unchecked
            {
                var hash = 23;
                for (var i = 0; i < (text?.Length ?? 0); i++) hash = hash * 31 + text[i];
                return hash;
            }
        }
    }
}
