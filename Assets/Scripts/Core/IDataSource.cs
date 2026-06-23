// ============================================================
// IDataSource.cs — 数据源抽象
// Unity 用 StreamingAssets 实现，dotnet 校验用文件系统实现
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using System.IO;

namespace Xiuxian.Core
{
    /// <summary>抽象 DLC 数据读取，解耦 Unity StreamingAssets 与普通文件系统。</summary>
    public interface IDataSource
    {
        /// <summary>列出 dlc/ 下的包目录名（如 core、cp-02-goudao）。</summary>
        IEnumerable<string> ListPackages();

        /// <summary>列出某包子目录下的文件名（如 dialogues/*.json）。</summary>
        IEnumerable<string> ListFiles(string package, string subfolder);

        /// <summary>判断某包下的某文件是否存在（相对 dlc/&lt;pkg&gt;/）。</summary>
        bool Exists(string package, string fileName);

        /// <summary>读取某包下某文件的全部文本。</summary>
        string ReadText(string package, string fileName);
    }

    /// <summary>基于文件系统根目录的实现：&lt;root&gt;/dlc/&lt;pkg&gt;/&lt;file&gt;。</summary>
    public sealed class FileSystemDataSource : IDataSource
    {
        private readonly string _dlcRoot;

        public FileSystemDataSource(string streamingAssetsRoot)
        {
            _dlcRoot = Path.Combine(streamingAssetsRoot, "dlc");
        }

        public IEnumerable<string> ListPackages()
        {
            if (!Directory.Exists(_dlcRoot)) yield break;
            foreach (var dir in Directory.GetDirectories(_dlcRoot))
                yield return Path.GetFileName(dir);
        }

        public IEnumerable<string> ListFiles(string package, string subfolder)
        {
            var dir = Path.Combine(_dlcRoot, package, subfolder);
            if (!Directory.Exists(dir)) yield break;
            foreach (var file in Directory.GetFiles(dir, "*.json"))
                yield return Path.GetFileName(file);
        }

        public bool Exists(string package, string fileName)
            => File.Exists(Path.Combine(_dlcRoot, package, fileName));

        public string ReadText(string package, string fileName)
            => File.ReadAllText(Path.Combine(_dlcRoot, package, fileName));
    }
}
