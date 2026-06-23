// ============================================================
// DlcPackMeta.cs — 内容包元信息（移植自各 manifest.ts）
// UnityEngine-free
// ============================================================

namespace Xiuxian.Data
{
    /// <summary>内容包元信息，对应 TS manifest（id/name/description/version/type/required）。</summary>
    public sealed class DlcPackMeta
    {
        public string Id;
        public string Name;
        public string Description;
        public string Version;
        public string Type;       // core | content-pack | expansion-pack
        public bool Required;
        public string FolderName; // dlc/ 下的目录名

        public DlcPackMeta(string folderName)
        {
            FolderName = folderName;
        }
    }
}
