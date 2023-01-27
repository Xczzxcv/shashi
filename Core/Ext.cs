using System.Runtime.CompilerServices;

namespace Core;

public static class HashsetExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetAny<T>(this HashSet<T> hashSet)
    {
        var enumerator = hashSet.GetEnumerator();
        enumerator.MoveNext();
        enumerator.Dispose();
        return enumerator.Current;
    }
}

public static class ListExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndex<T>(this List<T> list)
    {
        return list.Count - 1;
    }
}