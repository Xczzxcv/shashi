namespace Core;

public partial struct Board
{
    public void SetState(ulong whitesState, ulong blacksState)
    {
        _white.SetState(whitesState);
        _black.SetState(blacksState);
    }

    private static bool TryGetSquareContent(char squareInfo, out SquareState squareState)
    {
        switch (squareInfo)
        {
                case EmptyBlackSqrView:
                    squareState = SquareState.Empty;
                    break;
                case WhiteCheckerView:
                    squareState = SquareState.WhiteChecker;
                    break;
                case BlackCheckerView:
                    squareState = SquareState.BlackChecker;
                    break;
                case WhiteKingView:
                    squareState = SquareState.WhiteKing;
                    break;
                case BlackKingView:
                    squareState = SquareState.BlackKing;
                    break;
                default:
                    squareState = default;
                    return false;
        }

        return true;
    }

    public void SetState(string stateStr)
    {
        var index = 0;
        for (var i = 0; i < stateStr.Length && index < Constants.BLACK_BOARD_SQUARES_COUNT; i++)
        {
            var squareInfo = stateStr[i];
            if (!TryGetSquareContent(squareInfo, out var squareState))
            {
                continue;
            }

            if (TryGetPiece(squareState, SideState.GetPos(index), out var piece))
            {
                SetSquareContent(piece);
            }

            index++;
        }
    }
}