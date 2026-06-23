// ============================================================
// DlcLoader.cs — DLC 加载器（移植自各 loader.ts + index.ts 注册流程）
// 数据驱动：扫描 dlc/ 下各包，加载存在的实体 JSON 文件。
// 目前覆盖 items.json；后续逐实体扩展。
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Xiuxian.Core;

namespace Xiuxian.Data
{
    public sealed class DlcLoader
    {
        private readonly IDataSource _source;
        private readonly GameDatabase _db;

        public DlcLoader(IDataSource source, GameDatabase db)
        {
            _source = source;
            _db = db;
        }

        /// <summary>加载全部存在的包（core 优先，其余按名称）。</summary>
        public void LoadAll()
        {
            var packages = new List<string>(_source.ListPackages());
            packages.Sort(ComparePackages);
            foreach (var pkg in packages)
                LoadPackage(pkg);
        }

        /// <summary>加载单个包目录下已知的实体文件。</summary>
        public void LoadPackage(string package)
        {
            var meta = new DlcPackMeta(package) { Id = package };
            _db.Packs[package] = meta;
            _db.EnabledPackIds.Add(package);

            if (_source.Exists(package, "items.json"))
            {
                var items = Deserialize<List<ItemDef>>(_source.ReadText(package, "items.json"));
                if (items != null)
                    foreach (var it in items) _db.RegisterItem(it);
            }
        }

        private static T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);

        // core 永远最先加载（对应 index.ts 中 core 为必需基底包）
        private static int ComparePackages(string a, string b)
        {
            if (a == "core") return -1;
            if (b == "core") return 1;
            return string.CompareOrdinal(a, b);
        }
    }
}
