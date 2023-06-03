using Practice6Sem.Core.GridComponents;
using Practice6Sem.Core.Local;

namespace Practice6Sem.FEM.Assembling.Local;

public interface ILocalAssembler
{
    public LocalMatrix AssembleMatrix(Element element);
    public LocalVector AssembleRightSide(Element element);
}