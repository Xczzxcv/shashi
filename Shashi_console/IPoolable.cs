namespace Core;

public interface IPoolable
{
    public int Id { get; }

    public void Reset();
}