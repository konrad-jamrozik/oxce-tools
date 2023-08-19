namespace OxceTools;

// ReSharper disable once ClassNeverInstantiated.Global
public class AlienMission
{
    public required string Type;
    public required string Region;
    public required string Race;
    public required int NextWave;
    public required int NextUfoCounter;
    public required int SpawnCountdown;
    public required int LiveUfos;
    public required int UniqueID;
    public required int MissionSiteZone;

    private static readonly AlienMission EmptyAlienMission = new AlienMission
    {
        Type = "",
        Region = "",
        Race = "",
        NextWave = 0,
        NextUfoCounter = 0,
        SpawnCountdown = 0,
        LiveUfos = 0,
        UniqueID = 0,
        MissionSiteZone = 0
    };

    public static string CsvHeader()
    {
        var fieldNames = EmptyAlienMission.GetType()
            .GetFields()
            .Select(field => field.Name);
        return string.Join(",", fieldNames);
    }

    public string CsvRow()
    {
        var fieldValues = GetType()
            .GetFields()
            .Select(field => field.GetValue(this)?.ToString());
        return string.Join(",", fieldValues);
    }
}