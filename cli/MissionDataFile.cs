using System.Text;

namespace OxceTools;

public class MissionDataFile(string missionDataCsvFilePath)
{
    public MissionDataFile(Dirs dirs) : this(dirs.MissionDataCsvFilePath)
    {

    }

    public void WriteFrom(SaveData data)
    {
        var stringBuilder = new StringBuilder();

        var alienMissions = data.AlienMissions;
        if (!alienMissions.Any())
            return;

        stringBuilder.AppendLine((alienMissions[0] as ICsvFile).CsvHeader());
        foreach (AlienMission alienMission in alienMissions)
            stringBuilder.AppendLine((alienMission as ICsvFile).CsvRow());

        Console.Out.WriteLine($"Writing out CSV mission data to {missionDataCsvFilePath}");
        File.WriteAllText(missionDataCsvFilePath, stringBuilder.ToString());
    }

    public AlienMissions Read()
    {
        var lines = File.ReadAllLines(missionDataCsvFilePath)
            // Skip the CSV header
            .Skip(1);
        IEnumerable<AlienMission> alienMissions = lines.Select(
            line => AlienMission.FromCsvRow(line.Split(",")));
        return new AlienMissions(alienMissions);
    }

}