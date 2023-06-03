namespace Practice6Sem.Core.Base;

public class BaseVector
{
    public double[] Vector { get; }

    public BaseVector() : this(Array.Empty<double>()) { }
    public BaseVector(double[] vector)
    {
        Vector = vector;
    }
    public BaseVector(int size) : this(new double[size]) { }

    public int Count => Vector.Length;
    public double this[int index]
    {
        get => Vector[index];
        set => Vector[index] = value;
    }

    public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)Vector).GetEnumerator();
}