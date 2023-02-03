namespace Core;

public class ConsoleLogger : ILogger
{
    public void Init()
    { }

    public void Log(string logMessage,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0)
    {
        var callerTypeName = Path.GetFileNameWithoutExtension(sourceFilePath);
        Console.WriteLine($"[{callerTypeName}:{memberName}] {logMessage}");
    }
}