using Practice6Sem.Core.Global;

namespace Practice6Sem.SLAE.Solvers;

public interface ISolver<TMatrix>
{
    public GlobalVector Solve(Equation<TMatrix> equation);
}