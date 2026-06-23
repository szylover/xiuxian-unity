using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xiuxian.Systems.Procedural
{
    public static class ProceduralInterpolate
    {
        private static readonly Regex Placeholder = new Regex("\\{([^}]+)\\}", RegexOptions.Compiled);
        private static readonly Regex Multiply = new Regex("^(\\w+)\\*(\\d+(?:\\.\\d+)?)$", RegexOptions.Compiled);
        public static string Interpolate(string template, IDictionary<string, string> vars)
        {
            if (template == null) return null;
            return Placeholder.Replace(template, m =>
            {
                string expr = m.Groups[1].Value;
                var mul = Multiply.Match(expr);
                if (mul.Success)
                {
                    string key = mul.Groups[1].Value;
                    if (vars != null && vars.TryGetValue(key, out var value) && double.TryParse(value, out var num))
                        return Math.Floor(num * double.Parse(mul.Groups[2].Value)).ToString("0");
                    return vars != null && vars.TryGetValue(key, out var fallback) ? fallback : m.Value;
                }
                return vars != null && vars.TryGetValue(expr, out var found) ? found : m.Value;
            });
        }
    }
}
