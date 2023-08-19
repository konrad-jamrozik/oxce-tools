namespace OxceTools;

public interface ICsvFile
{
    public string CsvHeader()
    {
        var fieldNames = GetType()
            .GetFields()
            .Where(field => field.FieldType != typeof(object))
            .Select(field => field.Name);
        return string.Join(",", fieldNames);
    }

    public string CsvRow()
    {
        var fieldValues = GetType()
            .GetFields()
            .Where(field => field.FieldType != typeof(object))
            .Select(field =>
            {
                object? value = field.GetValue(this);
                var str = value switch
                {
                    Dictionary<string, bool> dict => string.Join("; ", dict.Select(kvp => $"{kvp.Key}: {kvp.Value}")),
                    List<int> list => string.Join("; ", list),
                    null => "",
                    _ => value.ToString()
                };
                return str;
            });
        return string.Join(",", fieldValues);
    }
}