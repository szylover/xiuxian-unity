// ============================================================
// UnityStreamingAssetsDataSource.cs — Unity StreamingAssets DLC reader
// ============================================================

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Xiuxian.Core;

namespace Xiuxian.App
{
    public sealed class UnityStreamingAssetsDataSource : IDataSource
    {
        private readonly string dlcRoot;

        public UnityStreamingAssetsDataSource()
            : this(Application.streamingAssetsPath)
        {
        }

        public UnityStreamingAssetsDataSource(string streamingAssetsRoot)
        {
            dlcRoot = Path.Combine(streamingAssetsRoot, "dlc");
        }

        public IEnumerable<string> ListPackages()
        {
            if (!Directory.Exists(dlcRoot)) yield break;
            foreach (var dir in Directory.GetDirectories(dlcRoot))
                yield return Path.GetFileName(dir);
        }

        public IEnumerable<string> ListFiles(string package, string subfolder)
        {
            var dir = Path.Combine(dlcRoot, package, subfolder);
            if (!Directory.Exists(dir)) yield break;
            foreach (var file in Directory.GetFiles(dir, "*.json"))
                yield return Path.GetFileName(file);
        }

        public bool Exists(string package, string fileName)
            => File.Exists(Path.Combine(dlcRoot, package, fileName));

        public string ReadText(string package, string fileName)
            => File.ReadAllText(Path.Combine(dlcRoot, package, fileName));
    }
}
