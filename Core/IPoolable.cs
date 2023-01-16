namespace Core;

public interface IPoolable
{
    public int Id { get; }

    public void Setup(int id, IPool parentPool);
    public void ReturnToPool();
    public void Reset();
}