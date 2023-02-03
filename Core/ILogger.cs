namespace Core;

public interface ILogger
{
    public void Init();
    public void Log(string logMessage,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0);
}