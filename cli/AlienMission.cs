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
            AlienBase = AlienBaseField.FromString(row[8]),
            MissionSiteZone = Convert.ToInt32(row[9]),
            Keep = !string.IsNullOrWhiteSpace(row[10]) ? row[10] : null
        };
    }

    public class AlienBaseField
    {
        public required double Lon;
        public required double Lat;
        public required string Type;
        public required int Id;

        public override string ToString()
            => $"{nameof(Lon)}: {Lon} | " +
               $"{nameof(Lat)}: {Lat} | " +
               $"{nameof(Type)}: {Type} | " +
               $"{nameof(Id)}: {Id}";

        public static AlienBaseField? FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string[] props = str.Split("|");

            return new AlienBaseField
            {
                Lon = Convert.ToDouble(GetValue(props[0])),
                Lat = Convert.ToDouble(GetValue(props[1])),
                Type = GetValue(props[2]),
                Id = Convert.ToInt32(GetValue(props[3]))
            };

            string GetValue(string prop) => prop.Split(":")[1].Trim();
        }
    }
}
