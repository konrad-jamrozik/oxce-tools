namespace OxceTools;

public class AlienMissions : List<AlienMission>
{
    public AlienMissions()
    {

    }

    public AlienMissions(IEnumerable<AlienMission> alienMissions)
    {
        AddRange(alienMissions);
    }
}

public static class AlienMissionsExtensions
{
    public static AlienMissions ToAlienMissions(this IEnumerable<AlienMission> alienMissionsEnumerable) 
        => new AlienMissions(alienMissionsEnumerable);

    public static AlienMissions ToAlienMissions(this AlienMission alienMission) 
        => new AlienMissions(new List<AlienMission> {alienMission});
}
