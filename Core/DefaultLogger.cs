namespace Core;

public static class DefaultLogger
{
    public static void Log(string logMessage,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        var callerTypeName = Path.GetFileNameWithoutExtension(sourceFilePath);
        Console.WriteLine($"[{callerTypeName}:{memberName}] {logMessage}");
    }
}