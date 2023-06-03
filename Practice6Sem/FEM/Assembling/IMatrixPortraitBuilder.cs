using Practice6Sem.Core;

namespace Practice6Sem.FEM.Assembling;

public interface IMatrixPortraitBuilder<TNode, out TMatrix>
{
    TMatrix Build(Grid<TNode> grid);
}