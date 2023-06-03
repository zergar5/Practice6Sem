﻿using Practice6Sem.Core;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.GridGenerator.Area.Splitting;

namespace Practice6Sem.GridGenerator;

public class GridBuilder2D : IGridBuilder<Node2D>
{
    private AxisSplitParameter? _rAxisSplitParameter;
    private AxisSplitParameter? _zAxisSplitParameter;
    private int[]? _materialsId;

    private int GetTotalRElements => _rAxisSplitParameter.Splitters.Sum(r => r.Steps);
    private int GetTotalZElements => _zAxisSplitParameter.Splitters.Sum(z => z.Steps);

    public GridBuilder2D SetRAxis(AxisSplitParameter splitParameter)
    {
        _rAxisSplitParameter = splitParameter;
        return this;
    }

    public GridBuilder2D SetZAxis(AxisSplitParameter splitParameter)
    {
        _zAxisSplitParameter = splitParameter;
        return this;
    }

    public GridBuilder2D SetMaterials(int[] materialsId)
    {
        _materialsId = materialsId;
        return this;
    }

    public Grid<Node2D> Build()
    {
        if (_rAxisSplitParameter == null || _zAxisSplitParameter == null)
            throw new ArgumentNullException();

        var totalRElements = GetTotalRElements;

        var totalNodes = GetTotalNodes();
        var totalElements = GetTotalElements();

        var nodes = new Node2D[totalNodes];
        var elements = new Element[totalElements];

        _materialsId ??= new int[totalElements];

        var i = 0;

        foreach (var (zSection, zSplitter) in _zAxisSplitParameter.SectionWithParameter)
        {
            var zValues = zSplitter.EnumerateValues(zSection);
            if (i > 0) zValues = zValues.Skip(1);

            foreach (var z in zValues)
            {
                var j = 0;

                foreach (var (rSection, rSplitter) in _rAxisSplitParameter.SectionWithParameter)
                {
                    var rValues = rSplitter.EnumerateValues(rSection);
                    if (j > 0) rValues = rValues.Skip(1);

                    foreach (var r in rValues)
                    {
                        var nodeIndex = j + i * (totalRElements + 1);

                        nodes[nodeIndex] = new Node2D(r, z);

                        if (i > 0 && j > 0)
                        {
                            var elementIndex = j - 1 + (i - 1) * totalRElements;
                            var nodesIndexes = GetCurrentElementIndexes(i - 1, j - 1);

                            elements[elementIndex] = new Element(
                                nodesIndexes,
                                nodes[nodesIndexes[1]].R - nodes[nodesIndexes[0]].R,
                                nodes[nodesIndexes[2]].Z - nodes[nodesIndexes[0]].Z,
                                _materialsId[elementIndex]
                                );
                        }

                        j++;
                    }
                }

                i++;
            }
        }

        return new Grid<Node2D>(nodes, elements);
    }

    private int GetTotalNodes()
    {
        return (GetTotalRElements + 1) * (GetTotalZElements + 1);
    }

    private int GetTotalElements()
    {
        return GetTotalRElements * GetTotalZElements;
    }

    private int[] GetCurrentElementIndexes(int j, int k)
    {
        var totalRElements = GetTotalRElements;

        var indexes = new[]
        {
            k + j * (totalRElements + 1),
            k + 1 + j * (totalRElements + 1),
            k + (j + 1) * (totalRElements + 1),
            k + 1 + (j + 1) * (totalRElements + 1)
        };

        return indexes;
    }
}