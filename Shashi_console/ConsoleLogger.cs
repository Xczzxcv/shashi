using Core;

namespace Shashi_console;

public class ConsoleLogger : ILogger
{
    public void Init()
    { }

    public void Log(string logMessage)
    {
        Console.Write(logMessage);
    }
}