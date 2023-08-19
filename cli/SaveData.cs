using System.Diagnostics;
using MoreLinq;

namespace OxceTools;

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
    public required object MissionSites;
    public required AlienMissions AlienMissions;
    public required object GeoscapeEvents;
    public required object Discovered;
    public required object PoppedResearch;
    public required object GeneratedEvents;
    public required object UfopediaRuleStatus;
    public required object ManufactureRuleStatus;
    public required object ResearchRuleStatus;
    public required object MonthlyPurchaseLimitLog;
    public required object HiddenPurchaseItems;
    public required object CustomRuleCraftDeployments;
    public required object AlienStrategy;
    public required object DeadSoldiers;
    public required object GlobalEquipmentLayout0;
    public required object GlobalEquipmentLayoutName0;
    public required object GlobalEquipmentLayout1;
    public required object GlobalEquipmentLayoutName1;
    public required object GlobalEquipmentLayout2;
    public required object GlobalEquipmentLayoutName2;
    public required object GlobalEquipmentLayout3;
    public required object GlobalEquipmentLayoutName3;
    public required object GlobalEquipmentLayout4;
    public required object GlobalEquipmentLayoutName4;
    public required object GlobalEquipmentLayout5;
    public required object GlobalEquipmentLayoutName5;
    public required object GlobalEquipmentLayout6;
    public required object GlobalEquipmentLayoutName6;
    public required object GlobalEquipmentLayout7;
    public required object GlobalEquipmentLayoutName7;
    public required object GlobalEquipmentLayout8;
    public required object GlobalEquipmentLayoutName8;
    public required object GlobalEquipmentLayout9;
    public required object GlobalEquipmentLayoutName9;
    public required object GlobalEquipmentLayout10;
    public required object GlobalEquipmentLayoutName10;
    public required object GlobalCraftLoadout0;
    public required object GlobalCraftLoadoutName0;
    public required object GlobalCraftLoadout1;
    public required object GlobalCraftLoadoutName1;
    public required object GlobalCraftLoadout2;
    public required object GlobalCraftLoadoutName2;
    public required object GlobalCraftLoadout3;
    public required object GlobalCraftLoadoutName3;
    public required object GlobalCraftLoadout4;
    public required object GlobalCraftLoadoutName4;
    public required object GlobalCraftLoadout5;
    public required object GlobalCraftLoadoutName5;
    public required object GlobalCraftLoadout6;
    public required object GlobalCraftLoadoutName6;
    public required object GlobalCraftLoadout7;
    public required object GlobalCraftLoadoutName7;
    public required object GlobalCraftLoadout8;
    public required object GlobalCraftLoadoutName8;
    public required object GlobalCraftLoadout9;
    public required object GlobalCraftLoadoutName9;
    public required object GlobalCraftLoadout10;
    public required object GlobalCraftLoadoutName10;
    public required object MissionStatistics;
    public required object Options;

    public void Update(AlienMissions alienMissions, bool shuffleInTime = true)
    {
        List<AlienMission> missionsToInclude =
            alienMissions.Where(mission => string.IsNullOrWhiteSpace(mission.Delete)).ToList();

        if (shuffleInTime)
            ShuffleInTime(missionsToInclude);

        missionsToInclude = UpdateMissionIds(missionsToInclude, alienMissions);

        AlienMissions = new AlienMissions(missionsToInclude);
    }

    private static void ShuffleInTime(List<AlienMission> missionsToInclude)
    {
        int missionsCount = missionsToInclude.Count;
        
        // Most entries in missionScripts_XCOMFILES.rul have randomDelay: 43500
        int totalMinutes = (60 * 24 * 30) + 300; // 43200 + 300 = 43500
        int intervalSize = totalMinutes / missionsCount;

        // Most entries in missionScripts_XCOMFILES.rul have startDelay: 30
        int startDelay = 30;

        var random = new Random();
        int[] spawnCountdowns = new int[missionsCount];
        for (int i = 0; i < missionsCount; i++)
            spawnCountdowns[i] = startDelay + i * intervalSize + random.Next(intervalSize);

        var shuffledSpawnCountdowns = spawnCountdowns.Shuffle(random).Shuffle().ToArray();
        Debug.Assert(missionsToInclude.Count == shuffledSpawnCountdowns.Length);
        missionsToInclude.ForEach((mission, i) => mission.SpawnCountdown = shuffledSpawnCountdowns[i]);
    }

    private List<AlienMission> UpdateMissionIds(List<AlienMission> missionsToInclude, AlienMissions alienMissions)
    {
        // We are about to reset all missions IDs to increase in sequence to remove the gaps 
        // caused by deleted missions. For readability we are ordering the missions by 
        // their spawn countdown.
        missionsToInclude = missionsToInclude.OrderBy(mission => mission.SpawnCountdown).ToList();

        int alienMissionsCount = alienMissions.Count;
        int deletedMissionsCount = alienMissionsCount - missionsToInclude.Count;
        Ids["ALIEN_MISSIONS"] -= deletedMissionsCount;

        int currentMinId = alienMissions.Min(mission => mission.UniqueID);
        int missionIdIterator = currentMinId;
        missionsToInclude.ForEach(mission => mission.UniqueID = missionIdIterator++);

        return missionsToInclude;
    }
}