using System.Diagnostics;

namespace Core;

public class RulesManager
{
    public MovesCollection GetPossibleSideMoves(Side side, Board board)
    {
        var possibleMoves = PoolsHolder.MovesCollectionPool.Get();
        var pieces = board.GetPieces(side);
        if (!TryAddPossibleTakes(pieces, possibleMoves, board))
        {
            AddPossibleMoves(board, pieces, possibleMoves);
        }

        pieces.ReturnToPool();

        return possibleMoves;
    }

    private bool TryAddPossibleTakes(PiecesCollection pieces, MovesCollection possibleMoves, Board board)
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

    private static readonly Vec2Int[] DiagonalDirections =
    {
        new(-1, -1),
        new(1, -1),
        new(1, 1),
        new(-1, 1)
    };
    private readonly Vec2Int[] _moveDirections = DiagonalDirections;
    private readonly Vec2Int[] _defaultAttackDirections = DiagonalDirections;

    private int AddPossiblePieceTakes(MovesCollection possibleMoves, Piece piece, Board board,
        bool noEnemiesMeansSuccess, Vec2Int[]? attackDirections = null)
    {
        var possibleTakesCounter = 0;
        var enemyPieces = DetectedEnemyPieces(piece, board, attackDirections);
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

        enemyPieces.ReturnToPool();

        return possibleTakesCounter;
    }

    private List<Vec2Int> GetPossibleTakeDestPositions(Piece piece, Piece enemyPiece, Board board)
    {
        var attackDirection = GetAttackDirection(piece, enemyPiece);
        var takeDestPositions = piece.Rank switch
        {
            PieceRank.Checker => GetPossibleTakeDestPositionsForChecker(enemyPiece, attackDirection, board),
            PieceRank.King => GetPossibleTakeDestPositionsForKing(piece, enemyPiece, attackDirection, board),
            _ => throw ThrowHelper.WrongPieceRankException(in piece),
        };

        return takeDestPositions;
    }

    private static Vec2Int GetAttackDirection(Piece piece, Piece enemyPiece)
    {
        var posDiff = enemyPiece.Position - piece.Position;
        var attackDirection = posDiff / Math.Abs(posDiff.X);
        return attackDirection;
    }

    private List<Vec2Int> GetPossibleTakeDestPositionsForChecker(Piece enemyPiece, 
        Vec2Int attackDirection, Board board)
    {
        const int checkerOvertakeRange = 1;
        var takeDestPositions = new List<Vec2Int>();
        for (int overtakeRange = 1; overtakeRange <= checkerOvertakeRange; overtakeRange++)
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

    private List<Vec2Int> GetPossibleTakeDestPositionsForKing(Piece piece, Piece enemyPiece,
        Vec2Int attackDirection, Board board)
    {
        const int kingOvertakeRange = Constants.BOARD_SIZE - 1;
        var hasAnyContinuationTakes = false;
        var takeDestPositionsWithContinuation = new List<Vec2Int>();
        var takeDestPositionsWithoutContinuation = new List<Vec2Int>();
        for (int overtakeRange = 1; overtakeRange <= kingOvertakeRange; overtakeRange++)
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

            var pieceAfterTake = new Piece(piece, takeDestPos);
            PerformSingleTake(piece, enemyPiece, takeDestPos, ref board);

            var oneDir = new Vec2Int(attackDirection.X * -1, attackDirection.Y);
            var otherDir = new Vec2Int(attackDirection.X, attackDirection.Y * -1);
            var attackDirections = new[] {oneDir, otherDir};
            var possibleMoves = PoolsHolder.MovesCollectionPool.Get();
            AddPossiblePieceTakes(possibleMoves, pieceAfterTake, board, false, attackDirections);
            var possibleMovesCount = possibleMoves.Count;
            possibleMoves.ReturnToPool();            
            
            if (possibleMovesCount > 0)
            {
                takeDestPositionsWithContinuation.Add(takeDestPos);
                hasAnyContinuationTakes = true;
            }
            else
            {
                takeDestPositionsWithoutContinuation.Add(takeDestPos);
            }

            RevertSingleTake(piece, pieceAfterTake, enemyPiece, ref board);
        }

        if (!hasAnyContinuationTakes)
        {
            takeDestPositionsWithContinuation.AddRange(takeDestPositionsWithoutContinuation);
        }

        return takeDestPositionsWithContinuation;
    }

