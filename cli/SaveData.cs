namespace OxceTools;

// ReSharper disable once ClassNeverInstantiated.Global
public class SaveData
{
    public required object Difficulty;
    public required object End;
    public required object MonthsPassed;
    public required object GraphRegionToggles;
    public required object GraphCountryToggles;
    public required object GraphFinanceToggles;
    public required object Rng;
    public required object Funds;
    public required object Maintenance;
    public required object UserNotes;
    public required object GeoscapeDebugLog;
    public required object ResearchScores;
    public required object Incomes;
    public required object Expenditures;
    public required object Warned;
    public required object TogglePersonalLight;
    public required object ToggleNightVision;
    public required object ToggleBrightness;
    public required object GlobeLon;
    public required object GlobeLat;
    public required object GlobeZoom;
    // ReSharper disable once UnassignedField.Global
    public required Dictionary<string, int> Ids;
    public required object Countries;
    public required object Regions;
    public required object Bases;
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once UnassignedField.Global
    public required AlienMissions AlienMissions;
    public required object GeoscapeEvents;
    public required object GeneratedEvents;
    public required object UfopediaRuleStatus;
    public required object ManufactureRuleStatus;
    public required object ResearchRuleStatus;
    public required object MonthlyPurchaseLimitLog;
    public required object HiddenPurchaseItems;
    public required object CustomRuleCraftDeployments;
    public required object AlienStrategy;
    public required object Options;

    public void Update(AlienMissions alienMissions)
    {
        int alienMissionsCount = alienMissions.Count;

        List<AlienMission> missionsToInclude =
            alienMissions.Where(mission => string.IsNullOrWhiteSpace(mission.Delete)).ToList();

        int deletedMissionsCount = alienMissionsCount - missionsToInclude.Count;

        Ids["ALIEN_MISSIONS"] -= deletedMissionsCount;

        int currentMinId = alienMissions.Min(mission => mission.UniqueID);
        int missionIdIterator = currentMinId;

        // kja bucket missions by time, then redo the IDs in timeline order

        missionsToInclude.OrderBy(mission => mission.UniqueID).ToList()
            .ForEach(mission => mission.UniqueID = missionIdIterator++);

        AlienMissions = new AlienMissions(missionsToInclude);
    }

}