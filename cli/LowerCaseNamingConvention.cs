using YamlDotNet.Serialization;

namespace OxceTools;

public class LowerCaseNamingConvention : INamingConvention
{
    public static readonly LowerCaseNamingConvention Instance = new LowerCaseNamingConvention();

    public string Apply(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = char.ToLower(value[0]) + value.Substring(1);
        }

        return value;
    }
}