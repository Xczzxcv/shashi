using System.Diagnostics;

namespace Core;

public class Pool<T> : IPool
    where T : IPoolable, new()
{
    private readonly Dictionary<int, T> _content = new();
    private readonly HashSet<int> _freeThings = new();
    private readonly HashSet<int> _rentedThings = new();

    private const int DefaultCapacityIncreaseAmount = 4;
    private readonly int _capacityIncreaseAmount;

    private int _currentIndex;

    public Pool(int capacityIncreaseAmount = DefaultCapacityIncreaseAmount)
    {
        Debug.Assert(capacityIncreaseAmount > 0);
        _capacityIncreaseAmount = capacityIncreaseAmount;
        _currentIndex = 0;
    }

    public long FreeTakenCounter;
    public long SpawnedTakenCounter;
    public int CurrentSize => _content.Count;
    public int CurrentFreeCount => _freeThings.Count;
    public int CurrentRentedCount => _rentedThings.Count;

    public T Get()
    {
        if (TryGetFree(out var poolable))
        {
            FreeTakenCounter++;
        }
        else
        {
            SpawnedTakenCounter++;
            poolable = SpawnSomeNew();
        }

        return poolable;
    }

    private bool TryGetFree(out T poolable)
    {
        if (_freeThings.Count > 0)
        {
            poolable = RentPoolable(_freeThings.GetAny());
            return true;
        }

        poolable = default;
        return false;
    }

    private T RentPoolable(int index)
    {
        Debug.Assert(_freeThings.Contains(index));
        Debug.Assert(!_rentedThings.Contains(index));

        _freeThings.Remove(index);
        _rentedThings.Add(index);
        return _content[index];
    }

    private T SpawnSomeNew()
    {
        var newStuffIndex = 0;
        for (int i = 0; i < _capacityIncreaseAmount; i++)
        {
            newStuffIndex = SpawnNew();
        }

        return RentPoolable(newStuffIndex);
    }

    private int SpawnNew()
    {
        var newStuff = new T();
        var newStuffIndex = _currentIndex;
        newStuff.Setup(newStuffIndex, this);

        _content.Add(newStuffIndex, newStuff);
        _freeThings.Add(newStuffIndex);
        _currentIndex++;

        return newStuffIndex;
    }

    public void Return(IPoolable poolableObject)
    {
        var index = poolableObject.Id;
        Debug.Assert(!_freeThings.Contains(index));
        Debug.Assert(_rentedThings.Contains(index));

        _rentedThings.Remove(index);
        _freeThings.Add(index);
        _content[index].Reset();
    }

    public void LogPoolStat()
    {
        DefaultLogger.Log($"{typeof(T)} Pool stat: size {CurrentSize}\n" +
                          $"free {FreeTakenCounter} " +
                          $"spawned {SpawnedTakenCounter}\n" +
                          $"current: free {CurrentFreeCount} " +
                          $"rented {CurrentRentedCount}");
    }
}