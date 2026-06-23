// ============================================================
// GameEventPayloads.cs — UnityEngine-free UI feedback payloads
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Core
{
    public enum ToastSeverity
    {
        Info,
        Success,
        Warning,
        Danger,
    }

    public enum FeedbackTextStyle
    {
        Damage,
        Crit,
        Heal,
        Gain,
        Cultivation,
    }

    public sealed class ToastRequest
    {
        public string Text;
        public ToastSeverity Severity = ToastSeverity.Info;
        public float DurationSeconds;
    }

    public sealed class FloatingFeedbackPayload
    {
        public string Text;
        public FeedbackTextStyle Style;
        public int Magnitude;
    }

    public sealed class CombatFeedbackPayload
    {
        public int Damage;
        public bool IsCrit;
        public bool IsDodge;
        public bool FromPlayer;
        public string SourceName;
        public string TargetName;
        public readonly List<FloatingFeedbackPayload> ExtraTexts = new();
    }
}
