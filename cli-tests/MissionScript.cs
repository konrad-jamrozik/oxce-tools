using OxceTools;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OxceToolsTests;

// ReSharper disable once ClassNeverInstantiated.Global
public class MissionScript : ICsvFile
{
    private static readonly Serializer Serializer = new Serializer();

    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    public static MissionScript From(IDictionary<object, object> dict)
    {
        string serialized = Serializer.Serialize(dict);
        var missionScript = Deserializer.Deserialize<MissionScript>(serialized);
        
        if (missionScript.LastMonth == 0)
            missionScript.LastMonth = 50;
        
        if (missionScript.ExecutionOdds == 0)
            missionScript.ExecutionOdds = 100;

        return missionScript;
    }

    public required string Type;
    public required int FirstMonth;
    public required int LastMonth;
    public required int ExecutionOdds;
    public required object MissionWeights;
    public required object RegionWeights;
    public required List<int> Conditionals;
    public required Dictionary<string, bool> ResearchTriggers;
    public required object MaxRuns;
    public required object Label;
    public required object VarName;
    public required object RaceWeights;
    public required object StartDelay;
    public required object RandomDelay;
    public required object TargetBaseOdds;
    public required object UseTable;
}