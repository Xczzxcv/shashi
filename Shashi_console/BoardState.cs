namespace Core;

public struct BoardState
{
    public SideState White;
    public SideState Black;

    public static BoardState Initial()
    {
        var initState = new BoardState
        {
            White = SideState.InitialWhite(),
            Black = SideState.InitialBlack(),
        };

        return initState;
    }

    public static bool IsBlackSquare(int x, int y)
    {
        return IsBlackSquare(new Vec2Int(x, y));
    }

    public static bool IsBlackSquare(Vec2Int pos)
    {
        return pos.Y % 2 == 0
            ? pos.X % 2 == 1
            : pos.X % 2 == 0;
    }

    public string GetView()
    {
        const char whiteSqr = '█';
        const char emptyBlackSqr = '░';
        const char whiteChecker = '0';
        const char blackChecker = '*';
        const char whiteKing = 'W';
        const char blackKing = 'B';
        
        var resultStr = string.Empty;
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            resultStr += $"{Constants.BOARD_SIZE - i}|";
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                var currPos = new Vec2Int(j, i);
                if (!IsBlackSquare(currPos))
                {
                    resultStr += whiteSqr;
                    continue;
                }

                if (White.HasChecker(currPos))
                {
                    resultStr += whiteChecker;
                    continue;
                }
                
                if (White.HasKing(currPos))
                {
                    resultStr += whiteKing;
                    continue;
                }
                
                if (Black.HasChecker(currPos))
                {
                    resultStr += blackChecker;
                    continue;
                }
                
                if (Black.HasKing(currPos))
                {
                    resultStr += blackKing;
                    continue;
                }

                resultStr += emptyBlackSqr;
            }
            resultStr += '\n';
        }

        resultStr += "  ";
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            resultStr += (char)('A' + i);
        }

        return resultStr;
    }

    public List<Vec2Int> GetEmptySquares()
    {
        var emptySquares = new List<Vec2Int>();
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                var currPos = new Vec2Int(j, i);
                if (!IsEmptySquare(currPos))
                {
                    continue;
                }

                emptySquares.Add(currPos);
            }
        }

        return emptySquares;
    }

    public bool IsEmptySquare(Vec2Int pos)
    {
        if (!IsBlackSquare(pos))
        {
            return false;
        }

        if (White.HasChecker(pos))
        {
            return false;
        }

        if (White.HasKing(pos))
        {
            return false;
        }

        if (Black.HasChecker(pos))
        {
            return false;
        }

        if (Black.HasKing(pos))
        {
            return false;
        }

        return true;
    }
}