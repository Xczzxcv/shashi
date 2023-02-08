namespace Core;

public static class DefaultLogger
{
    private static readonly ILogger Logger = new ConsoleLogger();

    private static readonly object LockObject = new();
    
    public static void Log(string logMessage,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        lock (LockObject)
        {
            Logger.Log(logMessage, memberName, sourceFilePath, sourceLineNumber);
        }
    }
}