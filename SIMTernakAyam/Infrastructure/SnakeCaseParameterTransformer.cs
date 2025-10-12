using System.Text.RegularExpressions;

namespace SIMTernakAyam.Infrastructure
{
    public class SnakeCaseParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;
            // Ubah ke string
            var str = value.ToString();
            if (string.IsNullOrEmpty(str)) return str;
            // Gunakan Regex untuk mengubah CamelCase atau PascalCase ke snake_case
            var snakeCase = Regex.Replace(str, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
            return snakeCase;
        }
    }
}
