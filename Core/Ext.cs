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

public static class StringExt
{
    public static int ParseNumber(this string str, int startNumberIndex, out int endNumberIndex)
    {
        endNumberIndex = startNumberIndex + 1;
        for (int i = startNumberIndex + 1; i < str.Length; i++)
        {
            var charToProcess = str[i];
            if (char.IsNumber(charToProcess))
            {
                continue;
            }

            endNumberIndex = i;
            break;
        }

        var length = endNumberIndex - startNumberIndex;
        var numberSpan = str.AsSpan(startNumberIndex, length);
        return int.Parse(numberSpan);
    }
}

public static class CharsSpanExt
{
    public static Vec2Int ExtractPosFromChars(this ReadOnlySpan<char> chars, int startPosIndex, int endPosIndex)
    {
        var posChars = chars.Slice(startPosIndex, endPosIndex - startPosIndex);
        var pos = Vec2Int.FromNotation(posChars);

        return pos;
    }
}