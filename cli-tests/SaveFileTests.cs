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
        var saveFile = new SaveFile(new SaveDir());

        (SaveMetadata metadata, SaveData data) = saveFile.Deserialize();

        ModifySaveFile(data);

        saveFile.Serialize(metadata, data);
    }

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
    }
}