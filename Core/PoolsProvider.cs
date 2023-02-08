using Core.NeuralNet;

namespace Core;

public class PoolsProvider
{
    public readonly Pool<PiecesCollection> PiecesCollectionPool = new();
    public readonly Pool<MovesCollection> MovesCollectionPool = new();
    public readonly Pool<Vectors2IntCollection> VectorsCollectionPool = new();
    public readonly Pool<BoardNeuralNetInput> BoardNeuralNetInputPool = new();
    
    public void LogPoolsStat(Game game)
    {
        MovesCollectionPool.LogPoolStat(game);
        PiecesCollectionPool.LogPoolStat(game);
        VectorsCollectionPool.LogPoolStat(game);
        BoardNeuralNetInputPool.LogPoolStat(game);
    }
}