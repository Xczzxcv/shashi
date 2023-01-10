namespace Core;

public class Game
{
    private readonly List<MoveInfo> _possibleMoves = new();
    private BoardState _boardState;

    public void Init()
    {
        _boardState = BoardState.Initial();
    }

    public List<MoveInfo> GetPossibleSideMoves(Side side)
    {
        _possibleMoves.Clear();
        var pieces = GetPieces(side);
        foreach (var piece in pieces)
        {
            AddPossiblePieceMoves(piece);
        }

        return _possibleMoves;
    }

    private List<Piece> GetPieces(Side side)
    {
        var piecesSrc = side == Side.White
            ? _boardState.White
            : _boardState.Black;
        return piecesSrc.GetPieces();
    }

    private void AddPossiblePieceMoves(Piece piece)
    {
        if (TryAddPossibleTakes(piece))
        {
            return;
        }

        AddPossibleMoves(piece);
    }

    private bool TryAddPossibleTakes(Piece piece)
    {
        var vulnerableEnemyPieces = Enumerable.Empty<Piece>();
        var possibleTakesCounter = 0;
        foreach (var enemyPiece in vulnerableEnemyPieces)
        {
            if (!CanTake(piece, enemyPiece))
            {
                continue;
            }

            var possibleTake = MoveInfo.BuildTake(_boardState, piece, enemyPiece);
            _possibleMoves.Add(possibleTake);
            possibleTakesCounter++;
        }

        return possibleTakesCounter > 0;
    }

    private bool CanTake(Piece piece, Piece enemyPiece)
    {
        return false;
    }

    private void AddPossibleMoves(Piece piece)
    {
        var boardSquares = _boardState.GetEmptySquares();
        foreach (var boardSquare in boardSquares)
        {
            if (!CanMoveTo(piece, boardSquare))
            {
                continue;
            }

            var possibleTake = MoveInfo.BuildMove(piece, boardSquare);
            _possibleMoves.Add(possibleTake);
        }
    }

    private bool CanMoveTo(Piece piece, Vec2Int boardSquare)
    {
        if (!_boardState.IsEmptySquare(boardSquare))
        {
            return false;
        }

        var diffX = boardSquare.X - piece.Position.X;
        var diffY = boardSquare.Y - piece.Position.Y;
        var rangeIs1ByDiagonal = Math.Abs(diffX) == 1 && Math.Abs(diffY) == 1;
        if (!rangeIs1ByDiagonal)
        {
            return false;
        }

        if (piece.Rank == PieceRank.Checker)
        {
            if (piece.Side == Side.White
                && diffY <= 0)
            {
                return false;
            }

            if (piece.Side == Side.Black
                && diffY >= 0)
            {
                return false;
            }
        }

        return true;
    }

    public string GetView()
    {
        return _boardState.GetView();
    }
}