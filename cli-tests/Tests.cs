using OxceTools;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LowerCaseNamingConvention = OxceTools.LowerCaseNamingConvention;

namespace OxceToolsTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Proof-of-concept of round-trip deserialization-modification-serialization of a save file,
    /// where the save file contents is recognized to be two yaml documents and appropriately
    /// deserialized into two object hierarchies: metadata and data.
    ///
    /// Reference:
    /// https://stackoverflow.com/questions/50788277/why-3-dashes-hyphen-in-yaml-file
    /// https://github.com/aaubry/YamlDotNet/wiki/Samples.DeserializingMultipleDocuments
    /// </summary>
    [Test]
    public void RoundTripsSaveFileUsingParserAndObjects()
    {
        var saveDir = new SaveDir();
        
        string saveFileContents = File.ReadAllText(saveDir.SaveFilePath);

        (SaveMetadata metadata, SaveData data) = DeserializeSaveFile(saveFileContents);

        ModifySaveFile(data);

        SerializeToSaveFile(metadata, data, saveDir);
    }

    private static void ModifySaveFile(SaveData data)
    {
        foreach (var alienMission in data.AlienMissions)
        {
            alienMission.SpawnCountdown = 60;
        }
    }

    private static (SaveMetadata metadata, SaveData data) DeserializeSaveFile(string saveFileContents)
    {
        var saveFileReader = new StringReader(saveFileContents);

        var deserializer =
            new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        var parser = new Parser(saveFileReader);

        parser.Consume<StreamStart>();
        parser.Consume<DocumentStart>();

        SaveMetadata metadata = deserializer.Deserialize<SaveMetadata>(parser);

        metadata.Name += "_MODIFIED";

        parser.Consume<DocumentEnd>();
        parser.Consume<DocumentStart>();

        SaveData data = deserializer.Deserialize<SaveData>(parser);

        parser.Consume<DocumentEnd>();
        parser.Consume<StreamEnd>();

        Assert.That(parser.Current, Is.Null);
        return (metadata, data);
    }

    private static void SerializeToSaveFile(
        SaveMetadata metadataObj,
        SaveData dataObj,
        SaveDir saveDir)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .Build();
        string metadata = serializer.Serialize(metadataObj);
        string data = serializer.Serialize(dataObj);

        string modifiedSaveFileContents = metadata + "---" + Environment.NewLine + data;

        Console.Out.WriteLine($"Writing out modified contents to {saveDir.ModifiedSaveFilePath}");
        File.WriteAllText(saveDir.ModifiedSaveFilePath, modifiedSaveFileContents);
    }
}