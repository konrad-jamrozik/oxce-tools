using System.Text;
using OxceTools;

namespace OxceToolsTests;

public class SaveFileTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void RoundTripsSaveFileModification()
    {
        var saveFile = new SaveFile(new Dirs());

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        ModifySaveFile(data);

        saveFile.Serialize(metadata, data);
    }

    [Test]
    public void ExtractsCsvData()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        SaveToCsv(dirs, data);

        saveFile.Serialize(metadata, data);
    }

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
    }

    private void SaveToCsv(Dirs dirs, SaveData data)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(AlienMission.CsvHeader());
        foreach (AlienMission alienMission in data.AlienMissions)
            stringBuilder.AppendLine(alienMission.CsvRow());

        Console.Out.WriteLine($"Writing out csv data to {dirs.OutputCsvFilePath}");
        File.WriteAllText(dirs.OutputCsvFilePath, stringBuilder.ToString());
    }
}