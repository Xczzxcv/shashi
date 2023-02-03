namespace Core;

internal class MockLogger : ILogger
{
    public void Init()
    { }


    public void Log(string logMessage,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0)
    {}
}