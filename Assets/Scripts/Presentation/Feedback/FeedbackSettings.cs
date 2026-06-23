// ============================================================
// FeedbackSettings.cs — persisted dynamic feedback switches
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation.Feedback
{
    public static class FeedbackSettings
    {
        private const string EnabledKey = "xiuxian.presentation.feedback.enabled";

        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) != 0;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool ReduceMotion => !Enabled;
    }
}
