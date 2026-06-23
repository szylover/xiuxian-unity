// ============================================================
// VfxSettings.cs — persisted presentation-only VFX switches
// ============================================================

using UnityEngine;

namespace Xiuxian.Presentation.Vfx
{
    public static class VfxSettings
    {
        private const string EnabledKey = "xiuxian.presentation.vfx.enabled";

        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) != 0;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
