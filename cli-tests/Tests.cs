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
        (SaveMetadata _, SaveData data) = saveFile.Deserialize();
        
        var missionDataFile = new MissionDataFile(dirs);

        // Act
        missionDataFile.WriteFrom(data);
    }

    [Test]
    public void UpdatesSaveFileFromMissionDataFile()
    {
        // kja todo
        var saveFile = new SaveFile(new Dirs());

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        ModifySaveFile(data);

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

        AlienMissions alienMissions = LoadMissionDataFromCsv(dirs);
        int alienMissionsCount = alienMissions.Count;

        List<AlienMission> missionsToLoad = alienMissions.Where(mission => string.IsNullOrWhiteSpace(mission.Delete)).ToList();

        int deletedMissionsCount = alienMissionsCount - missionsToLoad.Count;

        data.Ids["ALIEN_MISSIONS"] -= deletedMissionsCount;

        int currentMinId = missionsToLoad.Min(mission => mission.UniqueID);
        int missionIdIterator = currentMinId;

        // kja bucket missions by time, then redo the IDs in timeline order

        missionsToLoad.OrderBy(mission => mission.UniqueID).ToList()
            .ForEach(mission => mission.UniqueID = missionIdIterator++);

        data.AlienMissions = new AlienMissions(missionsToLoad);
        
        saveFile.Serialize(metadata, data);
    }

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
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