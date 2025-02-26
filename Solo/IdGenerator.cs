namespace Solo;

public class IdGenerator<T>
{
    private static int _lastId = 0;

    public static int Next() => Interlocked.Increment(ref _lastId);
}
