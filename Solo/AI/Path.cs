namespace Solo.AI;

//TODO: this should be immutable
public class Path<T>
{
    private readonly Queue<T> _steps = new();

    public Path(IEnumerable<T> steps)
    {
        foreach (var node in steps)
            _steps.Enqueue(node);

        End = steps.LastOrDefault();
    }

    public readonly static Path<T> Empty = new Path<T>(Enumerable.Empty<T>());

    public bool Any() => _steps.Any();

    public T Next() => _steps.Dequeue();

    public readonly T? End;
}
