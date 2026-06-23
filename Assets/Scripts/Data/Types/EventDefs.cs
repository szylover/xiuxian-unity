// ============================================================
// EventDefs.cs — 事件与程序化事件 DTO
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public sealed class GameEventDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("category")] public string Category;
        [JsonProperty("tone")] public string Tone;
        [JsonProperty("name")] public string Name;
        [JsonProperty("weight")] public int? Weight;
        [JsonProperty("effects")] public Dictionary<string, EffectValue> Effects;
        [JsonProperty("message")] public string Message;
        [JsonProperty("condition")] public JToken Condition;
        [JsonProperty("once")] public bool? Once;
        [JsonProperty("cooldown")] public int? Cooldown;
        [JsonProperty("regionTags")] public List<string> RegionTags;
    }

    public sealed class EventTemplateDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("category")] public string Category;
        [JsonProperty("tone")] public string Tone;
        [JsonProperty("namePattern")] public string NamePattern;
        [JsonProperty("messagePattern")] public string MessagePattern;
        [JsonProperty("effectsPattern")] public Dictionary<string, string> EffectsPattern;
        [JsonProperty("condition")] public JToken Condition;
        [JsonProperty("weight")] public int? Weight;
        [JsonProperty("variableSlots")] public List<string> VariableSlots;
        [JsonProperty("varConstraints")] public JToken VarConstraints;
        [JsonProperty("regionTags")] public List<string> RegionTags;
    }

    public sealed class VariablePoolDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("namespace")] public string Namespace;
        [JsonProperty("variable")] public string Variable;
        [JsonProperty("entries")] public List<JToken> Entries;
    }
}
