namespace Core;

public partial struct Board
{
    private const char WhiteSqrView = '█';
    private const char EmptyBlackSqrView = '░';
    private const char WhiteCheckerView = '0';
    private const char BlackCheckerView = '*';
    private const char WhiteKingView = 'W';
    private const char BlackKingView = 'B';

    public readonly string GetView()
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
                    resultStr += WhiteSqrView;
                    continue;
                }

                if (!TryGetPiece(currPos, out var piece))
                {
                    resultStr += EmptyBlackSqrView;
                    continue;
                }

                if (piece.Side == Side.White)
                {
                    if (piece.Rank == PieceRank.Checker)
                    {
                        resultStr += WhiteCheckerView;
                    }
                    else if (piece.Rank == PieceRank.King)
                    {
                        resultStr += WhiteKingView;
                    }
                }
                else if (piece.Side == Side.Black)
                {
                    if (piece.Rank == PieceRank.Checker)
                    {
                        resultStr += BlackCheckerView;
                    }
                    else if (piece.Rank == PieceRank.King)
                    {
                        resultStr += BlackKingView;
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
        resultStr += '\n';

        return resultStr;
    }
}