using System.Text;
using System.Text.Json;

namespace SIMTernakAyam.Infrastructure
{
    public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {

        public override string ConvertName(string name)
        {

            if(string.IsNullOrEmpty(name))
                return name;


            var sb = new StringBuilder(name.Length + 10);
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    // tambahkan '_' di batas kata (aA atau AAa)
                    if (i > 0 && name[i - 1] != '_' &&
                        (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                    {
                        sb.Append('_');
                    }
                    sb.Append(char.ToLowerInvariant(c));
                }
                else if (c == '-') // konversi kebab-case ke snake-case
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

    }
}
