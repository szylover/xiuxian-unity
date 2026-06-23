// ============================================================
// Common.cs — 通用枚举/常量（移植自 src/game/types/common.ts）
// UnityEngine-free：可在 Unity 与 dotnet 下编译
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Data
{
    /// <summary>元素类型常量（对应 TS ElementType 字符串联合）。</summary>
    public static class ElementType
    {
        public const string Fire = "fire";
        public const string Water = "water";
        public const string Thunder = "thunder";
        public const string Wind = "wind";
        public const string Earth = "earth";
        public const string Wood = "wood";
        public const string Metal = "metal";
    }

    /// <summary>物品分类常量（对应 TS ItemCategory）。</summary>
    public static class ItemCategory
    {
        public const string Weapon = "weapon";
        public const string Armor = "armor";
        public const string Accessory = "accessory";
        public const string Consumable = "consumable";
        public const string Material = "material";
        public const string Technique = "technique";
        public const string Misc = "misc";
        public const string Scroll = "scroll";
    }

    /// <summary>品质常量（对应 TS ItemRarity）。</summary>
    public static class ItemRarity
    {
        public const string Common = "common";
        public const string Uncommon = "uncommon";
        public const string Rare = "rare";
        public const string Epic = "epic";
        public const string Legendary = "legendary";
    }

    public static class ElementTable
    {
        public const double CounterMultiplier = 1.3;

        /// <summary>元素克制表：攻击元素 → 可克制的防御元素列表。</summary>
        public static readonly IReadOnlyDictionary<string, string[]> Counter =
            new Dictionary<string, string[]>
            {
                { ElementType.Water,   new[] { ElementType.Fire } },
                { ElementType.Fire,    new[] { ElementType.Wood } },
                { ElementType.Wood,    new[] { ElementType.Earth } },
                { ElementType.Earth,   new[] { ElementType.Water } },
                { ElementType.Thunder, new[] { ElementType.Water } },
                { ElementType.Wind,    new[] { ElementType.Fire } },
                { ElementType.Metal,   new[] { ElementType.Wood } },
            };
    }
}
