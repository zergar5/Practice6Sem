using Practice6Sem.Core;
using Practice6Sem.Core.Global;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.FEM;
using Practice6Sem.TwoDimensional.Assembling.Local;
using System.Numerics;

namespace Practice6Sem.TwoDimensional;

public class FEMSolution
{
    private readonly Grid<Node2D> _grid;
    private readonly GlobalVector _solution;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly double _omega;

    public FEMSolution
    (
        Grid<Node2D> grid,
        GlobalVector solution,
        LocalBasisFunctionsProvider localBasisFunctionsProvider,
        double omega
    )
    {
        _grid = grid;
        _solution = solution;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
        _omega = omega;
    }

    public (double, double) Calculate(Node2D point)
    {
        if (AreaHas(point))
        {
            var element = _grid.Elements.First(x => ElementHas(x, point));

            var basisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

            var sumS = 0d;
            var sumC = 0d;

            for (var i = 0; i < element.NodesIndexes.Length; i++)
            {
                sumS += _solution[element.NodesIndexes[i] * 2] * basisFunctions[i].Calculate(point);
                sumC += _solution[element.NodesIndexes[i] * 2 + 1] * basisFunctions[i].Calculate(point);
            }

            CourseHolder.WriteSolution(point, sumS, sumC);

            return (sumS, sumC);
        }

        CourseHolder.WriteAreaInfo();
        CourseHolder.WriteSolution(point, double.NaN, double.NaN);
        return (double.NaN, double.NaN);
    }

    public double Calculate(Node2D point, double time)
    {
        if (AreaHas(point))
        {
            var element = _grid.Elements.First(x => ElementHas(x, point));

            var basisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

            var sumS = 0d;
            var sumC = 0d;

            for (var i = 0; i < element.NodesIndexes.Length; i++)
            {
                sumS += _solution[element.NodesIndexes[i] * 2] * basisFunctions[i].Calculate(point);
                sumC += _solution[element.NodesIndexes[i] * 2 + 1] * basisFunctions[i].Calculate(point);
            }

            var result = sumS * Math.Sin(_omega * time) + sumC * Math.Cos(_omega * time);

            CourseHolder.WriteSolution(point, time, result);

            return result;
        }

        CourseHolder.WriteAreaInfo();
        CourseHolder.WriteSolution(point, double.NaN, double.NaN);
        return double.NaN;
    }

    public double Calculate(Node2D point, double time, double r)
    {
        if (AreaHas(point))
        {
            var element = _grid.Elements.First(x => ElementHas(x, point));

            var basisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

            var sumS = 0d;
            var sumC = 0d;

            for (var i = 0; i < element.NodesIndexes.Length; i++)
            {
                sumS += _solution[element.NodesIndexes[i] * 2] * basisFunctions[i].Calculate(point);
                sumC += _solution[element.NodesIndexes[i] * 2 + 1] * basisFunctions[i].Calculate(point);
            }

            var result = (sumS * Math.Sin(_omega * time) + sumC * Math.Cos(_omega * time)) * 2 * Math.PI * r;

            CourseHolder.WriteSolution(point, time, result);

            return result;
        }

        CourseHolder.WriteAreaInfo();
        CourseHolder.WriteSolution(point, double.NaN, double.NaN);
        return double.NaN;
    }

    public double CalcError(Func<Node2D, Complex> u)
    {
        var trueSolution = new GlobalVector(_solution.Count);

        for (var i = 0; i < trueSolution.Count / 2; i++)
        {
            var uValues = u(_grid.Nodes[i]);
            trueSolution[i * 2] = uValues.Real;
            trueSolution[i * 2 + 1] = uValues.Imaginary;
        }

        GlobalVector.Subtract(_solution, trueSolution, trueSolution);

        return trueSolution.Norm;
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var leftCornerNode = _grid.Nodes[element.NodesIndexes[0]];
        var rightCornerNode = _grid.Nodes[element.NodesIndexes[^1]];
        return node.R >= leftCornerNode.R && node.Z >= leftCornerNode.Z &&
               node.R <= rightCornerNode.R && node.Z <= rightCornerNode.Z;
    }

    private bool AreaHas(Node2D node)
    {
        var leftCornerNode = _grid.Nodes[0];
        var rightCornerNode = _grid.Nodes[^1];
        return node.R >= leftCornerNode.R && node.Z >= leftCornerNode.Z &&
               node.R <= rightCornerNode.R && node.Z <= rightCornerNode.Z;
    }
}