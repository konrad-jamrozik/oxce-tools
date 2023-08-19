using System.Text;
using OxceTools;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace OxceToolsTests;

public class Tests
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
    public void SaveMissionDataToCsv()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        SaveMissionDataToCsv(dirs, data);

        saveFile.Serialize(metadata, data);
    }


    [Test]
    public void SaveMissionScriptsDataToCsv()
    {
        var dirs = new Dirs();

        string missionScriptsContents = File.ReadAllText(dirs.MissionScriptsPath);

        var deserializer =
            new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        Dictionary<object, object> rootDict = (Dictionary<object, object>) deserializer.Deserialize(missionScriptsContents)!;
        List<object> objectMissionScripts = (List<object>)rootDict["missionScripts"];
        List<Dictionary<object, object>> dictMissionScripts = objectMissionScripts.Cast<Dictionary<object, object>>().ToList();
        List<MissionScript> missionScripts = dictMissionScripts
            .Where(dict => !dict.ContainsKey("delete"))
            .Select(MissionScript.From)
            .ToList();

        SaveMissionScriptsDataToCsv(dirs, missionScripts);
        Assert.Pass();
    }

    [Test]
    public void UpdateSaveFromMissionData()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        var alienMissions = LoadMissionDataFromCsv(dirs);
        int alienMissionsCount = alienMissions.Count;

        var missionsToDelete = alienMissions.Where(mission => mission.Delete == "1").ToList();

        var remainingMissions = alienMissions.Where(
            mission => missionsToDelete.All(missionToDel => missionToDel.UniqueID != mission.UniqueID)).ToList();

        int deletedMissionsCount = alienMissionsCount - remainingMissions.Count();

        if (deletedMissionsCount != missionsToDelete.Count)
            Console.Out.WriteLine(
                $"Something went wrong with deletion. missionsToDelete: {missionsToDelete.Count}, deletedMissions: {deletedMissionsCount}");

        data.Ids["ALIEN_MISSIONS"] -= deletedMissionsCount;

        int currentMinId = remainingMissions.Min(mission => mission.UniqueID);
        int missionIdIterator = currentMinId;

        remainingMissions.OrderBy(mission => mission.UniqueID).ToList()
            .ForEach(mission => mission.UniqueID = missionIdIterator++);

        data.AlienMissions = new AlienMissions(remainingMissions);
        
        saveFile.Serialize(metadata, data);
    }

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
    }

    private void SaveMissionDataToCsv(Dirs dirs, SaveData data)
    {
        var stringBuilder = new StringBuilder();

        var alienMissions = data.AlienMissions;
        if (!alienMissions.Any())
            return;


        stringBuilder.AppendLine((alienMissions[0] as ICsvFile).CsvHeader());
        foreach (AlienMission alienMission in alienMissions)
            stringBuilder.AppendLine((alienMission as ICsvFile).CsvRow());

        Console.Out.WriteLine($"Writing out csv data to {dirs.MissionDataCsvFilePath}");
        File.WriteAllText(dirs.MissionDataCsvFilePath, stringBuilder.ToString());
    }

    private AlienMissions LoadMissionDataFromCsv(Dirs dirs)
    {
        var lines = File.ReadAllLines(dirs.MissionDataCsvFilePath)
            // Skip the CSV header
            .Skip(1);
        IEnumerable<AlienMission> alienMissions = lines.Select(
            line => AlienMission.FromCsvRow(line.Split(",")));
        return new AlienMissions(alienMissions);
    }

    private void SaveMissionScriptsDataToCsv(Dirs dirs, List<MissionScript> missionScripts)
    {
        var stringBuilder = new StringBuilder();

        if (!missionScripts.Any())
            return;


        stringBuilder.AppendLine((missionScripts[0] as ICsvFile).CsvHeader());
        foreach (MissionScript missionScript in missionScripts)
            stringBuilder.AppendLine((missionScript as ICsvFile).CsvRow());

        Console.Out.WriteLine($"Writing out csv data to {dirs.MissionScriptsDataCsvFilePath}");
        File.WriteAllText(dirs.MissionScriptsDataCsvFilePath, stringBuilder.ToString());
    }
}