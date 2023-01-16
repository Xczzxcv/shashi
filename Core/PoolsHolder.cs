using System.Diagnostics;

namespace Core;

public static class PoolsHolder
{
    public static readonly Pool<PiecesCollection> PiecesCollectionPool = new();

    public static long GetPiecesCallsCount;
    public static readonly Stopwatch GetPiecesSw = new();
}