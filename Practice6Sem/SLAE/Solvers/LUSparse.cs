using Practice6Sem.Core.Global;
using Practice6Sem.SLAE.Preconditions;

namespace Practice6Sem.SLAE.Solvers;

public class LUSparse : ISolver<SparseMatrix>
{
    private readonly LUPreconditioner _luPreconditioner;

    public LUSparse(LUPreconditioner luPreconditioner)
    {
        _luPreconditioner = luPreconditioner;
    }

    public GlobalVector Solve(Equation<SparseMatrix> equation)
    {
        var matrix = _luPreconditioner.Decompose(equation.Matrix);
        var y = CalcY(matrix, equation.RightSide);
        var x = CalcX(matrix, y);

        return x;
    }

    public GlobalVector CalcY(SparseMatrix sparseMatrix, GlobalVector b)
    {
        var y = b;

        for (var i = 0; i < sparseMatrix.CountRows; i++)
        {
            var sum = 0.0;
            for (var j = sparseMatrix.RowsIndexes[i]; j < sparseMatrix.RowsIndexes[i + 1]; j++)
            {
                sum += sparseMatrix.LowerValues[j] * y[sparseMatrix.ColumnsIndexes[j]];
            }
            y[i] = (b[i] - sum) / sparseMatrix.Diagonal[i];
        }

        return y;
    }

    public GlobalVector CalcX(SparseMatrix sparseMatrix, GlobalVector y)
    {
        var x = y.Clone();

        for (var i = sparseMatrix.CountRows - 1; i >= 0; i--)
        {
            for (var j = sparseMatrix.RowsIndexes[i + 1] - 1; j >= sparseMatrix.RowsIndexes[i]; j--)
            {
                x[sparseMatrix.ColumnsIndexes[j]] -= sparseMatrix.UpperValues[j] * x[i];
            }
        }

        return x;
    }
}