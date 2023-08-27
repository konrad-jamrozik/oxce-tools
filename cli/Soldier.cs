namespace OxceTools;

public class Soldier : ICsvRecord
{
    public required string Type;
    public required int Id;
    public required string Name;
    public required int Nationality;
    public required object InitialStats;
    public required object CurrentStats;
    public required int Rank;
    public required object Craft;
    public required int Gender;
    public required int Look;
    public required int LookVariant;
    public required int Missions;
    public required int Kills;
    public required int Stuns;
    public required double Recovery;
    public required int ManaMissing;
    public required string Armor;
    public required bool ReturnToTrainingWhenHealed;
    public required bool Training;
    public required int Improvement;
    public required int PsiStrImprovement;
    public required object EquipmentLayout;
    public required object PersonalEquipmentLayout;
    public required string PersonalEquipmentArmor;
    public required object Diary;
    public required object PreviousTransformations;
    public required object TransformationBonuses;

    public static Soldier FromCsvRow(string[] row)
    {
        return new Soldier
        {
            Type = row[0],
            Id = Convert.ToInt32(row[1]),
            Name = row[2],
            Nationality = Convert.ToInt32(row[3]),
            InitialStats = "",
            CurrentStats = "",
            Rank = Convert.ToInt32(row[6]),
            Craft = "",
            Gender = Convert.ToInt32(row[8]),
            Look = Convert.ToInt32(row[9]),
            LookVariant = Convert.ToInt32(row[10]),
            Missions = Convert.ToInt32(row[11]),
            Kills = Convert.ToInt32(row[12]),
            Stuns = Convert.ToInt32(row[13]),
            Recovery = Convert.ToDouble(row[14]),
            ManaMissing = Convert.ToInt32(row[15]), 
            Armor = row[16],
            ReturnToTrainingWhenHealed = Convert.ToBoolean(row[17]),
            Training = Convert.ToBoolean(row[18]),
            Improvement = Convert.ToInt32(row[19]),
            PsiStrImprovement = Convert.ToInt32(row[20]),
            EquipmentLayout = "",
            PersonalEquipmentLayout = "",
            PersonalEquipmentArmor = "",
            Diary = "",
            PreviousTransformations = "",
            TransformationBonuses = ""
        };
    }
}