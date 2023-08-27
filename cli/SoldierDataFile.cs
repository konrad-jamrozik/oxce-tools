using System.Text;

namespace OxceTools;

public class SoldierDataFile(string soldierDataCsvFilePath)
{
    public SoldierDataFile(Dirs dirs) : this(dirs.SoldierDataCsvFilePath)
    {

    }

    // kja curr work
    public void WriteFrom(SaveData data)
    {
        var stringBuilder = new StringBuilder();

        var bases = data.Bases;
        if (!bases.Any())
            return;

        List<(string baseName, Soldier soldier)> soldiers =
            bases
                .Where(@base => @base.Soldiers != null)
                .SelectMany(
                    @base => @base.Soldiers!.Select(
                        soldier => (@base.Name, soldier)))
                .ToList();

        if (!soldiers.Any())
            return;


        stringBuilder.AppendLine($"Base,{(soldiers[0].soldier as ICsvRecord).CsvHeader()}");
        foreach ((string baseName, Soldier soldier) soldier in soldiers)
            stringBuilder.AppendLine($"{soldier.baseName},{(soldier.soldier as ICsvRecord).CsvRow()}");

        Console.Out.WriteLine($"Writing out CSV soldier data to {soldierDataCsvFilePath}");
        File.WriteAllText(soldierDataCsvFilePath, stringBuilder.ToString());
    }
}