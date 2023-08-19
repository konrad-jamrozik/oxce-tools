using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OxceToolsTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Proof-of-concept showing round-tripping save file after making modification to it.
    /// Deserialized to dictionary of objects.
    /// </summary>
    [Test]
    public void RoundTripsSaveFileWithoutTyping()
    {
        var stopwatch = Stopwatch.StartNew();
        var saveDir = new SaveDir();
        Console.Out.WriteLine($"Reading and deserializing save file from {saveDir.SaveFilePath}");
        
        string saveFileContents = File.ReadAllText(saveDir.SaveFilePath);
        
        // The save file has a header delimiter "---" that is not a valid yaml, hence we need to remove it (and later 
        // on add it back.
        // Actually:
        // https://stackoverflow.com/questions/50788277/why-3-dashes-hyphen-in-yaml-file
        // https://github.com/aaubry/YamlDotNet/wiki/Samples.DeserializingMultipleDocuments
        saveFileContents = saveFileContents.Replace("---", "");
        Console.Out.WriteLine("Time until read: " + stopwatch.Elapsed);

        Dictionary<object, object> dict = DeserializeSaveFileToDict(saveFileContents);

        Console.Out.WriteLine($"Time until yaml: {stopwatch.Elapsed}");

        dict["name"] += "_MODIFIED";

        List<object> alienMissions = (List<object>)dict["alienMissions"];

        foreach (Dictionary<object, object> alienMission in alienMissions)
        {
            Console.Out.WriteLine("Next alien mission");
            foreach (var keyValuePair in alienMission)
            {
                Console.Out.WriteLine("kvp: " + keyValuePair);
            }
        }

        Serialize(dict, saveDir);

        Console.Out.WriteLine($"Time until done: {stopwatch.Elapsed}");
        // Assert: the modified save file can be loaded
    }

    [Test]
    public void DeserializesSaveFileUsingTypeConverter()
    {
        var saveDir = new SaveDir();
        
        string saveFileContents = File.ReadAllText(saveDir.SaveFilePath);
        
        saveFileContents = saveFileContents.Replace("---", "");

        SaveFile saveFile = DeserializeSaveFileUsingTypeConverter(saveFileContents);

        Assert.Pass("No exception thrown");
    }

    /// <summary>
    /// Proof-of-concept of parsing of both of the yaml documents in save file, separated by ---.
    ///
    /// Based on:
    /// https://stackoverflow.com/questions/50788277/why-3-dashes-hyphen-in-yaml-file
    /// https://github.com/aaubry/YamlDotNet/wiki/Samples.DeserializingMultipleDocuments
    /// </summary>
    [Test]
    public void ParsesSaveFileDocuments()
    {
        var saveDir = new SaveDir();
        
        string saveFileContents = File.ReadAllText(saveDir.SaveFilePath);

        var input = new StringReader(saveFileContents);

        var deserializer = new DeserializerBuilder().Build();

        var parser = new Parser(input);

        // Consume the stream start event "manually"
        parser.Consume<StreamStart>();

        parser.Consume<DocumentStart>();
        Dictionary<object, object> saveMetadata = deserializer.Deserialize<Dictionary<object, object>>(parser);
        
        Console.Out.WriteLine("");
        Console.Out.WriteLine("## Save metadata");
        Console.Out.WriteLine("");

        foreach (var key in saveMetadata.Keys)
        {
            Console.Out.WriteLine(key);
        }

        parser.Consume<DocumentEnd>();
        parser.Consume<DocumentStart>();
        var saveData = deserializer.Deserialize<Dictionary<object, object>>(parser);
        parser.Consume<DocumentEnd>();

        Console.Out.WriteLine("");
        Console.Out.WriteLine("## Save data");
        Console.Out.WriteLine("");

        foreach (var key in saveData.Keys)
        {
            Console.Out.WriteLine(key);
        }

        parser.Consume<StreamEnd>();
        Assert.That(parser.Current, Is.Null);
    }

    private static Dictionary<object, object> DeserializeSaveFileToDict(string saveFileContents)
    {
        var yamlDeserializer = new DeserializerBuilder().Build();
        Dictionary<object, object> dict = (Dictionary<object, object>)yamlDeserializer.Deserialize(saveFileContents)!;
        return dict;
    }

    private static SaveFile DeserializeSaveFileUsingTypeConverter(string saveFileContents)
    {
        var yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            // kja experimental
            .WithTypeConverter(new SaveFileTypeConverter())
            .Build();
        var saveFile = yamlDeserializer.Deserialize<SaveFile>(saveFileContents);
        return saveFile;
    }

    private static void Serialize(Dictionary<object, object> dict, SaveDir saveDir)
    {
        var serializer = new SerializerBuilder().Build();
        string modifiedSaveFileContents = serializer
            .Serialize(dict)
            .Replace("difficulty", "---" + Environment.NewLine + "difficulty");
        

        Console.Out.WriteLine($"Writing out modified contents to {saveDir.ModifiedSaveFilePath}");
        File.WriteAllText(saveDir.ModifiedSaveFilePath, modifiedSaveFileContents);
    }
}
