using Practice6Sem.Core;

namespace Practice6Sem.GridGenerator;

public interface IGridBuilder<TPoint>
{
    public Grid<TPoint> Build();
}