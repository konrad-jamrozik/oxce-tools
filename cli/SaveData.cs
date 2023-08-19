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
    public required object GeneratedEvents;
    public required object UfopediaRuleStatus;
    public required object ManufactureRuleStatus;
    public required object ResearchRuleStatus;
    public required object MonthlyPurchaseLimitLog;
    public required object HiddenPurchaseItems;
    public required object CustomRuleCraftDeployments;
    public required object AlienStrategy;
    public required object MissionStatistics;
    public required object Options;

    public void Update(AlienMissions alienMissions, bool shuffleInTime = true)
    {
        List<AlienMission> missionsToInclude =
            alienMissions.Where(mission => string.IsNullOrWhiteSpace(mission.Delete)).ToList();

        if (shuffleInTime)
            ShuffleInTime(missionsToInclude);

        UpdateMissionIds(missionsToInclude, alienMissions);

        AlienMissions = new AlienMissions(missionsToInclude);
    }

    private static void ShuffleInTime(List<AlienMission> missionsToInclude)
    {
        int missionsCount = missionsToInclude.Count;
        int totalMinutes = 60 * 24 * 30; // 43200
        int intervalSize = totalMinutes / missionsCount;

        var random = new Random();
        int[] spawnCountdowns = new int[missionsCount];
        for (int i = 0; i < missionsCount; i++)
            spawnCountdowns[i] = i * intervalSize + random.Next(intervalSize);

        var shuffledSpawnCountdowns = spawnCountdowns.Shuffle(random).Shuffle().ToArray();
        Debug.Assert(missionsToInclude.Count == shuffledSpawnCountdowns.Length);
        missionsToInclude.ForEach((mission, i) => mission.SpawnCountdown = shuffledSpawnCountdowns[i]);
    }

    private void UpdateMissionIds(List<AlienMission> missionsToInclude, AlienMissions alienMissions)
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
    }
}