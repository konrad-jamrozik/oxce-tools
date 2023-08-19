using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OxceTools;

public class SaveFile(string saveFilePath, string modifiedSaveFilePath)
{
    public SaveFile(Dirs dirs) : this(dirs.SaveFilePath, dirs.ModifiedSaveFilePath)
    {

    }

    /// <summary>
    /// Implementation based on:
    /// https://stackoverflow.com/questions/50788277/why-3-dashes-hyphen-in-yaml-file
    /// https://github.com/aaubry/YamlDotNet/wiki/Samples.DeserializingMultipleDocuments
    /// </summary>
    public (SaveMetadata metadata, SaveData data) Deserialize()
    {
        Console.Out.WriteLine($"Reading save file path {saveFilePath}");

        string saveFileContents = File.ReadAllText(saveFilePath);

        var saveFileReader = new StringReader(saveFileContents);

        var deserializer =
            new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        var parser = new Parser(saveFileReader);

        parser.Consume<StreamStart>();
        parser.Consume<DocumentStart>();

        SaveMetadata metadata = deserializer.Deserialize<SaveMetadata>(parser);

        metadata.Name = "toprocess_MODIFIED";

        parser.Consume<DocumentEnd>();
        parser.Consume<DocumentStart>();

        SaveData data = deserializer.Deserialize<SaveData>(parser);

        parser.Consume<DocumentEnd>();
        parser.Consume<StreamEnd>();

        Debug.Assert(parser.Current == null);
        return (metadata, data);
    }

    /// <summary>
    /// Implementation based on:
    /// https://github.com/aaubry/YamlDotNet/wiki/Samples.SerializeObjectGraph
    /// </summary>
    public void Serialize(
        SaveMetadata metadataObj,
        SaveData dataObj)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .Build();
        string metadata = serializer.Serialize(metadataObj);
        string data = serializer.Serialize(dataObj);

        string modifiedSaveFileContents = metadata + "---" + Environment.NewLine + data;

        Console.Out.WriteLine($"Writing out save contents to {modifiedSaveFilePath}");
        File.WriteAllText(modifiedSaveFilePath, modifiedSaveFileContents);
    }
}