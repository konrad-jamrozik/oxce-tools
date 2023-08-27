namespace OxceTools;

public class Bases : List<Base>
{
    public Bases()
    {

    }

    public Bases(IEnumerable<Base> @base)
    {
        AddRange(@base);
    }
}

public static class BasesExtensions
{
    public static Bases ToBases(this IEnumerable<Base> basesEnumerable) 
        => new Bases(basesEnumerable);

    public static Bases ToBases(this Base @base) 
        => new Bases(new List<Base> {@base});
}
