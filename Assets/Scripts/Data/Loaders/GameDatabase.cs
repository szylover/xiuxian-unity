// ============================================================
// GameDatabase.cs — 全局注册表（移植自 registry/stores.ts 的各 Map）
// 目前覆盖物品；后续逐实体扩展（事件/妖兽/功法/区域/NPC/任务/境界...）
// UnityEngine-free
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Data
{
    /// <summary>
    /// 数据注册表。对应网页版 registry/stores.ts 中集中管理的各类 Map。
    /// 内容包通过 DlcLoader 注册进来；游戏系统从这里查询定义。
    /// </summary>
    public sealed class GameDatabase
    {
        public readonly Dictionary<string, DlcPackMeta> Packs = new();
        public readonly Dictionary<string, ItemDef> Items = new();

        /// <summary>已启用的包 id 集合（按加载顺序）。</summary>
        public readonly List<string> EnabledPackIds = new();

        public void Clear()
        {
            Packs.Clear();
            Items.Clear();
            EnabledPackIds.Clear();
        }

        public void RegisterItem(ItemDef item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id)) return;
            Items[item.Id] = item; // 后注册覆盖（与 TS Map.set 行为一致）
        }
    }
}
