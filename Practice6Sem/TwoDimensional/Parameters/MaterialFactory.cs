using Practice6Sem.Core.GridComponents;

namespace Practice6Sem.TwoDimensional.Parameters;

public class MaterialFactory
{
    private readonly Dictionary<int, double> _mus;
    private readonly Dictionary<int, double> _sigmas;

    public MaterialFactory(IEnumerable<double> mus, IEnumerable<double> sigmas)
    {
        _mus = mus.Select((value, index) => new KeyValuePair<int, double>(index, value))
            .ToDictionary(index => index.Key, value => value.Value);
        _sigmas = sigmas.Select((value, index) => new KeyValuePair<int, double>(index, value))
            .ToDictionary(index => index.Key, value => value.Value);
    }

    public Material GetById(int id)
    {
        return new Material(
            _mus[id],
            _sigmas[id]
        );
    }
}