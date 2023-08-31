namespace OxceTools;

public class AlienMission : ICsvRecord
{
    public required string Type;
    public required string Region;
    public required string Race;
    public required int NextWave;
    public required int NextUfoCounter;
    public required int SpawnCountdown;
    public required int LiveUfos;
    public required int UniqueID;
    public required AlienBaseField? AlienBase;
    public required int MissionSiteZone;
    public string? Keep;
    
    public static AlienMission FromCsvRow(string[] row)
    {
        return new AlienMission
        {
            Type = row[0],
            Region = row[1],
            Race = row[2],
            NextWave = Convert.ToInt32(row[3]),
            NextUfoCounter = Convert.ToInt32(row[4]),
            SpawnCountdown = Convert.ToInt32(row[5]),
            LiveUfos = Convert.ToInt32(row[6]),
            UniqueID = Convert.ToInt32(row[7]),
            AlienBase = null,
            MissionSiteZone = Convert.ToInt32(row[8]),
            Keep = !string.IsNullOrWhiteSpace(row[9]) ? row[9] : null
        };
    }

    public class AlienBaseField
    {
        public required float Lon;
        public required float Lat;
        public required string Type;
        public required int Id;
    }
}

