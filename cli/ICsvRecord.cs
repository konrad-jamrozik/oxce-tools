namespace OxceTools;

public interface ICsvRecord
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
            // Ignore fields of type object because if a field is of type object it means I didn't
            // yet implemented parsing it; instead, I am just dumping to whatever is in the .yml file
            // to this field.
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