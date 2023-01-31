namespace Core;

public partial class Game
{
    public record struct History(List<MoveInfo> Moves, List<Board> Boards);

    public History GetHistory()
    {
        return new History(_madeMoves, _playedBoards);
    }

    public History GetSideHistory(Side side)
    {
        return side switch
        {
            Side.White => new History(
                _madeMoves.Where((_, i) => i % 2 == 0).ToList(),
                _playedBoards.Where((_, i) => i % 2 == 0).ToList()
            ),
            Side.Black => new History(
                _madeMoves.Where((_, i) => i % 2 == 1).ToList(),
                _playedBoards.Where((_, i) => i % 2 == 1).ToList()
            ),
            _ => throw ThrowHelper.WrongSideException(side)
        };
    }
}