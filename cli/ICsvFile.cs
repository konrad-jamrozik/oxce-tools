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
            .Select(field => field.GetValue(this)?.ToString());
        return string.Join(",", fieldValues);
    }
}