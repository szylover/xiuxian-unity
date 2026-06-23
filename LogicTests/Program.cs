// ============================================================
// Program.cs — 数据层校验入口（dotnet 运行，无需 Unity 编辑器）
// 加载 Assets/StreamingAssets/dlc 下全部 JSON，校验反序列化与计数。
// ============================================================

using System;
using System.IO;
using System.Linq;
using Xiuxian.Core;
using Xiuxian.Data;

namespace Xiuxian.LogicTests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            // 定位 StreamingAssets：默认相对本工程 ..\Assets\StreamingAssets
            string streaming = args.Length > 0
                ? args[0]
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "StreamingAssets"));

            if (!Directory.Exists(streaming))
            {
                Console.Error.WriteLine($"[FAIL] StreamingAssets 未找到: {streaming}");
                return 1;
            }

            int failures = 0;

            var source = new FileSystemDataSource(streaming);
            var db = new GameDatabase();
            var loader = new DlcLoader(source, db);

            try
            {
                loader.LoadAll();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] 加载异常: {ex.Message}");
                return 1;
            }

            Console.WriteLine($"[OK] 已加载包数: {db.Packs.Count}  ({string.Join(", ", db.EnabledPackIds)})");
            Console.WriteLine($"[OK] 物品总数: {db.Items.Count}");

            // 断言：core 必须存在且 core:hp_pill 可解析其效果
            if (!db.Items.TryGetValue("core:hp_pill", out var hpPill))
            {
                Console.Error.WriteLine("[FAIL] 缺少 core:hp_pill");
                failures++;
            }
            else
            {
                var rng = new Random(1);
                double newHp = hpPill.Effects["hp"].Resolve(10, 200, rng);
                if (Math.Abs(newHp - 60) > 0.001)
                {
                    Console.Error.WriteLine($"[FAIL] core:hp_pill 效果解析错误: 期望 60, 实得 {newHp}");
                    failures++;
                }
                else
                {
                    Console.WriteLine($"[OK] core:hp_pill 效果解析: 10 + 50 = {newHp}");
                }
            }

            // 断言：每个包至少加载到 1 件物品（除非该包确无 items.json）
            foreach (var pkgId in db.EnabledPackIds)
            {
                bool hasItems = db.Items.Values.Any(i => i.Id != null && i.Id.StartsWith(PackPrefix(pkgId)));
                // 不强制：部分包可能不含物品；仅打印诊断
                Console.WriteLine($"    - {pkgId}: items? {(hasItems ? "yes" : "no")}");
            }

            // 断言：所有物品 id 非空且含命名空间冒号
            int badIds = db.Items.Values.Count(i => string.IsNullOrEmpty(i.Id) || !i.Id.Contains(':'));
            if (badIds > 0)
            {
                Console.Error.WriteLine($"[FAIL] {badIds} 件物品 id 非法（应为 namespace:id）");
                failures++;
            }
            else
            {
                Console.WriteLine("[OK] 所有物品 id 合法（含命名空间）");
            }

            if (failures == 0)
            {
                Console.WriteLine("\n✅ 数据层校验全部通过");
                return 0;
            }

            Console.Error.WriteLine($"\n❌ 校验失败：{failures} 项");
            return 1;
        }

        // 包 id → 物品命名空间前缀（core → "core:", cp-02-goudao → "cp-02:" 之类的近似前缀）
        private static string PackPrefix(string pkgId)
        {
            if (pkgId == "core") return "core:";
            // cp-02-goudao → cp-02:  ; exp-01-qiandao → exp-01:
            var parts = pkgId.Split('-');
            if (parts.Length >= 2) return $"{parts[0]}-{parts[1]}:";
            return pkgId + ":";
        }
    }
}
