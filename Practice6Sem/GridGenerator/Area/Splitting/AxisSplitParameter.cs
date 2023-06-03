﻿using Practice6Sem.GridGenerator.Area.Core;

namespace Practice6Sem.GridGenerator.Area.Splitting;

public class AxisSplitParameter
{
    public Interval[] Sections { get; }
    public IIntervalSplitter[] Splitters { get; }

    public IEnumerable<(Interval section, IIntervalSplitter parameter)> SectionWithParameter =>
        Sections.Select((section, index) => new ValueTuple<Interval, IIntervalSplitter>(section, Splitters[index]));

    public AxisSplitParameter(double[] points, params IIntervalSplitter[] splitters)
    {
        if (points.Length - 1 != splitters.Length)
            throw new ArgumentException();

        Sections = GenerateSections(points).ToArray();
        Splitters = splitters;
    }

    private IEnumerable<Interval> GenerateSections(IEnumerable<double> points)
    {
        var left = points.First();
        foreach (var point in points.Skip(1))
        {
            var end = point;
            yield return new Interval(left, end);

            left = end;
        }
    }
}