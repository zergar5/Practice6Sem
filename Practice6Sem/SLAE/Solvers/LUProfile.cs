using Practice6Sem.Core.Global;

namespace Practice6Sem.SLAE.Solvers;

public class LUProfile : ISolver<ProfileMatrix>
{
    public GlobalVector Solve(Equation<ProfileMatrix> equation)
    {
        var matrix = equation.Matrix.LU();
        var y = CalcY(matrix, equation.Solution, equation.RightSide);
        var x = CalcX(matrix, y);

        return x;
    }

    private GlobalVector CalcY(ProfileMatrix globalMatrix, GlobalVector q, GlobalVector b)
    {
        var y = q;

        for (var i = 0; i < globalMatrix.CountRows; i++)
        {
            var sum = 0d;

            var k = i - (globalMatrix.RowsIndexes[i + 1] - globalMatrix.RowsIndexes[i]);

            for (var j = globalMatrix.RowsIndexes[i]; j < globalMatrix.RowsIndexes[i + 1]; j++, k++)
            {
                sum += globalMatrix.LowerValues[j] * y[k];
            }

            y[i] = (b[i] - sum) / globalMatrix.Diagonal[i];
        }

        return y;
    }

    private GlobalVector CalcX(ProfileMatrix sparseMatrix, GlobalVector y)
    {
        var x = y;

        for (var i = sparseMatrix.CountRows - 1; i >= 0; i--)
        {
            var k = i - 1;

            for (var j = sparseMatrix.RowsIndexes[i + 1] - 1; j >= sparseMatrix.RowsIndexes[i]; j--, k--)
            {
                x[k] -= sparseMatrix.UpperValues[j] * x[i];
            }
        }

        return x;
    }
}