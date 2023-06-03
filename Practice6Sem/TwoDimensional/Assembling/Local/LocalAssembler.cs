using Practice6Sem.Calculus;
using Practice6Sem.Core;
using Practice6Sem.Core.Base;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.Core.Local;
using Practice6Sem.FEM.Assembling.Local;
using Practice6Sem.FEM.Parameters;
using Practice6Sem.GridGenerator.Area.Core;
using Practice6Sem.TwoDimensional.Parameters;

namespace Practice6Sem.TwoDimensional.Assembling.Local;

public class LocalAssembler : ILocalAssembler
{
    private readonly Grid<Node2D> _grid;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly MaterialFactory _materialFactory;
    private readonly IFunctionalParameter _functionalParameter;
    private readonly DoubleIntegralCalculator _doubleIntegralCalculator;
    private readonly DerivativeCalculator _derivativeCalculator;
    private readonly double _omega;
    private readonly BaseMatrix _massMatrix = new(4);
    private readonly BaseMatrix _stiffnessMatrix = new(4);
    private readonly BaseMatrix _matrix = new(8);
    private readonly BaseVector _rightPart = new(8);
    private readonly int[] _complexIndexes = new int[8];

    public LocalAssembler
    (
        Grid<Node2D> grid,
        LocalBasisFunctionsProvider localBasisFunctionsProvider,
        MaterialFactory materialFactory,
        IFunctionalParameter functionalParameter,
        DoubleIntegralCalculator doubleIntegralCalculator,
        DerivativeCalculator derivativeCalculator,
        double omega
    )
    {
        _grid = grid;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
        _materialFactory = materialFactory;
        _functionalParameter = functionalParameter;
        _doubleIntegralCalculator = doubleIntegralCalculator;
        _derivativeCalculator = derivativeCalculator;
        _omega = omega;
    }

    public LocalMatrix AssembleMatrix(Element element)
    {
        var matrix = GetComplexMatrix(element);
        var indexes = GetComplexIndexes(element);

        return new LocalMatrix(indexes, matrix);
    }

    public LocalVector AssembleRightSide(Element element)
    {
        var vector = GetComplexRightPart(element);
        var indexes = GetComplexIndexes(element);

        return new LocalVector(indexes, vector);
    }

    private BaseMatrix GetStiffnessMatrix(Element element)
    {
        var rInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].R, _grid.Nodes[element.NodesIndexes[1]].R);
        var zInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].Z, _grid.Nodes[element.NodesIndexes[2]].Z);

        var localBasisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                _stiffnessMatrix[i, j] = _doubleIntegralCalculator.Calculate
                (
                    rInterval,
                    zInterval,
                    (r, z) =>
                    {
                        var node = new Node2D(r, z);
                        return
                            (_derivativeCalculator.Calculate(localBasisFunctions[i], node, 'r') +
                             localBasisFunctions[i].Calculate(node) / r) *
                            (_derivativeCalculator.Calculate(localBasisFunctions[j], node, 'r') +
                             localBasisFunctions[j].Calculate(node) / r) +
                            _derivativeCalculator.Calculate(localBasisFunctions[i], node, 'z') *
                            _derivativeCalculator.Calculate(localBasisFunctions[j], node, 'z');
                    }
                );

                _stiffnessMatrix[j, i] = _stiffnessMatrix[i, j];
            }
        }

        return _stiffnessMatrix;
    }

    private BaseMatrix GetMassMatrix(Element element)
    {
        var rInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].R, _grid.Nodes[element.NodesIndexes[1]].R);
        var zInterval = new Interval(_grid.Nodes[element.NodesIndexes[0]].Z, _grid.Nodes[element.NodesIndexes[2]].Z);

        var localBasisFunctions = _localBasisFunctionsProvider.GetBilinearFunctions(element);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            for (var j = 0; j <= i; j++)
            {
                _massMatrix[i, j] = _doubleIntegralCalculator.Calculate
                (
                    rInterval,
                    zInterval,
                    (r, z) =>
                    {
                        var node = new Node2D(r, z);
                        return
                            localBasisFunctions[i].Calculate(node) * localBasisFunctions[j].Calculate(node) * r;
                    }
                );

                _massMatrix[j, i] = _massMatrix[i, j];
            }
        }

        return _massMatrix;
    }

    private BaseMatrix GetComplexMatrix(Element element)
    {
        var mass = GetMassMatrix(element);
        var stiffness = GetStiffnessMatrix(element);
        var material = _materialFactory.GetById(element.MaterialId);

        BaseMatrix.Multiply(1d / material.Mu, stiffness, stiffness);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            for (var j = 0; j < element.NodesIndexes.Length; j++)
            {
                _matrix[i * 2, j * 2] = stiffness[i, j];
                _matrix[i * 2, j * 2 + 1] = -_omega * material.Sigma * mass[i, j];
                _matrix[i * 2 + 1, j * 2] = -_matrix[i * 2, j * 2 + 1];
                _matrix[i * 2 + 1, j * 2 + 1] = stiffness[i, j];
            }
        }

        return _matrix;
    }

    private BaseVector GetComplexRightPart(Element element)
    {
        Span<double> fS = stackalloc double[element.NodesIndexes.Length];
        Span<double> fC = stackalloc double[element.NodesIndexes.Length];

        Span<double> bufferFS = stackalloc double[element.NodesIndexes.Length];
        Span<double> bufferFC = stackalloc double[element.NodesIndexes.Length];

        var material = _materialFactory.GetById(element.MaterialId);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            var fValue = _functionalParameter.Calculate(element.NodesIndexes[i], material.Mu, material.Sigma);
            bufferFS[i] = fValue.Real;
            bufferFC[i] = fValue.Imaginary;
        }

        BaseMatrix.Multiply(_massMatrix, bufferFS, fS);
        BaseMatrix.Multiply(_massMatrix, bufferFC, fC);

        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            _rightPart[i * 2] = fS[i];
            _rightPart[i * 2 + 1] = fC[i];
        }

        return _rightPart;
    }

    private int[] GetComplexIndexes(Element element)
    {
        for (var i = 0; i < element.NodesIndexes.Length; i++)
        {
            _complexIndexes[i * 2] = 2 * element.NodesIndexes[i];
            _complexIndexes[i * 2 + 1] = _complexIndexes[i * 2] + 1;
        }

        return _complexIndexes;
    }
}