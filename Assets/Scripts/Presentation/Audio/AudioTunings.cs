// ============================================================
// AudioTunings.cs — procedural audio constants ported from web
// ============================================================

using System;

namespace Xiuxian.Presentation.Audio
{
    public enum SoundCue
    {
        CultivateTick,
        CombatHit,
        BreakthroughSuccess,
        BreakthroughFailure,
        ItemGain,
        ButtonClick,
        Death,
    }

    public enum ToneWaveform
    {
        Sine,
        Square,
        Sawtooth,
        Triangle,
    }

    public readonly struct ToneStep
    {
        public ToneStep(float frequency, float duration, ToneWaveform type)
        {
            Frequency = frequency;
            Duration = duration;
            Type = type;
        }

        public float Frequency { get; }
        public float Duration { get; }
        public ToneWaveform Type { get; }
    }

    public static class AudioTunings
    {
        public const string SoundSettingsKey = "xiuxian_sound_settings";
        public const float DefaultVolume = 0.45f;
        public const float SfxGain = 0.18f;
        public const int SampleRate = 44100;
        public const float EnvelopeAttackSeconds = 0.01f;
        public const float EnvelopeReleaseSeconds = 0.02f;
        public const float BgmCrossfadeSeconds = 1.2f;
        public const float BgmLoopSeconds = 8f;
        public const float BgmGain = 0.32f;
        public const string RegionBgmResourcePrefix = "Bgm/";
        public const string RealmBgmResourcePrefix = "Bgm/realm_";

        public static readonly ToneStep[] CultivateTick =
        {
            new(523.25f, 0.08f, ToneWaveform.Sine),
            new(659.25f, 0.12f, ToneWaveform.Sine),
        };

        public static readonly ToneStep[] CombatHit =
        {
            new(180f, 0.05f, ToneWaveform.Square),
            new(95f, 0.08f, ToneWaveform.Sawtooth),
        };

        public static readonly ToneStep[] BreakthroughSuccess =
        {
            new(523.25f, 0.09f, ToneWaveform.Triangle),
            new(783.99f, 0.09f, ToneWaveform.Triangle),
            new(1046.5f, 0.18f, ToneWaveform.Sine),
        };

        public static readonly ToneStep[] BreakthroughFailure =
        {
            new(392f, 0.08f, ToneWaveform.Triangle),
            new(246.94f, 0.18f, ToneWaveform.Sawtooth),
        };

        public static readonly ToneStep[] ItemGain =
        {
            new(659.25f, 0.07f, ToneWaveform.Sine),
            new(987.77f, 0.1f, ToneWaveform.Sine),
        };

        public static readonly ToneStep[] ButtonClick =
        {
            new(440f, 0.035f, ToneWaveform.Triangle),
        };

        public static readonly ToneStep[] Death =
        {
            new(220f, 0.12f, ToneWaveform.Sawtooth),
            new(164.81f, 0.18f, ToneWaveform.Sawtooth),
            new(110f, 0.24f, ToneWaveform.Sine),
        };

        public static ToneStep[] StepsFor(SoundCue cue)
        {
            switch (cue)
            {
                case SoundCue.CultivateTick: return CultivateTick;
                case SoundCue.CombatHit: return CombatHit;
                case SoundCue.BreakthroughSuccess: return BreakthroughSuccess;
                case SoundCue.BreakthroughFailure: return BreakthroughFailure;
                case SoundCue.ItemGain: return ItemGain;
                case SoundCue.ButtonClick: return ButtonClick;
                case SoundCue.Death: return Death;
                default: return Array.Empty<ToneStep>();
            }
        }
    }
}
