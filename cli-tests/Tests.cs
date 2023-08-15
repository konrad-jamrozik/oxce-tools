using System.Diagnostics;
using YamlDotNet.Serialization;

namespace OxceToolsTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Proof-of-concept showing round-tripping save file after making modification to it.
    /// </summary>
    [Test]
    public void TestModifySaveFile()
    {
        var stopwatch = Stopwatch.StartNew();
        var saveDir = new SaveDir();
        Console.Out.WriteLine($"Reading and deserializing save file from {saveDir.SaveFilePath}");
        
        string saveFileContents = File.ReadAllText(saveDir.SaveFilePath);
        
        // The save file has a header delimiter "---" that is not a valid yaml, hence we need to remove it (and later 
        // on add it back.
        saveFileContents = saveFileContents.Replace("---", "");
        Console.Out.WriteLine("Time until read: " + stopwatch.Elapsed);

        Dictionary<object, object> yaml = Deserialize(saveFileContents);

        Console.Out.WriteLine($"Time until yaml: {stopwatch.Elapsed}");

        yaml["name"] += "_MODIFIED";

        List<object> alienMissions = (List<object>)yaml["alienMissions"]!;

        foreach (Dictionary<object, object> alienMission in alienMissions)
        {
            Console.Out.WriteLine("Next alien mission");
            foreach (var keyValuePair in alienMission)
            {
                Console.Out.WriteLine("kvp: " + keyValuePair);
            }
        }

        Serialize(yaml, saveDir);

        Console.Out.WriteLine($"Time until done: {stopwatch.Elapsed}");
        // Assert: the modified save file can be loaded
    }

    private static Dictionary<object, object> Deserialize(string saveFileContents)
    {
        var yamlDeserializer = new DeserializerBuilder().Build();
        Dictionary<object, object> yaml = (Dictionary<object, object>)yamlDeserializer.Deserialize(saveFileContents)!;
        return yaml;
    }

    private static void Serialize(Dictionary<object, object> yaml, SaveDir saveDir)
    {
        var serializer = new SerializerBuilder().Build();
        string modifiedSaveFileContents = serializer
            .Serialize(yaml)
            .Replace("difficulty", "---" + Environment.NewLine + "difficulty");


        Console.Out.WriteLine($"Writing out modified contents to {saveDir.ModifiedSaveFilePath}");
        File.WriteAllText(saveDir.ModifiedSaveFilePath, modifiedSaveFileContents);
    }
}

