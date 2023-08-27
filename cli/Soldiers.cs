namespace OxceTools;

public class Soldiers : List<Soldier>
{
    public Soldiers()
    {

    }

    public Soldiers(IEnumerable<Soldier> soldier)
    {
        AddRange(soldier);
    }
}


public static class SoldiersExtensions
{
    public static Soldiers ToSoldiers(this IEnumerable<Soldier> soldiersEnumerable) 
        => new Soldiers(soldiersEnumerable);

    public static Soldiers ToSoldiers(this Soldier soldier) 
        => new Soldiers(new List<Soldier> {soldier});
}