namespace Practice6Sem.SLAE.Preconditions;

public interface IPreconditioner<TMatrix>
{
    public TMatrix Decompose(TMatrix globalMatrix);
}