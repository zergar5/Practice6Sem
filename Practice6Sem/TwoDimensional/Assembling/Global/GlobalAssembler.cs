using Practice6Sem.Core;
using Practice6Sem.Core.Boundary;
using Practice6Sem.Core.Global;
using Practice6Sem.FEM.Assembling;
using Practice6Sem.FEM.Assembling.Local;

namespace Practice6Sem.TwoDimensional.Assembling.Global;

public class GlobalAssembler<TNode>
{
    private readonly IMatrixPortraitBuilder<TNode, SparseMatrix> _matrixPortraitBuilder;
    private readonly ILocalAssembler _localAssembler;
    private readonly IInserter<SparseMatrix> _inserter;
    private readonly GaussExcluder _gaussExсluder;
    private Equation<SparseMatrix> _equation;

    public GlobalAssembler
    (
        IMatrixPortraitBuilder<TNode, SparseMatrix> matrixPortraitBuilder,
        ILocalAssembler localAssembler,
        IInserter<SparseMatrix> inserter,
        GaussExcluder gaussExсluder
    )
    {
        _matrixPortraitBuilder = matrixPortraitBuilder;
        _localAssembler = localAssembler;
        _inserter = inserter;
        _gaussExсluder = gaussExсluder;
    }

    public GlobalAssembler<TNode> AssembleEquation(Grid<TNode> grid)
    {
        var globalMatrix = _matrixPortraitBuilder.Build(grid);
        _equation = new Equation<SparseMatrix>(
            globalMatrix,
            new GlobalVector(grid.Nodes.Length * 2),
            new GlobalVector(grid.Nodes.Length * 2)
        );

        foreach (var element in grid)
        {
            var localMatrix = _localAssembler.AssembleMatrix(element);
            var localVector = _localAssembler.AssembleRightSide(element);

            _inserter.InsertMatrix(_equation.Matrix, localMatrix);
            _inserter.InsertVector(_equation.RightSide, localVector);
        }

        return this;
    }

    public GlobalAssembler<TNode> ApplyFirstConditions(List<FirstCondition> conditions)
    {
        foreach (var condition in conditions)
        {
            _gaussExсluder.Exclude(_equation, condition);
        }

        return this;
    }

    public Equation<SparseMatrix> BuildEquation()
    {
        return _equation;
    }
}