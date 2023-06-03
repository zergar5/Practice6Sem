using Practice6Sem.Core.Boundary;
using Practice6Sem.Core.Global;

namespace Practice6Sem.TwoDimensional.Assembling.Global;

public class GaussExcluder
{
    public void Exclude(Equation<SparseMatrix> equation, FirstCondition condition)
    {
        equation.RightSide[condition.NodeIndex] = condition.Value;
        equation.Matrix.Diagonal[condition.NodeIndex] = 1d;

        for (var j = equation.Matrix.RowsIndexes[condition.NodeIndex];
             j < equation.Matrix.RowsIndexes[condition.NodeIndex + 1];
             j++)
        {
            equation.Matrix.LowerValues[j] = 0d;
        }

        for (var j = condition.NodeIndex + 1; j < equation.Matrix.CountRows; j++)
        {
            var elementIndex = equation.Matrix[j, condition.NodeIndex];

            if (elementIndex == -1) continue;
            equation.Matrix.UpperValues[elementIndex] = 0;
        }
    }
}