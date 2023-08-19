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
    public void RoundTripsSaveFileWithModification()
    {
        var saveFile = new SaveFile(new Dirs());

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        ModifySaveFile(data);

        saveFile.Serialize(metadata, data);
    }

    [Test]
    public void SavesMissionDataToCsv()
    {
        var dirs = new Dirs();
        SavesMissionDataToCsv(dirs.SaveFilePath, dirs.MissionDataCsvFilePath);
    }

    [Test]
    public void UpdatesSaveFileFromMissionDataFile()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();
        AlienMissions alienMissions = new MissionDataFile(dirs).Read();
        data.Update(alienMissions);
        saveFile.Serialize(metadata, data);
    }

    [Test]
    public void SavesModifiedSaveMissionDataToCsv()
    {
        var dirs = new Dirs();
        SavesMissionDataToCsv(dirs.ModifiedSaveFilePath, dirs.ModifiedMissionDataCsvFilePath);
    }

    [Test]
    public void SavesMissionScriptsDataToCsv()
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

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
    }

    private void SavesMissionDataToCsv(string saveFilePath, string missionDataCsvFilePath)
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(saveFilePath, dirs.ModifiedSaveFilePath);
        (SaveMetadata _, SaveData data) = saveFile.Deserialize();
        
        var missionDataFile = new MissionDataFile(missionDataCsvFilePath);

        // Act
        missionDataFile.WriteFrom(data);
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