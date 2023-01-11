namespace Core;

public class RulesManager
{
    public List<MoveInfo> GetPossibleSideMoves(Side side, Board board)
    {
        var possibleMoves = new List<MoveInfo>();
        var pieces = board.GetPieces(side);
        if (TryAddPossibleTakes(pieces, possibleMoves, board))
        {
            return possibleMoves;
        }
        
        AddPossibleMoves(board, pieces, possibleMoves);

        return possibleMoves;

    }

    private bool TryAddPossibleTakes(List<Piece> pieces, List<MoveInfo> possibleMoves, Board board)
    {
        var enemyPieces = board.GetPieces(Game.GetOppositeSide(pieces.First().Side));
        var possibleTakesCounter = 0;

        foreach (var piece in pieces)
        {
            possibleTakesCounter += AddPossiblePieceTakes(possibleMoves, board, enemyPieces, piece);
        }

        return possibleTakesCounter > 0;
    }

    private int AddPossiblePieceTakes(List<MoveInfo> possibleMoves, Board board, 
        List<Piece> enemyPieces, Piece piece)
    {
        var possibleTakesCounter = 0;
        foreach (var enemyPiece in enemyPieces)
        {
            if (!CanTake(piece, enemyPiece, board, out var takeDestPos))
            {
                continue;
            }

            var possibleTake = MoveInfo.BuildTake(piece, enemyPiece, takeDestPos);
            possibleMoves.Add(possibleTake);
            possibleTakesCounter++;
        }

        return possibleTakesCounter;
    }

    private void AddPossibleMoves(Board board, List<Piece> pieces, List<MoveInfo> possibleMoves)
    {
        foreach (var piece in pieces)
        {
            AddPossiblePieceMoves(piece, possibleMoves, board);
        }
    }

    private void AddPossiblePieceMoves(Piece piece, List<MoveInfo> possibleMoves, Board board)
    {
        var boardSquares = board.GetEmptySquares();
        foreach (var boardSquare in boardSquares)
        {
            if (!CanMoveTo(piece, boardSquare, board))
            {
                continue;
            }

            var possibleMove = MoveInfo.BuildMove(piece, boardSquare);
            possibleMoves.Add(possibleMove);
        }
    }

    private bool CanTake(Piece piece, Piece enemyPiece, Board board, out Vec2Int takeDestPos)
    {
        takeDestPos = default;
        var diffX = enemyPiece.Position.X - piece.Position.X;
        var diffY = enemyPiece.Position.Y - piece.Position.Y;

        var rangeIs1ByDiagonal = Math.Abs(diffX) == 1 && Math.Abs(diffY) == 1;
        if (!rangeIs1ByDiagonal)
        {
            return false;
        }

        var neededEmptyPosition = piece.Position + new Vec2Int(diffX, diffY) * 2;
        if (!board.IsEmptySquare(neededEmptyPosition))
        {
            return false;
        }

        takeDestPos = neededEmptyPosition;
        return true;
    }

    private bool CanMoveTo(Piece piece, Vec2Int boardSquare, Board board)
    {
        if (!board.IsEmptySquare(boardSquare))
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
                && diffY >= 0)
            {
                return false;
            }

            if (piece.Side == Side.Black
                && diffY <= 0)
            {
                return false;
            }
        }

        return true;
    }
}