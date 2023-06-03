using Practice6Sem.Core;
using Practice6Sem.Core.Boundary;
using Practice6Sem.Core.GridComponents;
using System.Numerics;

namespace Practice6Sem.TwoDimensional.Assembling.Boundary;

public class FirstBoundaryProvider
{
    private readonly Grid<Node2D> _grid;

    public FirstBoundaryProvider(Grid<Node2D> grid)
    {
        _grid = grid;
    }

    public List<FirstCondition> GetConditions(List<int> elementsIndexes, List<Bound> bounds)
    {
        var conditions = new List<FirstCondition>(elementsIndexes.Count * 4);

        for (var i = 0; i < elementsIndexes.Count; i++)
        {
            var (indexes, _) = _grid.Elements[elementsIndexes[i]].GetBoundNodeIndexes(bounds[i]);

            foreach (var t in indexes)
            {
                conditions.Add(new FirstCondition(t * 2, 0d));
                conditions.Add(new FirstCondition(t * 2 + 1, 0d));
            }
        }

        return conditions;
    }

    public List<FirstCondition> GetConditions(int elementsByLength, int elementsByHeight)
    {
        var elementsIndexes = new List<int>(elementsByLength + elementsByHeight * 2 +
                                            (elementsByLength * elementsByHeight -
                                             elementsByLength * (elementsByHeight - 1)));
        var bounds = new List<Bound>(elementsByLength + elementsByHeight * 2 +
                                     (elementsByLength * elementsByHeight
                                      - elementsByLength * (elementsByHeight - 1)));

        for (var i = 0; i < elementsByLength; i++)
        {
            elementsIndexes.Add(i);
            bounds.Add(Bound.Lower);
        }

        for (var i = 0; i < elementsByHeight; i++)
        {
            elementsIndexes.Add(i * elementsByLength);
            bounds.Add(Bound.Left);
        }

        for (var i = 0; i < elementsByHeight; i++)
        {
            elementsIndexes.Add((i + 1) * elementsByLength - 1);
            bounds.Add(Bound.Right);
        }

        for (var i = elementsByLength * (elementsByHeight - 1); i < elementsByLength * elementsByHeight; i++)
        {
            elementsIndexes.Add(i);
            bounds.Add(Bound.Upper);
        }

        return GetConditions(elementsIndexes, bounds);
    }
}