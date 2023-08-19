using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OxceToolsTests;

public class SaveFile
{
    // kja read on https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors#create-mutable-state
    public string? Name { get; set; }
}

public class SaveFileTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return (type == typeof(SaveFile));
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        parser.Consume<MappingStart>();
        var name = parser.Consume<Scalar>().Value;
        while (parser.Current!.GetType() != typeof(DocumentEnd))
            parser.MoveNext();
        Console.Out.WriteLine("Name parsed: " + name);
        return new SaveFile { Name = name };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        return;
    }
}


// kja implement SaveFileNodeDeserializer
// https://github.com/aaubry/YamlDotNet/wiki/Serialization.Deserializer#withnodedeserializer
// to support nested deserializers.
public class SaveMetadataNodeDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        return false;
    }
}

