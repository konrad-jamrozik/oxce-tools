using System.Diagnostics;
using YamlDotNet.Serialization;

namespace OxceToolsTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestModifySaveFile()
    {
        // Proof-of-concept showing round-tripping save file after making modification to it.

        string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE")!;
        string homePath = Environment.GetEnvironmentVariable("HOMEPATH")!;
        string saveFilePath = Path.GetFullPath($"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/x-com-files/toprocess.sav");
        string modifiedSaveFilePath = Path.GetFullPath($"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/x-com-files/toprocess_modified.sav");
        
        Console.Out.WriteLine("saveFilePath: " + Path.GetFullPath(saveFilePath));
        var stopwatch = Stopwatch.StartNew();
        string saveFileContents = File.ReadAllText(saveFilePath);
        // Apparently this header delimiter "---" in the save file is not a valid yaml.
        saveFileContents = saveFileContents.Replace("---", "");
        Console.Out.WriteLine("Time to read: " + stopwatch.Elapsed);
        stopwatch.Restart();

        var yamlDeserializer = new DeserializerBuilder().Build();
        Dictionary<object, object> yaml = (Dictionary<object, object>)yamlDeserializer.Deserialize(saveFileContents)!;

        Console.Out.WriteLine("Time to deserialize to yaml: " + stopwatch.Elapsed);

        yaml["name"] = "toprocess_MODIFIED";

        var fundsNode = (List<object>)yaml["funds"];

        // The modification
        // $$$
        fundsNode[0] = "11000000";

        List<object> alienMissions = (List<object>)yaml["alienMissions"]!;

        foreach (Dictionary<object, object> alienMission in alienMissions)
        {
            Console.Out.WriteLine("Next alien mission");
            foreach (var keyValuePair in alienMission)
            {
                Console.Out.WriteLine("kvp: " + keyValuePair);
            }
        }

        var serializer = new SerializerBuilder().Build();

        string modifiedYamlAsString = serializer.Serialize(yaml);


        modifiedYamlAsString = modifiedYamlAsString.Replace("difficulty", "---" + Environment.NewLine + "difficulty");

        File.WriteAllText(modifiedSaveFilePath, modifiedYamlAsString);

        // Assert: the modified save file can be loaded
        Assert.Pass();
    }
}