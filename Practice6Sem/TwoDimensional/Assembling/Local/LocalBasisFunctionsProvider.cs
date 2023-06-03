using Practice6Sem.Core;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.FEM;

namespace Practice6Sem.TwoDimensional.Assembling.Local;

public class LocalBasisFunctionsProvider
{
    private readonly Grid<Node2D> _grid;
    private readonly LinearFunctionsProvider _linearFunctionsProvider;

    public LocalBasisFunctionsProvider(Grid<Node2D> grid, LinearFunctionsProvider linearFunctionsProvider)
    {
        _grid = grid;
        _linearFunctionsProvider = linearFunctionsProvider;
    }

    public LocalBasisFunction[] GetBilinearFunctions(Element element)
    {
        var firstXFunction =
            _linearFunctionsProvider.CreateFirstFunction(_grid.Nodes[element.NodesIndexes[1]].R, element.Length);
        var secondXFunction =
            _linearFunctionsProvider.CreateSecondFunction(_grid.Nodes[element.NodesIndexes[0]].R, element.Length);
        var firstYFunction =
            _linearFunctionsProvider.CreateFirstFunction(_grid.Nodes[element.NodesIndexes[2]].Z, element.Height);
        var secondYFunction =
            _linearFunctionsProvider.CreateSecondFunction(_grid.Nodes[element.NodesIndexes[0]].Z, element.Height);

        var basisFunctions = new LocalBasisFunction[]
        {
            new (firstXFunction, firstYFunction),
            new (secondXFunction, firstYFunction),
            new (firstXFunction, secondYFunction),
            new (secondXFunction, secondYFunction)
        };

        return basisFunctions;
    }
}