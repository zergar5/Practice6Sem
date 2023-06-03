using Practice6Sem.Core;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.FEM.Parameters;
using System.Numerics;

namespace Practice6Sem.TwoDimensional.Parameters;

public class RightPartParameter : IFunctionalParameter
{
    private readonly Func<Node2D, double, double, Complex> _function;
    private readonly Grid<Node2D> _grid;

    public RightPartParameter(
        Func<Node2D, double, double, Complex> function,
        Grid<Node2D> grid
    )
    {
        _function = function;
        _grid = grid;
    }

    public Complex Calculate(int nodeNumber, double mu, double sigma)
    {
        var node = _grid.Nodes[nodeNumber];
        return Calculate(node, mu, sigma);
    }

    public Complex Calculate(Node2D node, double mu, double sigma)
    {
        return _function(node, mu, sigma);
    }
}