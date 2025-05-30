using System.Collections;

namespace Solo.AI;

/// Based on the implementation by Eric Lippert
/// http://blogs.msdn.com/b/ericlippert/archive/2007/10/02/path-finding-using-a-in-c-3-0.aspx
public class Pathfinder
{
    private class TempPath<TN> : IEnumerable<TN>
    {
        public TN LastStep { get; private set; }
        public TempPath<TN> PreviousSteps { get; private set; }
        public float TotalCost { get; private set; }

        private TempPath(TN lastStep, TempPath<TN> previousSteps, float totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }
        public TempPath(TN start) : this(start, null, 0) { }
        public TempPath<TN> AddStep(TN step, float stepCost)
        {
            return new TempPath<TN>(step, this, TotalCost + stepCost);
        }

        public IEnumerator<TN> GetEnumerator()
        {
            for (TempPath<TN> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/issues/17730
    /// </summary>
    public static Task<Path<TN>> FindPathAsync<TN>(TN start,
                                        TN destination,
                                        Func<TN, TN, float> distance,
                                        Func<TN, IEnumerable<TN>> findNeighbours)
        => Task.Run(() => FindPath(start, destination, distance, findNeighbours));

    public static Path<TN> FindPath<TN>(TN start, TN destination, Func<TN, TN, float> distance, Func<TN, IEnumerable<TN>> findNeighbours)
    {
        var path = RunPathfinder(start, destination, distance, findNeighbours);
        if (path is null)
            return new Path<TN>([start]);

        var steps = path.Reverse();
        return new Path<TN>(steps);
    }

    private static TempPath<TN>? RunPathfinder<TN>(TN start,
                                        TN destination,
                                        Func<TN, TN, float> distance,
                                        Func<TN, IEnumerable<TN>> findNeighbours)
    {
        var closed = new HashSet<TN>();
        var queue = new PriorityQueue<TempPath<TN>, float>();
        queue.Enqueue(new TempPath<TN>(start), 0);
        while (0 != queue.Count)
        {
            var path = queue.Dequeue();
            if (closed.Contains(path.LastStep))
                continue;
            if (path.LastStep.Equals(destination))
                return path;
            closed.Add(path.LastStep);

            var neighs = findNeighbours(path.LastStep);
            if (null != neighs && neighs.Any())
            {
                foreach (TN n in neighs)
                {
                    var d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    var newPriority = newPath.TotalCost + distance(n, destination);
                    queue.Enqueue(newPath, newPriority);
                }
            }
        }
        return null;
    }
}
