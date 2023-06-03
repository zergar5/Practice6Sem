using Practice6Sem.GridGenerator.Area.Core;

namespace Practice6Sem.GridGenerator.Area.Splitting;

public interface IIntervalSplitter
{
    public IEnumerable<double> EnumerateValues(Interval interval);
    public int Steps { get; }
}