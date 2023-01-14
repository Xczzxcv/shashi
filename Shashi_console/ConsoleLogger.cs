using System.Text;
using Core;

namespace Shashi_console;

public class ConsoleLogger : ILogger
{
    private static FileStream _logFile;

    public void Init()
    {
        _logFile = new FileStream("log.txt", FileMode.Create);
    }

    public void Log(string logMessage)
    {
        var bytes = Encoding.Unicode.GetBytes(logMessage);
        _logFile.Write(bytes);
    }
}