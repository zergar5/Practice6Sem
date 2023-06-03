using Practice6Sem.Core.GridComponents;
using System.Numerics;

namespace Practice6Sem.FEM.Parameters;

public interface IFunctionalParameter
{
    public Complex Calculate(int nodeIndex, double mu, double sigma);

    public Complex Calculate(Node2D node, double mu, double sigma);
}