    private PiecesCollection DetectedEnemyPieces(Piece piece, Board board, 
        Vec2Int[]? attackDirections = null)
    {
        var maxDetectRange = piece.Rank switch
        {
            PieceRank.Checker => 1,
            PieceRank.King => Constants.BOARD_SIZE - 1,
            _ => throw ThrowHelper.WrongPieceRankException(piece)
        };

        attackDirections ??= _defaultAttackDirections;

        var detectedEnemyPieces = PoolsHolder.PiecesCollectionPool.Get();
        foreach (var attackDirection in attackDirections)
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
                break;
            }
        }

        return detectedEnemyPieces;
    }

    private readonly HashSet<Vec2Int> _takenPiecesPositions = new();
    private readonly List<Take> _takesDone = new();

    private int AddPossiblePieceToPieceTakes(MovesCollection possibleMoves, Piece piece, 
        Piece checkedEnemyPiece, Board board)
    {
        _takenPiecesPositions.Add(checkedEnemyPiece.Position);
        var possibleTakeDestPositions = GetPossibleTakeDestPositions(piece, checkedEnemyPiece, board);
        if (possibleTakeDestPositions.Count == 0)
        {
            _takenPiecesPositions.Remove(checkedEnemyPiece.Position);
            return 0;
        }

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
            RevertSingleTake(piece, pieceAfterTake, checkedEnemyPiece, ref board);
        }

        _takenPiecesPositions.Remove(checkedEnemyPiece.Position);

        return takesCount;
    }

    private Piece PerformSingleTake(Piece piece, Piece enemyPiece, Vec2Int possibleTakeDestPos, 
        ref Board board)
    {
        var pieceRank = ProcessPiecePromotion(piece, possibleTakeDestPos);
        var pieceAfterTake = new Piece(piece.Side, pieceRank, possibleTakeDestPos);
        board.DelSquareContent(piece);
        // board.DelSquareContent(enemyPiece);
        board.SetSquareContent(pieceAfterTake);

        return pieceAfterTake;
    }
    private void RevertSingleTake(Piece oldPiece, Piece currentPiece, Piece enemyPiece, ref Board board)
    {
        board.DelSquareContent(currentPiece);
        board.SetSquareContent(oldPiece);
        // board.SetSquareContent(enemyPiece);
    }

    private void AddPossibleMoves(Board board, PiecesCollection pieces, MovesCollection possibleMoves)
    {
        foreach (var piece in pieces)
        {
            AddPossiblePieceMoves(piece, possibleMoves, board);
        }
    }

    private void AddPossiblePieceMoves(Piece piece, MovesCollection possibleMoves, Board board)
    {
        var maxMoveRange = piece.Rank switch
        {
            PieceRank.Checker=>1,
            PieceRank.King=>Constants.BOARD_SIZE - 1,
            _ => throw ThrowHelper.WrongPieceRankException(piece)
        };
        foreach (var moveDirection in _moveDirections)
        {
            for (int moveRange = 1; moveRange <= maxMoveRange; moveRange++)
            {
                var possibleMoveDestPos = piece.Position + moveDirection * moveRange;
                if (!CanMoveTo(piece, possibleMoveDestPos, board))
                {
                    continue;
                }
                
                var possibleMove = MoveInfo.BuildMove(piece, possibleMoveDestPos);
                possibleMoves.Add(possibleMove);
            }
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

        var diffY = boardSquare.Y - piece.Position.Y;
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
        var attackDirection = GetAttackDirection(piece, enemyPiece);

        var neededEmptyPosition = enemyPiece.Position + attackDirection;
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
    
    public static PieceRank ProcessPiecePromotion(Piece piece, Vec2Int destPos)
    {
        var pieceRank = piece.Side switch
        {
            Side.White when destPos.Y == 0 => PieceRank.King,
            Side.Black when destPos.Y == Constants.BOARD_SIZE - 1 => PieceRank.King,
            _ => piece.Rank
        };
        return pieceRank;
    }
}