using Practice6Sem.Core.Global;
using Practice6Sem.SLAE.Preconditions;

namespace Practice6Sem.SLAE.Solvers;

public class LUSparse
{
    private readonly LUPreconditioner _luPreconditioner;

    public LUSparse(LUPreconditioner luPreconditioner)
    {
        _luPreconditioner = luPreconditioner;
    }

    public GlobalVector CalcY(SparseMatrix sparseMatrix, GlobalVector b, GlobalVector? y = null)
    {
        y ??= new GlobalVector(b.Count);

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

    public GlobalVector CalcX(SparseMatrix sparseMatrix, GlobalVector y, GlobalVector? x = null)
    {
        x ??= y.Clone();

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