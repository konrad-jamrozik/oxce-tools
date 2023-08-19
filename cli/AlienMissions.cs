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