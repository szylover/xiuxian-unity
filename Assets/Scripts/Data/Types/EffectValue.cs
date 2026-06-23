// ============================================================
// EffectValue.cs — 效果值联合类型（移植自 event-loader.ts 的 EffectValue）
// 形态：number | [number,number] | string("max" | "=N" | "*N")
// UnityEngine-free
// ============================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiuxian.Data
{
    public enum EffectValueKind { Scalar, Range, Expr }

    /// <summary>
    /// 数值效果规格。对应 TS: number | [number, number] | string。
    /// 字符串支持 "max"、"=N"（设为 N）、"*N"（乘以 N）。
    /// </summary>
    [JsonConverter(typeof(EffectValueConverter))]
    public struct EffectValue
    {
        public EffectValueKind Kind;
        public double Scalar;      // Kind == Scalar
        public double RangeMin;    // Kind == Range
        public double RangeMax;    // Kind == Range
        public string Expr;        // Kind == Expr

        public static EffectValue FromScalar(double v) =>
            new EffectValue { Kind = EffectValueKind.Scalar, Scalar = v };

        public static EffectValue FromRange(double min, double max) =>
            new EffectValue { Kind = EffectValueKind.Range, RangeMin = min, RangeMax = max };

        public static EffectValue FromExpr(string expr) =>
            new EffectValue { Kind = EffectValueKind.Expr, Expr = expr };

        /// <summary>
        /// 解析为新数值（对应 item-loader.ts resolveValue）。
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="maxValue">上限（可空，用于 "max"）</param>
        /// <param name="rng">随机数源（用于区间）</param>
        public double Resolve(double current, double? maxValue, Random rng)
        {
            switch (Kind)
            {
                case EffectValueKind.Scalar:
                    return current + Scalar;
                case EffectValueKind.Range:
                    int lo = (int)RangeMin, hi = (int)RangeMax;
                    return current + rng.Next(lo, hi + 1);
                case EffectValueKind.Expr:
                    if (Expr == "max") return maxValue ?? current;
                    if (Expr != null && Expr.StartsWith("=")) return double.Parse(Expr.Substring(1));
                    if (Expr != null && Expr.StartsWith("*")) return Math.Floor(current * double.Parse(Expr.Substring(1)));
                    return current;
                default:
                    return current;
            }
        }
    }

    public sealed class EffectValueConverter : JsonConverter<EffectValue>
    {
        public override EffectValue ReadJson(JsonReader reader, Type objectType, EffectValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    return EffectValue.FromScalar(token.Value<double>());
                case JTokenType.String:
                    return EffectValue.FromExpr(token.Value<string>());
                case JTokenType.Array:
                    var arr = (JArray)token;
                    return EffectValue.FromRange(arr[0].Value<double>(), arr[1].Value<double>());
                default:
                    throw new JsonSerializationException($"Unsupported EffectValue token: {token.Type}");
            }
        }

        public override void WriteJson(JsonWriter writer, EffectValue value, JsonSerializer serializer)
        {
            switch (value.Kind)
            {
                case EffectValueKind.Scalar: writer.WriteValue(value.Scalar); break;
                case EffectValueKind.Range:
                    writer.WriteStartArray();
                    writer.WriteValue(value.RangeMin);
                    writer.WriteValue(value.RangeMax);
                    writer.WriteEndArray();
                    break;
                case EffectValueKind.Expr: writer.WriteValue(value.Expr); break;
            }
        }
    }
}
