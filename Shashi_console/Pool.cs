using System.Diagnostics;

namespace Core;

public class Pool<T> where T : IPoolable, new()
{
    private readonly Dictionary<int, T> _content = new();
    private readonly Dictionary<int, bool> _status = new();

    private const int DEFAULT_CAPACITY_INCREASE_AMOUNT = 4;
    private readonly int _capacityIncreaseAmount;

    private int _currentIndex;

    public Pool(int capacityIncreaseAmount = DEFAULT_CAPACITY_INCREASE_AMOUNT)
    {
        Debug.Assert(capacityIncreaseAmount > 0);
        _capacityIncreaseAmount = capacityIncreaseAmount;
        _currentIndex = 0;
    }

    public T Get()
    {
        if (TryGetFree(out var poolable))
        {
            return poolable;
        }

        return SpawnSomeNew();
    }

    private bool TryGetFree(out T poolable)
    {
        foreach (var (index, isFree) in _status)
        {
            if (isFree)
            {
                {
                    poolable = _content[index];
                    return true;
                }
            }
        }

        poolable = default;
        return false;
    }

    private T SpawnSomeNew()
    {
        var newStuffIndex = 0;
        for (int i = 0; i < _capacityIncreaseAmount; i++)
        {
            newStuffIndex = SpawnNew();
        }

        return _content[newStuffIndex];
    }

    private int SpawnNew()
    {
        var newStuff = new T();
        var newStuffIndex = _currentIndex;
        _content.Add(newStuffIndex, newStuff);
        _status.Add(newStuffIndex, true);
        _currentIndex++;

        return newStuffIndex;
    }

    public void Return(int index)
    {
        Debug.Assert(_status[index] == false);
        _status[index] = true;
    }
}