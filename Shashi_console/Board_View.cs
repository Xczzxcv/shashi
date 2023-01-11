namespace Core;

public partial struct Board
{
    private const char WhiteSqr = '█';
    private const char EmptyBlackSqr = '░';
    private const char WhiteChecker = '0';
    private const char BlackChecker = '*';
    private const char WhiteKing = 'W';
    private const char BlackKing = 'B';

    public string GetView()
    {
        var resultStr = string.Empty;
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            resultStr += $"{Constants.BOARD_SIZE - i}|";
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                var currPos = new Vec2Int(j, i);
                if (!IsBlackSquare(currPos))
                {
                    resultStr += WhiteSqr;
                    continue;
                }

                if (!TryGetPiece(currPos, out var piece))
                {
                    resultStr += EmptyBlackSqr;
                    continue;
                }

                if (piece.Side == Side.White)
                {
                    if (piece.Rank == PieceRank.Checker)
                    {
                        resultStr += WhiteChecker;
                    }
                    else if (piece.Rank == PieceRank.King)
                    {
                        resultStr += WhiteKing;
                    }
                }
                else if (piece.Side == Side.Black)
                {
                    if (piece.Rank == PieceRank.Checker)
                    {
                        resultStr += BlackChecker;
                    }
                    else if (piece.Rank == PieceRank.King)
                    {
                        resultStr += BlackKing;
                    }
                }
            }

            resultStr += '\n';
        }

        resultStr += "  ";
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            resultStr += (char) ('A' + i);
        }

        return resultStr;
    }
}