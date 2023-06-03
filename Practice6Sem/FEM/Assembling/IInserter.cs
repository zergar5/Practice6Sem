using Practice6Sem.Core.Global;
using Practice6Sem.Core.Local;

namespace Practice6Sem.FEM.Assembling;

public interface IInserter<in TMatrix>
{
    public void InsertMatrix(TMatrix globalMatrix, LocalMatrix localMatrix);
    public void InsertVector(GlobalVector vector, LocalVector localVector);
}