using System.Diagnostics;

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
        var possibleTakesCounter = 0;
        foreach (var piece in pieces)
        {
            possibleTakesCounter += AddPossiblePieceTakes(possibleMoves, piece, board, false);
            Debug.Assert(!_takenPiecesPositions.Any());
            Debug.Assert(!_takesDone.Any());
        }

        return possibleTakesCounter > 0;
    }

    private readonly Vec2Int[] _attackDirections =
    {
        new(-1, -1),
        new(1, -1),
        new(1, 1),
        new(-1, 1)
    };
    private int AddPossiblePieceTakes(List<MoveInfo> possibleMoves, Piece piece, Board board,
        bool noEnemiesMeansSuccess)
    {
        var enemyPieces = DetectedEnemyPieces(piece, board);

        var possibleTakesCounter = 0;
        foreach (var enemyPiece in enemyPieces)
        {
            if (_takenPiecesPositions.Contains(enemyPiece.Position))
            {
                continue;
            }

            if (!CanTake(piece, enemyPiece, board))
            {
                continue;
            }

            possibleTakesCounter += AddPossiblePieceToPieceTakes(possibleMoves, piece,
                enemyPiece, board);
        }

        if (possibleTakesCounter == 0 && noEnemiesMeansSuccess)
        {
            var takes = new List<Take>(_takesDone);
            var move = MoveInfo.BuildTake(takes);
            possibleMoves.Add(move);
            possibleTakesCounter++;
        }

        return possibleTakesCounter;
    }

    private List<Vec2Int> GetPossibleTakeDestPositions(Piece piece, Piece enemyPiece, Board board)
    {
        var maxOvertakeRange = piece.Rank switch
        {
            PieceRank.Checker => 1,
            PieceRank.King => Constants.BOARD_SIZE - 1,
            _ => throw new ArgumentException($"Unknown piece {piece} rank")
        };

        var posDiff = enemyPiece.Position - piece.Position;
        var attackDirection = posDiff / Math.Abs(posDiff.X);

        var takeDestPositions = new List<Vec2Int>();
        for (int overtakeRange = 1; overtakeRange <= maxOvertakeRange; overtakeRange++)
        {
            var takeDestPos = enemyPiece.Position + attackDirection * overtakeRange;
            if (!Board.IsValidSquare(takeDestPos))
            {
                break;
            }

            if (!board.IsEmptySquare(takeDestPos))
            {
                break;
            }

            takeDestPositions.Add(takeDestPos);
        }

        return takeDestPositions;
    }

    private List<Piece> DetectedEnemyPieces(Piece piece, Board board)
    {
        var maxDetectRange = piece.Rank switch
        {
            PieceRank.Checker => 1,
            PieceRank.King => Constants.BOARD_SIZE - 1,
            _ => throw new ArgumentException($"Unknown piece {piece} rank")
        };

        var detectedEnemyPieces = new List<Piece>();
        foreach (var attackDirection in _attackDirections)
        {
            for (int detectRange = 1; detectRange <= maxDetectRange; detectRange++)
            {
                var detectSquare = piece.Position + attackDirection * detectRange;
                if (!Board.IsValidSquare(detectSquare))
                {
                    break;
                }

                if (!board.TryGetPiece(detectSquare, out var detectedPiece))
                {
                    continue;
                }

                if (detectedPiece.Side == piece.Side)
                {
                    break;
                }

                detectedEnemyPieces.Add(detectedPiece);
            }
        }

        return detectedEnemyPieces;
    }

    private readonly HashSet<Vec2Int> _takenPiecesPositions = new();
    private readonly List<Take> _takesDone = new();

    private int AddPossiblePieceToPieceTakes(List<MoveInfo> possibleMoves, Piece piece, 
        Piece checkedEnemyPiece, Board board)
    {
        var possibleTakeDestPositions = GetPossibleTakeDestPositions(piece, checkedEnemyPiece, board);
        if (!possibleTakeDestPositions.Any())
        {
            return 0;
        }

        _takenPiecesPositions.Add(checkedEnemyPiece.Position);
        var takesCount = 0;
        foreach (var possibleTakeDestPos in possibleTakeDestPositions)
        {
            var pieceAfterTake = PerformSingleTake(piece, checkedEnemyPiece, possibleTakeDestPos, ref board);
            var take = new Take
            {
                SrcPos = piece.Position,
                DestPos = possibleTakeDestPos,
                TakenPiecePos = checkedEnemyPiece.Position,
            };
            _takesDone.Add(take);
            takesCount += AddPossiblePieceTakes(possibleMoves, pieceAfterTake, board, 
                true);
            _takesDone.Remove(take);
        }

        _takenPiecesPositions.Remove(checkedEnemyPiece.Position);

        return takesCount;
    }

    private Piece PerformSingleTake(Piece piece, Piece enemyPiece, Vec2Int possibleTakeDestPos, 
        ref Board board)
    {
        var pieceAfterTake = new Piece(piece.Side, piece.Rank, possibleTakeDestPos);
        board.DelSquareContent(piece);
        board.DelSquareContent(enemyPiece);
        board.SetSquareContent(pieceAfterTake);

        return pieceAfterTake;
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

    private bool CanMoveTo(Piece piece, Vec2Int boardSquare, Board board)
    {
        if (!Board.IsValidSquare(boardSquare))
        {
            return false;
        }

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

    private static bool CanTake(Piece piece, Piece enemyPiece, Board board)
    {
        var diffX = enemyPiece.Position.X - piece.Position.X;
        var diffY = enemyPiece.Position.Y - piece.Position.Y;

        var rangeIs1ByDiagonal = Math.Abs(diffX) == 1 && Math.Abs(diffY) == 1;
        if (!rangeIs1ByDiagonal)
        {
            return false;
        }

        var neededEmptyPosition = piece.Position + new Vec2Int(diffX, diffY) * 2;
        if (!Board.IsValidSquare(neededEmptyPosition))
        {
            return false;
        }

        if (!board.IsEmptySquare(neededEmptyPosition))
        {
            return false;
        }

        return true;
    }
}