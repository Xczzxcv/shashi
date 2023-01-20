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
                case EmptyBlackSqr:
                    squareState = SquareState.Empty;
                    break;
                case WhiteChecker:
                    squareState = SquareState.WhiteChecker;
                    break;
                case BlackChecker:
                    squareState = SquareState.BlackChecker;
                    break;
                case WhiteKing:
                    squareState = SquareState.WhiteKing;
                    break;
                case BlackKing:
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
        for (var i = 0; i < stateStr.Length && index < 32; i++)
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