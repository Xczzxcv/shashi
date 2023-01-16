namespace Core;

public class PiecesCollection : List<Piece>, IPoolable
{
    public int Id { get; private set; }

    private const int DefaultPiecesCapacity = SideState.INIT_CHECKERS_COUNT;
    
    private IPool? _parentPool;

    public PiecesCollection()
    { }

    public PiecesCollection(int capacity) : base(capacity)
    { }

    public void Setup(int id, IPool parentPool)
    {
        Id = id;
        _parentPool = parentPool;
        Capacity = DefaultPiecesCapacity;
    }

    public void ReturnToPool()
    {
        _parentPool?.Return(this);
    }

    public void Reset()
    {
        Clear();
    }
}