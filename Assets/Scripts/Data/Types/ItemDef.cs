// ============================================================
// ItemDef.cs — 物品定义 DTO（移植自 item-loader.ts JsonItem / types/items.ts）
// JSON 中的纯数据部分；运行时 effect 函数由 EffectValue.Resolve 提供
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiuxian.Data
{
    public sealed class ItemDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("category")] public string Category;
        [JsonProperty("rarity")] public string Rarity;
        [JsonProperty("description")] public string Description;
        [JsonProperty("stackable")] public bool Stackable;
        [JsonProperty("maxStack")] public int MaxStack;
        [JsonProperty("usable")] public bool Usable;

        /// <summary>数值效果（字段名 → 效果值），对应 JSON "effects"。可空。</summary>
        [JsonProperty("effects")] public Dictionary<string, EffectValue> Effects;

        [JsonProperty("effectMessage")] public string EffectMessage;
        [JsonProperty("sellPrice")] public int SellPrice;

        // 丹方/功法卷轴
        [JsonProperty("scrollType")] public string ScrollType;       // technique | divineArt | recipe | smithingRecipe
        [JsonProperty("scrollTargetId")] public string ScrollTargetId;
        [JsonProperty("scrollStudyMonths")] public int? ScrollStudyMonths;
        [JsonProperty("scrollMinRealm")] public int? ScrollMinRealm;
    }
}
