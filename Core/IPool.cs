namespace Core;

public interface IPool
{
    public void Return(IPoolable poolableObject);
}