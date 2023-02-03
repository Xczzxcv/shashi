namespace Core;

public abstract class CollectionBase<T> : List<T>, IPoolable
    where T : struct
{
    public int Id { get; private set; }

    private const int DefaultPiecesCapacity = SideState.INIT_CHECKERS_COUNT;
    
    private IPool? _parentPool;

    protected CollectionBase()
    { }

    protected CollectionBase(IEnumerable<T> collection) : base(collection)
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