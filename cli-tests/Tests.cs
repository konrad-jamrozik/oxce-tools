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

    /// <summary>
    /// This test proves that a save file contents can be read, modified, and written out
    /// to another save file, and still work in OXCE as expected.
    ///
    /// This test doesn't guarantee the written out modified save file actually works.
    /// For that, load it up in OXCE and check if everything is order. After loading such
    /// modified save file you could also save it back to the file system and diff its contents
    /// to the baseline unmodified save file. Lack of modifications, except the indented ones
    /// or expected ones (like few seconds passing in-game), means the round-tripping indeed works.
    /// </summary>
    [Test]
    public void RoundTripsSaveFileWithModification()
    {
        var saveFile = new SaveFile(new Dirs());

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();
        ModifySaveFile(data);
        saveFile.Serialize(metadata, data);
        
        Assert.Pass();
    }

    /// <summary>
    /// Use this test as a tool that creates a .csv file with mission data that then
    /// you can edit manually. Notably, you may add "1" in the "Keep" column
    /// to denote the mission should be kept in the save file. Then you
    /// can apply this deletion by running "UpdateSaveFileFromMissionDataFileAndShuffle".
    /// If you want to verify the changes made to the missions to the modified save file,
    /// run "SaveModifiedSaveMissionDataToCsv" and manually inspect the output file.
    ///
    /// The test "UpdateSaveFileFromMissionDataFileAndShuffle" is going to shuffle the mission
    /// times, and reorder the missions by spawn time. You can always update the times by
    /// opening the .csv output by "SaveModifiedSaveMissionDataToCsv", making your changes,
    /// and then running "UpdateSaveFileFromModifiedMissionDataFile".
    ///
    /// To figure out the file paths being read/written to, observe stdout of the tests.
    /// </summary>
    [Test]
    public void ReadMissionDataFromSave()
    {
        var dirs = new Dirs();
        ReadMissionDataFromSave(dirs.SaveFilePath, dirs.MissionDataCsvFilePath);
    }

    /// <summary>
    /// See comment on "SaveMissionDataToCsv".
    /// </summary>
    [Test]
    public void UpdateSaveFileFromMissionDataFileAndShuffle()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();
        AlienMissions alienMissions = new MissionDataFile(dirs).Read();
        data.Update(alienMissions, shuffleInTime: true);
        saveFile.Serialize(metadata, data);
    }

    /// <summary>
    /// See comment on "SaveMissionDataToCsv".
    /// </summary>
    [Test]
    public void UpdateSaveFileFromModifiedMissionDataFile()
    {
        var dirs = new Dirs();
        var saveFile = new SaveFile(dirs);

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();
        AlienMissions alienMissions = new MissionDataFile(dirs.ModifiedMissionDataCsvFilePath).Read();
        data.Update(alienMissions, shuffleInTime: false);
        saveFile.Serialize(metadata, data);
    }


    /// <summary>
    /// See comment on "SaveMissionDataToCsv".
    /// </summary>
    [Test]
    public void ReadModifiedMissionDataFromModifiedSave()
    {
        var dirs = new Dirs();
        ReadMissionDataFromSave(dirs.ModifiedSaveFilePath, dirs.ModifiedMissionDataCsvFilePath);
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
            .Where(dict => dict.ContainsKey("keep") && (string)dict["keep"] == "1")
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

    private void ReadMissionDataFromSave(string saveFilePath, string missionDataCsvFilePath)
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