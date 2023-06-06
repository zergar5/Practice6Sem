using System.Net.Http.Headers;
using CourseProject.SLAE;
using Practice6Sem.Core.Global;
using Practice6Sem.FEM;
using Practice6Sem.SLAE.Preconditions;

namespace Practice6Sem.SLAE.Solvers;

public class LOS
{
    private readonly LUPreconditioner _luPreconditioner;
    private readonly LUSparse _luSparse;
    private SparseMatrix _preconditionMatrix;
    private GlobalVector _r;
    private GlobalVector _z;
    private GlobalVector _p;

    public LOS(LUPreconditioner luPreconditioner, LUSparse luSparse)
    {
        _luPreconditioner = luPreconditioner;
        _luSparse = luSparse;
    }

    private void PrepareProcess(Equation<SparseMatrix> equation)
    {
        _preconditionMatrix = _luPreconditioner.Decompose(_preconditionMatrix);
        var bufferVector = SparseMatrix.Multiply(equation.Matrix, equation.Solution);
        _r = _luSparse.CalcY(_preconditionMatrix, GlobalVector.Subtract(equation.RightSide, bufferVector, bufferVector));
        _z = _luSparse.CalcX(_preconditionMatrix, _r);
        _p = _luSparse.CalcY(_preconditionMatrix, SparseMatrix.Multiply(equation.Matrix, _z));
    }

    public GlobalVector Solve(Equation<SparseMatrix> equation, SparseMatrix preconditionMatrix)
    {
        _preconditionMatrix = preconditionMatrix;
        PrepareProcess(equation);
        IterationProcess(equation);
        return equation.Solution;
    }

    private void IterationProcess(Equation<SparseMatrix> equation)
    {
        Console.WriteLine("LOS");

        var residual = GlobalVector.ScalarProduct(_r, _r);
        var residualNext = residual;
        var bufferVector = new GlobalVector(equation.Solution.Count);

        for (var i = 1; i <= MethodsConfig.MaxIterations && residualNext > Math.Pow(MethodsConfig.Eps, 2); i++)
        {
            var scalarPP = GlobalVector.ScalarProduct(_p, _p);

            var alpha = GlobalVector.ScalarProduct(_p, _r) / scalarPP;

            GlobalVector.Multiply(alpha, _z, bufferVector);

            GlobalVector.Sum(equation.Solution, bufferVector, equation.Solution);

            var rNext = GlobalVector.Subtract(_r, GlobalVector.Multiply(alpha, _p, bufferVector));

            bufferVector = _luSparse.CalcX(_preconditionMatrix, rNext);
            var LAUr = _luSparse.CalcY(_preconditionMatrix, SparseMatrix.Multiply(equation.Matrix, bufferVector));

            var beta = -(GlobalVector.ScalarProduct(_p, LAUr) / scalarPP);

            var zNext = GlobalVector.Sum(_luSparse.CalcX(_preconditionMatrix, rNext), GlobalVector.Multiply(beta, _z, _z));

            var pNext = GlobalVector.Sum(LAUr, GlobalVector.Multiply(beta, _p, _p));

            _r = rNext;
            _z = zNext;
            _p = pNext;

            residualNext = GlobalVector.ScalarProduct(_r, _r) / residual;

            CourseHolder.GetInfo(i, residualNext);
        }

        Console.WriteLine();
    }
}