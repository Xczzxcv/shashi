namespace Core;

public static class DefaultLogger
{
    private static readonly ILogger Logger = new ConsoleLogger();
    
    public static void Log(string logMessage,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        Logger.Log(logMessage, memberName, sourceFilePath, sourceLineNumber);
    }
}