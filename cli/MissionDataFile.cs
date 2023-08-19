using System.Text;

namespace OxceTools;

public class MissionDataFile(Dirs dirs)
{
    public void WriteFrom(SaveData data)
    {
        var stringBuilder = new StringBuilder();

        var alienMissions = data.AlienMissions;
        if (!alienMissions.Any())
            return;


        stringBuilder.AppendLine((alienMissions[0] as ICsvFile).CsvHeader());
        foreach (AlienMission alienMission in alienMissions)
            stringBuilder.AppendLine((alienMission as ICsvFile).CsvRow());

        Console.Out.WriteLine($"Writing out CSV mission data to {dirs.MissionDataCsvFilePath}");
        File.WriteAllText(dirs.MissionDataCsvFilePath, stringBuilder.ToString());
    }

}